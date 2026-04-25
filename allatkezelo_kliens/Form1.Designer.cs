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
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.hullokUC1 = new allatkezelo_kliens.hullokUC();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.halakUC1 = new allatkezelo_kliens.halakUC();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1776, 1120);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.hullokUC1);
            this.tabPage1.Location = new System.Drawing.Point(8, 45);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage1.Size = new System.Drawing.Size(1760, 1067);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Hüllők";
            this.tabPage1.UseVisualStyleBackColor = true;
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
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.halakUC1);
            this.tabPage2.Location = new System.Drawing.Point(8, 45);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage2.Size = new System.Drawing.Size(1760, 1067);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Halak";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // halakUC1
            // 
            this.halakUC1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.halakUC1.Location = new System.Drawing.Point(4, 5);
            this.halakUC1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.halakUC1.Name = "halakUC1";
            this.halakUC1.Size = new System.Drawing.Size(1752, 1057);
            this.halakUC1.TabIndex = 0;
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
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private hullokUC hullokUC1;
        private System.Windows.Forms.TabPage tabPage2;
        private halakUC halakUC1;
    }
}

