namespace MarketplacePublisher
{
    partial class PublisherForm
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
            this.btnPublish = new System.Windows.Forms.Button();
            this.btnUpdateSheet = new System.Windows.Forms.Button();
            this.lbCurrentSheet = new System.Windows.Forms.Label();
            this.btnSetExcelSheet = new System.Windows.Forms.Button();
            this.btnUpdateAmznPrice = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnPublish
            // 
            this.btnPublish.Location = new System.Drawing.Point(236, 74);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(90, 40);
            this.btnPublish.TabIndex = 7;
            this.btnPublish.Text = "Publish";
            this.btnPublish.UseVisualStyleBackColor = true;
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            // 
            // btnUpdateSheet
            // 
            this.btnUpdateSheet.Location = new System.Drawing.Point(12, 74);
            this.btnUpdateSheet.Name = "btnUpdateSheet";
            this.btnUpdateSheet.Size = new System.Drawing.Size(90, 40);
            this.btnUpdateSheet.TabIndex = 6;
            this.btnUpdateSheet.Text = "Update";
            this.btnUpdateSheet.UseVisualStyleBackColor = true;
            this.btnUpdateSheet.Click += new System.EventHandler(this.btnUpdateSheet_Click);
            // 
            // lbCurrentSheet
            // 
            this.lbCurrentSheet.AutoSize = true;
            this.lbCurrentSheet.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbCurrentSheet.Location = new System.Drawing.Point(108, 22);
            this.lbCurrentSheet.Name = "lbCurrentSheet";
            this.lbCurrentSheet.Size = new System.Drawing.Size(32, 18);
            this.lbCurrentSheet.TabIndex = 5;
            this.lbCurrentSheet.Text = "N/A";
            // 
            // btnSetExcelSheet
            // 
            this.btnSetExcelSheet.Location = new System.Drawing.Point(12, 12);
            this.btnSetExcelSheet.Name = "btnSetExcelSheet";
            this.btnSetExcelSheet.Size = new System.Drawing.Size(90, 40);
            this.btnSetExcelSheet.TabIndex = 4;
            this.btnSetExcelSheet.Text = "Set Sheet";
            this.btnSetExcelSheet.UseVisualStyleBackColor = true;
            this.btnSetExcelSheet.Click += new System.EventHandler(this.btnSetExcelSheet_Click);
            // 
            // btnUpdateAmznPrice
            // 
            this.btnUpdateAmznPrice.Location = new System.Drawing.Point(124, 74);
            this.btnUpdateAmznPrice.Name = "btnUpdateAmznPrice";
            this.btnUpdateAmznPrice.Size = new System.Drawing.Size(90, 40);
            this.btnUpdateAmznPrice.TabIndex = 8;
            this.btnUpdateAmznPrice.Text = "Update Price";
            this.btnUpdateAmznPrice.UseVisualStyleBackColor = true;
            this.btnUpdateAmznPrice.Click += new System.EventHandler(this.btnUpdateAmznPrice_Click);
            // 
            // PublisherForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(594, 262);
            this.Controls.Add(this.btnUpdateAmznPrice);
            this.Controls.Add(this.btnPublish);
            this.Controls.Add(this.btnUpdateSheet);
            this.Controls.Add(this.lbCurrentSheet);
            this.Controls.Add(this.btnSetExcelSheet);
            this.Name = "PublisherForm";
            this.Text = "Publisher";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPublish;
        private System.Windows.Forms.Button btnUpdateSheet;
        private System.Windows.Forms.Label lbCurrentSheet;
        private System.Windows.Forms.Button btnSetExcelSheet;
        private System.Windows.Forms.Button btnUpdateAmznPrice;
    }
}

