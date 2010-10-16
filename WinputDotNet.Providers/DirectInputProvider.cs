// Copyright (c) 2010, Eric Maupin, Strager Neds
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Microsoft.DirectX.DirectInput;

namespace WinputDotNet.Providers {
    public class DirectInputSequence : IInputSequence {
        private readonly string inputString;

        public string InputString {
            get {
                return this.inputString;
            }
        }

        public DirectInputSequence(string inputString) {
            if (string.IsNullOrWhiteSpace(inputString)) {
                throw new ArgumentNullException("inputString");
            }

            this.inputString = inputString;
        }

        public string HumanString {
            get {
                if (!this.inputString.Contains("|")) {
                    throw new FormatException("Input was in an unrecognized format.");
                }

                string[] parts = this.inputString.Split('|');
                Guid deviceGuid = new Guid(parts[0]);

                return GetNiceInputName(parts[1], new Device(deviceGuid));
            }
        }

        private static string GetNiceInputName(string input, Device device) {
            switch (device.DeviceInformation.DeviceType) {
                case DeviceType.Keyboard:
                    return input;

                case DeviceType.Mouse:
                    return "Mouse " + (Int32.Parse(input) + 1);

                default:
                    string[] parts = input.Split(';');

                    string r = device.Properties.ProductName + " " +
                        device.GetObjectInformation(Int32.Parse(parts[0]), ParameterHow.ByOffset).Name;
                    if (parts.Length == 2) {
                        r += parts[1];
                    }

                    return r;
            }
        }
    }
    
    /// <summary>
    /// DirectX's DirectInput.
    /// </summary>
    /// <remarks>
    /// <![CDATA[
    /// Users of this provider MUST add the following attribute to the startup element in the app.config:
    /// 
    /// useLegacyV2RuntimeActivationPolicy="true"
    /// 
    /// In addition, users MUST add a reference to Microsoft.DirectX.DirectInput.
    /// 
    /// (If anyone knows a way around these restrictions, feel free to change the code!)
    /// ]]>
    /// </remarks>
    [Export(typeof(IInputProvider))]
    public class DirectInputProvider : IInputProvider {
        public event EventHandler<CommandStateChangedEventArgs> CommandStateChanged;

        public string Name {
            get {
                return "DirectInput";
            }
        }

