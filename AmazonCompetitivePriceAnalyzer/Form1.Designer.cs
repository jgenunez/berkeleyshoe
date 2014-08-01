namespace AmznPriceComparator
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
            this.btnUpdateSheet = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnUpdateSheet
            // 
            this.btnUpdateSheet.Location = new System.Drawing.Point(31, 174);
            this.btnUpdateSheet.Name = "btnUpdateSheet";
            this.btnUpdateSheet.Size = new System.Drawing.Size(203, 66);
            this.btnUpdateSheet.TabIndex = 0;
            this.btnUpdateSheet.Text = "Find Amazon Prices";
            this.btnUpdateSheet.UseVisualStyleBackColor = true;
            this.btnUpdateSheet.Click += new System.EventHandler(this.btnUpdateSheet_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.btnUpdateSheet);
            this.Name = "Form1";
            this.Text = "Competitive Price Analysis";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnUpdateSheet;
    }
}

