namespace ScontrinoPenta
{
    partial class ErroreStampa
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErroreStampa));
            this.Label1 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.Buttonexito = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.Label1.AutoEllipsis = true;
            this.Label1.BackColor = System.Drawing.Color.RoyalBlue;
            this.Label1.Font = new System.Drawing.Font("Franklin Gothic Medium Cond", 45F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(0, 0);
            this.Label1.Margin = new System.Windows.Forms.Padding(0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(490, 117);
            this.Label1.TabIndex = 27;
            this.Label1.Text = "ATTENDERE";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label2
            // 
            this.Label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.Label2.AutoEllipsis = true;
            this.Label2.BackColor = System.Drawing.Color.White;
            this.Label2.Font = new System.Drawing.Font("Lucida Sans", 19.2F);
            this.Label2.Location = new System.Drawing.Point(49, 182);
            this.Label2.Margin = new System.Windows.Forms.Padding(0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(392, 78);
            this.Label2.TabIndex = 28;
            this.Label2.Text = "ERRORE STAMPA\r\nCONTROLLARE STAMPANTE\r\n\r\n";
            this.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Buttonexito
            // 
            this.Buttonexito.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Buttonexito.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(45)))), ((int)(((byte)(255)))));
            this.Buttonexito.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.Buttonexito.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(45)))), ((int)(((byte)(255)))));
            this.Buttonexito.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(45)))), ((int)(((byte)(255)))));
            this.Buttonexito.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Buttonexito.Font = new System.Drawing.Font("Century Gothic", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Buttonexito.ForeColor = System.Drawing.Color.Black;
            this.Buttonexito.Location = new System.Drawing.Point(161, 325);
            this.Buttonexito.Name = "Buttonexito";
            this.Buttonexito.Size = new System.Drawing.Size(168, 89);
            this.Buttonexito.TabIndex = 29;
            this.Buttonexito.TabStop = false;
            this.Buttonexito.Text = "OK";
            this.Buttonexito.UseVisualStyleBackColor = false;
            this.Buttonexito.Click += new System.EventHandler(this.Buttonexito_Click);
            // 
            // ErroreStampa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(490, 455);
            this.Controls.Add(this.Buttonexito);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.Label2);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ErroreStampa";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "StampaScontrino";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.StampaScontrino_Load);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Button Buttonexito;
    }
}