        public void Attach(IntPtr window) {
            if (this.running) {
                throw new InvalidOperationException("Already attached");
            }

            int index = 2;
            this.running = true;

            lock (this.syncRoot) {
                List<AutoResetEvent> resets = new List<AutoResetEvent>();
                foreach (DeviceInstance di in Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly)) {
                    AutoResetEvent reset = new AutoResetEvent(false);

                    var d = new Device(di.InstanceGuid);
                    d.Properties.BufferSize = 10;
                    d.SetCooperativeLevel(window, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                    d.SetDataFormat(DeviceDataFormat.Joystick);
                    d.SetEventNotification(reset);
                    d.Acquire();

                    resets.Add(reset);
                    this.joysticks.Add(di.InstanceGuid, d);
                    this.joystickIndexes.Add(index++, di.InstanceGuid);
                }

                this.waits = new AutoResetEvent[this.joysticks.Count + 2];
                this.waits[0] = new AutoResetEvent(false);
                this.keyboard = new Device(SystemGuid.Keyboard);
                this.keyboard.Properties.BufferSize = 10;
                this.keyboard.SetCooperativeLevel(window, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                this.keyboard.SetDataFormat(DeviceDataFormat.Keyboard);
                this.keyboard.SetEventNotification(this.waits[0]);
                this.keyboard.Acquire();

                this.waits[1] = new AutoResetEvent(false);
                this.mouse = new Device(SystemGuid.Mouse);
                this.mouse.Properties.BufferSize = 10;
                this.mouse.SetCooperativeLevel(window, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
                this.mouse.SetDataFormat(DeviceDataFormat.Mouse);
                this.mouse.SetEventNotification(this.waits[1]);
                this.mouse.Acquire();

                resets.CopyTo(this.waits, 2);
            }

            (this.inputRunnerThread = new Thread(InputRunner) {
                Name = "DirectInput Runner",
                IsBackground = true
            }).Start();
        }

        public void AttachRecorder(IntPtr window, InputRecorder recorder) {
			if (this.running) {
			    throw new InvalidOperationException("Already attached");
			}

            this.recordedSequenceQueue = new BlockingQueue<IInputSequence>();

            (this.recordingHandlerThread = new Thread(RecordingHandler) {
                Name = "Direct Input Recording Runner",
                IsBackground = true
            }).Start();

            this.recording = true;
            this.recordingHandler = recorder;

            Attach(window);
        }

        private void RecordingHandler() {
            IInputSequence input;

            while (this.recordedSequenceQueue != null && this.recordedSequenceQueue.TryDequeue(out input)) {
                var handler = this.recordingHandler;

                if (handler != null) {
                    handler(input);
                }
            }
        }

        public void SetBindings(IEnumerable<ICommandBinding> bindings) {
            if (bindings == null) {
                throw new ArgumentNullException("bindings");
            }

            lock (this.syncRoot) {
                foreach (ICommandBinding binding in bindings) {
                    var inputSequence = (DirectInputSequence) binding.Input;

                    string[] parts = inputSequence.InputString.Split('|');
                    Guid deviceGuid = new Guid(parts[0]);
                    if (deviceGuid == SystemGuid.Keyboard) {
                        string[] keys = parts[1].Split('+');
                        Key[] boundKeys = new Key[keys.Length];

                        for (int i = 0; i < boundKeys.Length; ++i) {
                            boundKeys[i] = (Key) Enum.Parse(typeof(Key), keys[i], true);
                        }

                        this.keyboardBindings.Add(boundKeys, binding.Command);
                    } else if (deviceGuid == SystemGuid.Mouse) {
                        int button;
                        if (!Int32.TryParse(parts[1], out button)) {
                            continue;
                        }

                        this.mouseBindings.Add(button, binding.Command);
                    } else {
                        Dictionary<int, ICommand> jbindings;
                        if (!this.joystickBindings.TryGetValue(deviceGuid, out jbindings)) {
                            this.joystickBindings[deviceGuid] = jbindings = new Dictionary<int, ICommand>();
                        }

                        string[] bindingParts = parts[1].Split(';');
                        jbindings.Add(Int32.Parse(bindingParts[0]), binding.Command);
                    }
                }
            }
        }

        public void Detach() {
            if (!this.running) {
                return;
            }

            this.running = false;
            this.recording = false;

            this.waits[0].Set(); // Keyboard wait always present, forces out of possible deadlock.
            this.inputRunnerThread.Join();
            this.inputRunnerThread = null;

            if (this.recordedSequenceQueue != null) {
                this.recordedSequenceQueue.Cancel();

                if (this.recordingHandlerThread != Thread.CurrentThread) {
                    this.recordingHandlerThread.Join();
                }

                this.recordingHandlerThread = null;
                this.recordedSequenceQueue = null;
            }

            lock (this.syncRoot) {
                if (this.waits != null) {
                    for (int i = 0; i < this.waits.Length; ++i) {
                        this.waits[i].Close();
                    }
                }

                this.mouse.Unacquire();
                this.mouse.Dispose();

                this.keyboard.Unacquire();
                this.keyboard.Dispose();

                foreach (Device d in joysticks.Values) {
                    d.Unacquire();
                    d.Dispose();
                }

                this.mouseBindings.Clear();
                this.keyboardBindings.Clear();
                this.joystickBindings.Clear();

                this.joysticks.Clear();
                this.joystickIndexes.Clear();
                this.waits = null;
            }
        }

        public void Dispose() {
            Detach();
        }

        private bool running;
        private bool recording;

        private readonly Dictionary<Guid, Device> joysticks = new Dictionary<Guid, Device>();
        private readonly Dictionary<int, Guid> joystickIndexes = new Dictionary<int, Guid>();
        private readonly Dictionary<Guid, Dictionary<int, ICommand>> joystickBindings = new Dictionary<Guid, Dictionary<int, ICommand>>();
        private readonly Dictionary<Key[], ICommand> keyboardBindings = new Dictionary<Key[], ICommand>();
        private readonly Dictionary<int, ICommand> mouseBindings = new Dictionary<int, ICommand>();

        private readonly object syncRoot = new object();

        private AutoResetEvent[] waits;

        private Device keyboard;
        private Device mouse;

        private Thread inputRunnerThread;

        private InputRecorder recordingHandler;
        private Thread recordingHandlerThread;
        private BlockingQueue<IInputSequence> recordedSequenceQueue;

        private void OnInputStateChanged(CommandStateChangedEventArgs e) {
            EventHandler<CommandStateChangedEventArgs> handler = CommandStateChanged;
            if (handler != null) {
                handler(this, e);
            }
        }

        private void OnNewRecording(IInputSequence sequence) {
            // Calling the recording handler must be asynchronous.  Else, if
            // the handler calls e.g. Detach, a deadlock will occur.
            // In addition, calls to the handler are in a single thread
            // so only one handler executes at a time, so it appears
            // synchronous to calling code.
            this.recordedSequenceQueue.Enqueue(sequence);
        }

        private void InputRunner() {
            Dictionary<Device, Dictionary<int, int>> objectInitial = new Dictionary<Device, Dictionary<int, int>>();
            Dictionary<Device, Dictionary<int, InputRange>> objectRanges = new Dictionary<Device, Dictionary<int, InputRange>>();
            Dictionary<Device, HashSet<int>> buttons = new Dictionary<Device, HashSet<int>>();

            Key[] modifierKeyValues = new[] {
                Key.LeftControl, Key.RightControl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift
            };
            Key[] keyValues = ((Key[]) Enum.GetValues(typeof(Key))).Except(modifierKeyValues).ToArray();

            lock (this.syncRoot) {
                foreach (Device d in this.joysticks.Values) {
                    objectInitial.Add(d, new Dictionary<int, int>());
                    objectRanges.Add(d, new Dictionary<int, InputRange>());
                    buttons.Add(d, new HashSet<int>());

                    foreach (DeviceObjectInstance o in d.GetObjects(DeviceObjectTypeFlags.Button)) {
                        buttons[d].Add(o.Offset);
                    }
                }
            }

            Dictionary<Key[], bool> keybindingStates = new Dictionary<Key[], bool>();
            Dictionary<int, bool> mousebindingStates = new Dictionary<int, bool>();

            while (this.running) {
                int waited = WaitHandle.WaitAny(this.waits);
                lock (this.syncRoot) {
                    switch (waited) {
                        case 0: // Keyboard
                        {
                            KeyboardState state = this.keyboard.GetCurrentKeyboardState();

                            if (!this.recording && this.keyboardBindings != null) {
                                foreach (var kvp in this.keyboardBindings) {
                                    int match = 0;
                                    for (int i = 0; i < kvp.Key.Length; ++i) {
                                        if (state[kvp.Key[i]]) {
                                            match++;
                                        }
                                    }

                                    bool nowState = (match == kvp.Key.Length);
                                    bool currentState;
                                    if (keybindingStates.TryGetValue(kvp.Key, out currentState)) {
                                        if (nowState != currentState) {
                                            OnInputStateChanged(new CommandStateChangedEventArgs(kvp.Value, (nowState) ? InputState.On : InputState.Off));
                                            keybindingStates[kvp.Key] = nowState;
                                        }
                                    } else if (nowState) {
                                        OnInputStateChanged(new CommandStateChangedEventArgs(kvp.Value, InputState.On));
                                        keybindingStates[kvp.Key] = true;
                                    }
                                }
                            } else {
                                string recording = SystemGuid.Keyboard + "|";
                                for (int i = 0; i < modifierKeyValues.Length; ++i) {
                                    if (state[modifierKeyValues[i]]) {
                                        recording += modifierKeyValues[i] + "+";
                                    }
                                }

                                bool nonModifier = false;
                                for (int i = 0; i < keyValues.Length; ++i) {
                                    if (!state[keyValues[i]]) {
                                        continue;
                                    }

                                    nonModifier = true;
                                    recording += keyValues[i].ToString().ToUpper();
                                    break;
                                }

                                if (nonModifier) {
                                    OnNewRecording(new DirectInputSequence(recording));
                                }
                            }

                            break;
                        }

                        case 1: // Mouse
                        {
                            if (!this.recording && this.mouseBindings.Count == 0) {
                                continue;
                            }

                            byte[] state = this.mouse.CurrentMouseState.GetMouseButtons();

                            if (!this.recording) {
                                for (int i = 0; i < state.Length; ++i) {
                                    ICommand c;
                                    if (!this.mouseBindings.TryGetValue(i, out c)) {
                                        continue;
                                    }

                                    bool newState = (state[i] != 0);
                                    bool currentState;
                                    if (mousebindingStates.TryGetValue(i, out currentState)) {
                                        if (currentState != newState) {
                                            OnInputStateChanged(new CommandStateChangedEventArgs(c, (newState) ? InputState.On : InputState.Off));
                                            mousebindingStates[i] = newState;
                                        }
                                    } else if (newState) {
                                        OnInputStateChanged(new CommandStateChangedEventArgs(c, InputState.On));
                                        mousebindingStates[i] = true;
                                    }
                                }
                            } else {
                                for (int i = 0; i < state.Length; ++i) {
                                    if (state[i] == 0) {
                                        continue;
                                    }

                                    OnNewRecording(new DirectInputSequence(SystemGuid.Mouse + "|" + i));
                                    break;
                                }
                            }

                            break;
                        }

                        default:
                            if (!this.recording && this.joystickBindings.Count == 0) {
                                continue;
                            }

                            var d = this.joysticks[this.joystickIndexes[waited]];
                            var currentButtons = buttons[d];
                            var currentInitials = objectInitial[d];
                            var currentRanges = objectRanges[d];

                            BufferedDataCollection buffer = d.GetBufferedData();
                            if (buffer == null) {
                                continue;
                            }

                            for (int i = 0; i < buffer.Count; i++) {
                                BufferedData bd = buffer[i];

                                if (this.recording) {
                                    int initial;
                                    if (!currentInitials.TryGetValue(bd.Offset, out initial)) {
                                        if (!currentButtons.Contains(bd.Offset)) {
                                            try {
                                                currentRanges.Add(bd.Offset, d.Properties.GetRange(ParameterHow.ByOffset, bd.Offset));
                                                currentInitials[bd.Offset] = bd.Data;
                                            } catch (UnsupportedException) {
                                            }
                                        } else {
                                            OnNewRecording(new DirectInputSequence(String.Format("{0}|{1}", d.DeviceInformation.InstanceGuid, bd.Offset)));
                                        }
                                    } else {
                                        InputRange range = currentRanges[bd.Offset];
                                        int delta = Math.Abs(initial - bd.Data);
                                        if (((float) delta / (range.Max - range.Min)) > 0.25) // >25% change
                                        {
                                            OnNewRecording(new DirectInputSequence(String.Format("{0}|{1};{2}", d.DeviceInformation.InstanceGuid, bd.Offset, ((initial > bd.Data) ? "+" : "-"))));
                                        }
                                    }
                                } else {
                                    Dictionary<int, ICommand> binds;
                                    if (!this.joystickBindings.TryGetValue(d.DeviceInformation.InstanceGuid, out binds) || binds.Count == 0) {
                                        continue;
                                    }

                                    ICommand c;
                                    if (!binds.TryGetValue(bd.Offset, out c)) {
                                        continue;
                                    }

                                    if (currentButtons.Contains(bd.Offset)) {
                                        OnInputStateChanged(new CommandStateChangedEventArgs(c, (bd.Data == 128) ? InputState.On : InputState.Off));
                                    } else {
                                        InputRange range = currentRanges[bd.Offset];

                                        double value = (bd.Data != -1) ? bd.Data : range.Max;
                                        OnInputStateChanged(new CommandStateChangedEventArgs(c, InputState.Axis, (value / (range.Max - range.Min)) * 100));
                                    }
                                }
                            }

                            break;
                    }
                }
            }
        }
    }
}