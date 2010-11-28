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
            this.recordLog = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
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
            this.log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.log.Location = new System.Drawing.Point(0, 0);
            this.log.Multiline = true;
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(207, 384);
            this.log.TabIndex = 1;
            // 
            // inputProviderList
            // 
            this.inputProviderList.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.inputProviderList.FormattingEnabled = true;
            this.inputProviderList.Location = new System.Drawing.Point(12, 12);
            this.inputProviderList.Name = "inputProviderList";
            this.inputProviderList.Size = new System.Drawing.Size(446, 24);
            this.inputProviderList.TabIndex = 2;
            this.inputProviderList.SelectedIndexChanged += new System.EventHandler(this.InputProviderSelected);
            // 
            // recordLog
            // 
            this.recordLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recordLog.Location = new System.Drawing.Point(0, 0);
            this.recordLog.Multiline = true;
            this.recordLog.Name = "recordLog";
            this.recordLog.Size = new System.Drawing.Size(235, 384);
            this.recordLog.TabIndex = 3;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 86);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.log);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.recordLog);
            this.splitContainer1.Size = new System.Drawing.Size(446, 384);
            this.splitContainer1.SplitterDistance = 207;
            this.splitContainer1.TabIndex = 4;
            // 
            // Tester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 482);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.inputProviderList);
            this.Controls.Add(this.recordButton);
            this.Name = "Tester";
            this.Text = "WinputDotNet Tester GUI";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button recordButton;
        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.ComboBox inputProviderList;
        private System.Windows.Forms.TextBox recordLog;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}

