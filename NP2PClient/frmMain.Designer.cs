namespace NP2PClient
{
    partial class frmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Label lbVersion = null;
        private TextBox tbReadme = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.lbVersion = new Label();
            this.tbReadme = new TextBox();
            this.Text = "NP2P-CLIENT";
            this.lbVersion.Location = new Point(320, 46);
            this.lbVersion.Size = new Size(300, 30);
            this.lbVersion.Text = "程序版本： V1.0";
            this.lbVersion.Font = new Font(new FontFamily("Microsoft YaHei UI"), 9.5f, FontStyle.Bold);
            this.tbReadme.Location = new Point(21, 92);
            this.tbReadme.Multiline = true;
            this.tbReadme.Size = new Size(755, 321);
            this.Controls.Add(this.lbVersion);
            this.Controls.Add(this.tbReadme);
            this.FormClosing += frmMain_FormClosing;
        }

        #endregion
    }
}