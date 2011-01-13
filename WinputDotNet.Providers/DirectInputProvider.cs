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
using System.Text.RegularExpressions;
using System.Threading;
using SlimDX.DirectInput;
using Device = SlimDX.DirectInput.Device;
using DeviceType = SlimDX.DirectInput.DeviceType;

namespace WinputDotNet.Providers {
    static class Helpers {
        public static DeviceType GetEffectiveDeviceType(this DeviceInstance deviceInstance) {
            const int deviceTypeMask = 0xFF;

            return (DeviceType) ((int) deviceInstance.Type & deviceTypeMask);
        }
    }

    [Serializable]
    public class DirectInputSequence : IInputSequence {
        private readonly string inputString;

        private readonly static string[] CommonKeys;
        private readonly static Regex[] SpecialKeyExpressions;

        private readonly static DirectInput DirectInput = new DirectInput();

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
                string remainingData;
                var device = GetInputStringDevice(DirectInput, out remainingData);

                return GetNiceInputName(remainingData, device);
            }
        }

        private Device GetInputStringDevice(DirectInput directInput, out string remainingData) {
            if (!this.inputString.Contains("|")) {
                throw new FormatException("Input was in an unrecognized format.");
            }

            string[] parts = this.inputString.Split('|');
            Guid deviceGuid = new Guid(parts[0]);

            remainingData = parts[1];

            return new Joystick(directInput, deviceGuid);
        }

        public bool IsSystem {
            get {
                // TODO
                return false;
            }
        }

        public bool IsCommon {
            get {
                string remainingData;
                var device = GetInputStringDevice(new DirectInput(), out remainingData);

                switch (device.Information.GetEffectiveDeviceType()) {
                    case DeviceType.Mouse:
                        int mouseButton = Int32.Parse(remainingData);

                        // First three mouse buttons are common
                        // (Left, right, middle)
                        return mouseButton >= 0 && mouseButton < 3;

                    case DeviceType.Keyboard:
                        return CommonKeys.Contains(remainingData)
                            || SpecialKeyExpressions.Any((re) => re.IsMatch(remainingData));

                    default:
                        return false;
                }
            }
        }

        private static string GetNiceInputName(string input, Device device) {
            switch (device.Information.GetEffectiveDeviceType()) {
                case DeviceType.Keyboard:
                    return input;

                case DeviceType.Mouse:
                    return "Mouse " + (Int32.Parse(input) + 1);

                default:
                    string[] parts = input.Split(';');

                    string inputName = "";

                    if (device.Properties.ProductName != null) {
                        inputName = device.Properties.ProductName + " ";
                    }

                    var deviceObject = device.GetObjects().First((o) => o.Offset == Int32.Parse(parts[0]));

                    if (deviceObject.Name != null) {
                        inputName += deviceObject.Name;
                    }

                    if (parts.Length == 2) {
                        inputName += parts[1];
                    }

                    return inputName;
            }
        }

        public bool Equals(IInputSequence other) {
            var otherSequence = other as DirectInputSequence;

            if (otherSequence == null) {
                return false;
            }

            return this.inputString == otherSequence.inputString;
        }

        static DirectInputSequence() {
            CommonKeys = (
                "GRAVE,TAB,CAPSLOCK,NUMLOCK,NUMBERLOCK,PAUSE,SCROLL,PRINTSCREEN,SYSRQ," +
                "HOME,PAGEUP,PAGEDOWN,END,DELETE,BACKSPACE,RIGHTWINDOWS," +
                "LEFTWINDOWS,MINUS,EQUALS,LEFTBRACKET,RIGHTBRACKET,SEMICOLON," +
                "APOSTROPHE,COMMA,PERIOD,SLASH,BACKSLASH,ENTER,ESCAPE," +
                "LEFTARROW,RIGHTARROW,UPARROW,DOWNARROW,ADD,DECIMAL,INSERT"
            ).Split(',');

            const RegexOptions opts = RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline;

            SpecialKeyExpressions = new[] {
                new Regex(@"^[A-Z]$", opts),                            // Letter keys (A, B, X, Y)
                new Regex(@"^F[0-9]+$", opts),                          // Function keys (F#)
                new Regex(@"^NUM(BER)?PAD[0-9]$", opts),                // Numpad numbers (NUMPAD#)
                new Regex(@"^NUM(BER)?PAD[A-Z]+", opts),                // Numpad buttons (e.g. NUMPADENTER)
                new Regex(@"^((Left|Right)?Shift\+)?D[0-9]+$", opts),   // Number keys (optionally with shift) (D#)
            };
        }
    }

    class InputRange {
        public float Min { get; set; }
        public float Max { get; set; }
    }
    
    /// <summary>
    /// DirectX's DirectInput.
    /// </summary>
    [Export(typeof(IInputProvider))]
    public class DirectInputProvider : IInputProvider {
        private const CooperativeLevel BackgroundFlags = CooperativeLevel.Background | CooperativeLevel.Nonexclusive;
        private const CooperativeLevel ForegroundFlags = CooperativeLevel.Foreground | CooperativeLevel.Exclusive;

        private static readonly IDictionary<string, Key> BackwardCompatibilityKeyMapping;

        public event EventHandler<CommandStateChangedEventArgs> CommandStateChanged;

        public string Name {
            get {
                return "DirectInput";
            }
        }

        static DirectInputProvider() {
            // Backward compatibility;
            // Microsoft's managed DirectInput wrapper used to be used.
            // SlimDX has slightly different names for some of the enum values,
            // so we provide a mapping in case an old enum name is parsed.

            var unknown = Key.Unknown;

            BackwardCompatibilityKeyMapping = new Dictionary<string, Key>(StringComparer.InvariantCultureIgnoreCase) {
                { "Add", unknown },
                { "Apps", Key.Applications },
                { "Back", unknown },
                { "Capital", unknown },
                { "Circumflex", unknown },
                { "Decimal", unknown },
                { "Divide", unknown },
                { "Down", Key.DownArrow },
                { "Left", Key.LeftArrow },
                { "LeftMenu", Key.LeftAlt },
                { "LeftWindows", Key.LeftWindowsKey },
                { "Multiply", unknown },
                { "Next", unknown },
                { "Numlock", Key.NumberLock },
                { "NumPad0", Key.NumberPad0 },
                { "NumPad1", Key.NumberPad1 },
                { "NumPad2", Key.NumberPad2 },
                { "NumPad3", Key.NumberPad3 },
                { "NumPad4", Key.NumberPad4 },
                { "NumPad5", Key.NumberPad5 },
                { "NumPad6", Key.NumberPad6 },
                { "NumPad7", Key.NumberPad7 },
                { "NumPad8", Key.NumberPad8 },
                { "NumPad9", Key.NumberPad9 },
                { "NumPadComma", Key.NumberPadComma },
                { "NumPadEnter", Key.NumberPadEnter },
                { "NumPadEquals", Key.NumberPadEquals },
                { "NumPadMinus", Key.NumberPadMinus },
                { "NumPadPeriod", Key.NumberPadPeriod },
                { "NumPadPlus", Key.NumberPadPlus },
                { "NumPadSlash", Key.NumberPadSlash },
                { "NumPadStar", Key.NumberPadStar },
                { "OEM102", Key.Oem102 },
                { "PrevTrack", Key.PreviousTrack },
                { "Prior", unknown },
                { "Right", Key.RightArrow },
                { "RightMenu", Key.RightAlt },
                { "RightWindows", Key.RightWindowsKey },
                { "Scroll", Key.ScrollLock },
                { "SemiColon", Key.Semicolon },
                { "Subtract", unknown },
                { "SysRq", Key.PrintScreen },
                { "Up", Key.UpArrow },
            };
        }

        public void Attach() {
            AttachImpl(IntPtr.Zero, BackgroundFlags);
        }

        public void Attach(IntPtr window) {
            if (window == IntPtr.Zero) {
                throw new ArgumentException("Window cannot be null", "window");
            }

            AttachImpl(window, ForegroundFlags);
        }

        private void AttachImpl(IntPtr window, CooperativeLevel cooperativeLevelFlags) {
            if (this.running) {
                throw new InvalidOperationException("Already attached");
            }

            int index = 2;
            this.running = true;

            lock (this.syncRoot) {
                List<AutoResetEvent> resets = new List<AutoResetEvent>();
                foreach (DeviceInstance di in DirectInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly)) {
                    AutoResetEvent reset = new AutoResetEvent(false);

                    var d = new Joystick(DirectInput, di.InstanceGuid);
                    d.Properties.BufferSize = 10;
                    d.SetCooperativeLevel(window, cooperativeLevelFlags);
                    d.SetNotification(reset);
                    d.Acquire();

                    resets.Add(reset);
                    this.joysticks.Add(di.InstanceGuid, d);
                    this.joystickIndexes.Add(index++, di.InstanceGuid);
                }

                this.waits = new AutoResetEvent[this.joysticks.Count + 2];
                this.waits[0] = new AutoResetEvent(false);
                this.keyboard = new Keyboard(DirectInput);
                this.keyboard.Properties.BufferSize = 10;
                this.keyboard.SetCooperativeLevel(window, cooperativeLevelFlags);
                this.keyboard.SetNotification(this.waits[0]);
                this.keyboard.Acquire();

                this.waits[1] = new AutoResetEvent(false);
                this.mouse = new Mouse(DirectInput);
                this.mouse.Properties.BufferSize = 10;
                this.mouse.SetCooperativeLevel(window, cooperativeLevelFlags);
                this.mouse.SetNotification(this.waits[1]);
                this.mouse.Acquire();

                resets.CopyTo(this.waits, 2);

                this.mouseBindings.Clear();
                this.keyboardBindings.Clear();
                this.joystickBindings.Clear();

                foreach (var binding in this.allBindings) {
                    ParseBinding(binding.Key, binding.Value);
                }
            }

            (this.inputRunnerThread = new Thread(InputRunner) {
                Name = "DirectInput Runner",
                IsBackground = true
            }).Start();
        }

        public void AttachRecorder(InputRecorder recorder) {
            AttachRecorderImpl(IntPtr.Zero, recorder, BackgroundFlags);
        }

        public void AttachRecorder(IntPtr window, InputRecorder recorder) {
            AttachRecorderImpl(window, recorder, ForegroundFlags);
        }

        private void AttachRecorderImpl(IntPtr window, InputRecorder recorder, CooperativeLevel cooperativeLevelFlags) {
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

            AttachImpl(window, cooperativeLevelFlags);
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
                this.allBindings.Clear();

                foreach (ICommandBinding binding in bindings) {
                    if (binding.Input == null) {
                        throw new ArgumentException("Null input sequence in bindings collection", "bindings");
                    }

                    var inputSequence = binding.Input as DirectInputSequence;

                    if (inputSequence == null) {
                        throw new ArgumentException(string.Format("Unknown input sequence of type '{0}'", binding.Input.GetType()), "bindings");
                    }

                    this.allBindings[inputSequence] = binding.Command;
                }
            }
        }

        private void ParseBinding(DirectInputSequence inputSequence, ICommand command) {
            string[] parts = inputSequence.InputString.Split('|');
            Guid deviceGuid = new Guid(parts[0]);
            if (deviceGuid == this.keyboard.Information.InstanceGuid) {
                string[] keys = parts[1].Split('+');
                Key[] boundKeys = new Key[keys.Length];

                for (int i = 0; i < boundKeys.Length; ++i) {
                    boundKeys[i] = GetKeyFromName(keys[i]);
                }

                this.keyboardBindings.Add(boundKeys, command);
            } else if (deviceGuid == this.mouse.Information.InstanceGuid) {
                int button;
                if (!Int32.TryParse(parts[1], out button)) {
                    return;
                }

                this.mouseBindings.Add(button, command);
            } else {
                Dictionary<int, ICommand> jbindings;
                if (!this.joystickBindings.TryGetValue(deviceGuid, out jbindings)) {
                    this.joystickBindings[deviceGuid] = jbindings = new Dictionary<int, ICommand>();
                }

                string[] bindingParts = parts[1].Split(';');
                jbindings.Add(Int32.Parse(bindingParts[0]), command);
            }
        }

        private static Key GetKeyFromName(string keyName) {
            Key compatibleKey;

            if (BackwardCompatibilityKeyMapping.TryGetValue(keyName, out compatibleKey)) {
                return compatibleKey;
            }

            return (Key) Enum.Parse(typeof(Key), keyName, true);
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

        private readonly Dictionary<Guid, Joystick> joysticks = new Dictionary<Guid, Joystick>();
        private readonly Dictionary<int, Guid> joystickIndexes = new Dictionary<int, Guid>();

        private readonly Dictionary<DirectInputSequence, ICommand> allBindings = new Dictionary<DirectInputSequence, ICommand>();
        private readonly Dictionary<Guid, Dictionary<int, ICommand>> joystickBindings = new Dictionary<Guid, Dictionary<int, ICommand>>();
        private readonly Dictionary<Key[], ICommand> keyboardBindings = new Dictionary<Key[], ICommand>();
        private readonly Dictionary<int, ICommand> mouseBindings = new Dictionary<int, ICommand>();

        private readonly object syncRoot = new object();

        private AutoResetEvent[] waits;

        private Keyboard keyboard;
        private Mouse mouse;

        private Thread inputRunnerThread;

        private InputRecorder recordingHandler;
        private Thread recordingHandlerThread;
        private BlockingQueue<IInputSequence> recordedSequenceQueue;
        private readonly static DirectInput DirectInput = new DirectInput();

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

                    foreach (DeviceObjectInstance o in d.GetObjects(ObjectDeviceType.Button)) {
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
                            KeyboardState state = this.keyboard.GetCurrentState();

                            if (!this.recording && this.keyboardBindings != null) {
                                foreach (var kvp in this.keyboardBindings) {
                                    int match = 0;
                                    for (int i = 0; i < kvp.Key.Length; ++i) {
                                        if (state.IsPressed(kvp.Key[i])) {
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
                                string recording = this.keyboard.Information.InstanceGuid + "|";
                                for (int i = 0; i < modifierKeyValues.Length; ++i) {
                                    if (state.IsPressed(modifierKeyValues[i])) {
                                        recording += modifierKeyValues[i] + "+";
                                    }
                                }

                                bool nonModifier = false;
                                for (int i = 0; i < keyValues.Length; ++i) {
                                    if (!state.IsPressed(keyValues[i])) {
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

                            bool[] state = this.mouse.GetCurrentState().GetButtons();

                            if (!this.recording) {
                                for (int i = 0; i < state.Length; ++i) {
                                    ICommand c;
                                    if (!this.mouseBindings.TryGetValue(i, out c)) {
                                        continue;
                                    }

                                    bool newState = state[i];
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
                                    if (state[i] == false) {
                                        continue;
                                    }

                                    OnNewRecording(new DirectInputSequence(this.mouse.Information.InstanceGuid + "|" + i));
                                    break;
                                }
                            }

                            break;
                        }

                        default:
                            if (!this.recording && this.joystickBindings.Count == 0) {
                                continue;
                            }

                            // TODO Joystick support
                            /*
                            var d = this.joysticks[this.joystickIndexes[waited]];
                            var currentButtons = buttons[d];
                            var currentInitials = objectInitial[d];
                            var currentRanges = objectRanges[d];

                            IList<JoystickState> buffer = d.GetBufferedData();
                            if (buffer == null) {
                                continue;
                            }

                            for (int i = 0; i < buffer.Count; i++) {
                                JoystickState bd = buffer[i];

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

                            */

                            break;
                    }
                }
            }
        }
    }
}