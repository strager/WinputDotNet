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
        // TODO Rename
        string GetHumanString();
    }

    public interface ICommand {
    }
}