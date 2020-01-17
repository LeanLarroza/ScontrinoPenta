namespace ScontrinoPenta
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.resetPostazioneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.impostaPagamentiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "ScontrinoPenta";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon1_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.resetPostazioneToolStripMenuItem,
            this.impostaPagamentiToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(178, 70);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
            this.toolStripMenuItem1.Text = "Chiudi";
            this.toolStripMenuItem1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripMenuItem1.Click += new System.EventHandler(this.ToolStripMenuItem1_Click);
            // 
            // resetPostazioneToolStripMenuItem
            // 
            this.resetPostazioneToolStripMenuItem.Name = "resetPostazioneToolStripMenuItem";
            this.resetPostazioneToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.resetPostazioneToolStripMenuItem.Text = "Reset Postazione";
            this.resetPostazioneToolStripMenuItem.Click += new System.EventHandler(this.ResetPostazioneToolStripMenuItem_Click);
            // 
            // impostaPagamentiToolStripMenuItem
            // 
            this.impostaPagamentiToolStripMenuItem.Name = "impostaPagamentiToolStripMenuItem";
            this.impostaPagamentiToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.impostaPagamentiToolStripMenuItem.Text = "Imposta Pagamenti";
            this.impostaPagamentiToolStripMenuItem.Click += new System.EventHandler(this.ImpostaPagamentiToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(120, 0);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.ShowInTaskbar = false;
            this.Text = "Penta-Scontrino";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem resetPostazioneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem impostaPagamentiToolStripMenuItem;
    }
}

