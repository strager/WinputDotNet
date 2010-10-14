namespace WinputDotNet {
    public sealed class CommandBinding : ICommandBinding {
        private readonly IInputSequence input;
        private readonly ICommand command;

        public IInputSequence Input {
            get {
                return this.input;
            }
        }

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public CommandBinding(IInputSequence input, ICommand command) {
            this.input = input;
            this.command = command;
        }
    }
}