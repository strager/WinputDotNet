using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows.Forms;
using WinputDotNet.Providers;

namespace WinputDotNet.TesterGUI {
    public partial class Tester : Form {
#pragma warning disable 649 // Field is never assigned
        [ImportMany]
        private IEnumerable<IInputProvider> inputProviders;
#pragma warning restore 649

        private IInputProvider activeProvider;

        public Tester() {
            InitializeComponent();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(DirectInputProvider).Assembly));

            var container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);

            this.inputProviderList.Items.AddRange(inputProviders.Select((ip) => ip.Name).ToArray());
        }

        private void InputProviderSelected(object sender, EventArgs e) {
            this.activeProvider = this.inputProviders.ElementAt(this.inputProviderList.SelectedIndex);

            Log("Selected input provider: {0}", this.activeProvider.Name);
        }

        private void Record(object sender, EventArgs e) {
            if (this.activeProvider == null) {
                throw new InvalidOperationException("Can't record without an input provider");
            }

            Log("Starting recording");

            this.activeProvider.AttachRecorder(Handle, (inputSequence) => {
                Log("Got sequence: {0}", inputSequence.GetHumanString());

                Log("Stopping recording");

                this.activeProvider.Detach();
            });
        }

        private void Log(string format, params object[] args) {
            if (InvokeRequired) {
                Invoke((Action) (() => Log(format, args)));

                return;
            }

            string message = string.Format(format, args);

            this.log.AppendText(string.Format("{0}: {1}", DateTime.Now, message));
            this.log.AppendText(Environment.NewLine);
        }
    }
}
