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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.hullokPage = new System.Windows.Forms.TabPage();
            this.hullokUC1 = new allatkezelo_kliens.hullokUC();
            this.halakPage = new System.Windows.Forms.TabPage();
            this.halakUC1 = new allatkezelo_kliens.halakUC();
            this.foglalasokPage = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.hullokPage.SuspendLayout();
            this.halakPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.hullokPage);
            this.tabControl1.Controls.Add(this.halakPage);
            this.tabControl1.Controls.Add(this.foglalasokPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1776, 1120);
            this.tabControl1.TabIndex = 3;
            // 
            // hullokPage
            // 
            this.hullokPage.Controls.Add(this.hullokUC1);
            this.hullokPage.Location = new System.Drawing.Point(8, 45);
            this.hullokPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.hullokPage.Name = "hullokPage";
            this.hullokPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.hullokPage.Size = new System.Drawing.Size(1760, 1067);
            this.hullokPage.TabIndex = 0;
            this.hullokPage.Text = "Hüllők";
            this.hullokPage.UseVisualStyleBackColor = true;
            // 
            // hullokUC1
            // 
            this.hullokUC1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hullokUC1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.hullokUC1.Location = new System.Drawing.Point(4, 5);
            this.hullokUC1.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.hullokUC1.Name = "hullokUC1";
            this.hullokUC1.Size = new System.Drawing.Size(1752, 1057);
            this.hullokUC1.TabIndex = 0;
            // 
            // halakPage
            // 
            this.halakPage.Controls.Add(this.halakUC1);
            this.halakPage.Location = new System.Drawing.Point(8, 45);
            this.halakPage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.halakPage.Name = "halakPage";
            this.halakPage.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.halakPage.Size = new System.Drawing.Size(1760, 1067);
            this.halakPage.TabIndex = 1;
            this.halakPage.Text = "Halak";
            this.halakPage.UseVisualStyleBackColor = true;
            // 
            // halakUC1
            // 
            this.halakUC1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.halakUC1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.halakUC1.Location = new System.Drawing.Point(4, 5);
            this.halakUC1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.halakUC1.Name = "halakUC1";
            this.halakUC1.Size = new System.Drawing.Size(1752, 1057);
            this.halakUC1.TabIndex = 0;
            this.halakUC1.Load += new System.EventHandler(this.halakUC1_Load);
            // 
            // foglalasokPage
            // 
            this.foglalasokPage.Location = new System.Drawing.Point(8, 45);
            this.foglalasokPage.Name = "foglalasokPage";
            this.foglalasokPage.Size = new System.Drawing.Size(1760, 1067);
            this.foglalasokPage.TabIndex = 2;
            this.foglalasokPage.Text = "Foglalások";
            this.foglalasokPage.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1776, 1120);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabControl1.ResumeLayout(false);
            this.hullokPage.ResumeLayout(false);
            this.halakPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage hullokPage;
        private hullokUC hullokUC1;
        private System.Windows.Forms.TabPage halakPage;
        private halakUC halakUC1;
        private System.Windows.Forms.TabPage foglalasokPage;
    }
}

