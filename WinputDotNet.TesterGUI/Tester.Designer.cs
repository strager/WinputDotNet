namespace WinputDotNet.TesterGUI
{
    partial class Tester
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.recordButton = new System.Windows.Forms.Button();
            this.log = new System.Windows.Forms.TextBox();
            this.inputProviderList = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // recordButton
            // 
            this.recordButton.Location = new System.Drawing.Point(12, 42);
            this.recordButton.Name = "recordButton";
            this.recordButton.Size = new System.Drawing.Size(93, 38);
            this.recordButton.TabIndex = 0;
            this.recordButton.Text = "Record";
            this.recordButton.UseVisualStyleBackColor = true;
            this.recordButton.Click += new System.EventHandler(this.Record);
            // 
            // log
            // 
            this.log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.log.Location = new System.Drawing.Point(12, 86);
            this.log.Multiline = true;
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(441, 208);
            this.log.TabIndex = 1;
            // 
            // inputProviderList
            // 
            this.inputProviderList.FormattingEnabled = true;
            this.inputProviderList.Location = new System.Drawing.Point(12, 12);
            this.inputProviderList.Name = "inputProviderList";
            this.inputProviderList.Size = new System.Drawing.Size(441, 24);
            this.inputProviderList.TabIndex = 2;
            this.inputProviderList.SelectedIndexChanged += new System.EventHandler(this.InputProviderSelected);
            // 
            // Tester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 306);
            this.Controls.Add(this.inputProviderList);
            this.Controls.Add(this.log);
            this.Controls.Add(this.recordButton);
            this.Name = "Tester";
            this.Text = "WinputDotNet Tester GUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button recordButton;
        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.ComboBox inputProviderList;
    }
}

