namespace allatkezelo_kliens
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
            this.btnHullok = new System.Windows.Forms.Button();
            this.btnHalak = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // btnHullok
            // 
            this.btnHullok.Location = new System.Drawing.Point(12, 12);
            this.btnHullok.Name = "btnHullok";
            this.btnHullok.Size = new System.Drawing.Size(75, 23);
            this.btnHullok.TabIndex = 0;
            this.btnHullok.Text = "Hüllők";
            this.btnHullok.UseVisualStyleBackColor = true;
            this.btnHullok.Click += new System.EventHandler(this.btnHullok_Click);
            // 
            // btnHalak
            // 
            this.btnHalak.Location = new System.Drawing.Point(93, 12);
            this.btnHalak.Name = "btnHalak";
            this.btnHalak.Size = new System.Drawing.Size(75, 23);
            this.btnHalak.TabIndex = 1;
            this.btnHalak.Text = "Halak";
            this.btnHalak.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(0, 41);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1021, 628);
            this.panel1.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1019, 667);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnHalak);
            this.Controls.Add(this.btnHullok);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnHullok;
        private System.Windows.Forms.Button btnHalak;
        private System.Windows.Forms.Panel panel1;
    }
}

