namespace WinputDotNet {
    public interface ICommandBinding {
        IInputSequence Input {
            get;
        }

        ICommand Command {
            get;
        }
    }

    public interface IInputSequence {
        string HumanString {
            get;
        }
    }

    public interface ICommand {
    }
}