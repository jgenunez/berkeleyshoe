namespace POConfirmation
{
    partial class POConfirmationForm
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
            this.tbPONumber = new System.Windows.Forms.TextBox();
            this.lbPONumber = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbPONumber
            // 
            this.tbPONumber.Location = new System.Drawing.Point(99, 45);
            this.tbPONumber.Name = "tbPONumber";
            this.tbPONumber.Size = new System.Drawing.Size(100, 20);
            this.tbPONumber.TabIndex = 0;
            // 
            // lbPONumber
            // 
            this.lbPONumber.AutoSize = true;
            this.lbPONumber.Location = new System.Drawing.Point(25, 48);
            this.lbPONumber.Name = "lbPONumber";
            this.lbPONumber.Size = new System.Drawing.Size(68, 13);
            this.lbPONumber.TabIndex = 1;
            this.lbPONumber.Text = "PO Number :";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(225, 43);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 2;
            this.btnConfirm.Text = "Confirm";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // POConfirmationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 89);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.lbPONumber);
            this.Controls.Add(this.tbPONumber);
            this.Name = "POConfirmationForm";
            this.Text = "Generate PO Confirmation";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbPONumber;
        private System.Windows.Forms.Label lbPONumber;
        private System.Windows.Forms.Button btnConfirm;
    }
}

