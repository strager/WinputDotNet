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

namespace WinputDotNet {
    public delegate void InputRecorder(IInputSequence input);

    /// <summary>
    /// Contract for input providers
    /// </summary>
    public interface IInputProvider : INamedComponent, IDisposable {
        /// <summary>
        /// Fired when a bound command state has changed.
        /// </summary>
        event EventHandler<CommandStateChangedEventArgs> CommandStateChanged;

        /// <summary>
        /// Attaches the input provider to the given <paramref name="window"/>.
        /// </summary>
        /// <param name="window">Application window handle.</param>
        /// <exception cref="ArgumentException">If <paramref name="window"/> is equal to <see cref="IntPtr.Zero"/>.</exception>
        /// <exception cref="InvalidOperationException">If the input provider is already attached to <paramref name="window"/>.</exception>
        void Attach(IntPtr window);

        void Attach();

        void AttachRecorder(IntPtr window, InputRecorder recorder);
        void AttachRecorder(InputRecorder recorder);

        /// <summary>
        /// Sets the bindings to listen for.
        /// </summary>
        /// <remarks>
        /// Any previously set bindings are un-bound before new bindings are added.
        /// </remarks>
        /// <param name="bindings"></param>
        /// <exception cref="ArgumentNullException"><paramref name="bindings"/> is <c>null</c>.</exception>
        void SetBindings(IEnumerable<ICommandBinding> bindings);

        /// <summary>
        /// Shuts down and detaches the input provider.
        /// </summary>
        void Detach();
    }
}