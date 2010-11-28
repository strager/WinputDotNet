using System;

namespace WinputDotNet {
    public interface ICommandBinding {
        IInputSequence Input { get; }

        ICommand Command { get; }
    }

    public interface IInputSequence : IEquatable<IInputSequence> {
        string HumanString { get; }

        bool IsSystem { get; }
        bool IsCommon { get; }
    }

    public interface ICommand {
    }
}