namespace BSI_InventoryPreProcessor
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.cbMarketplaces = new System.Windows.Forms.ComboBox();
            this.lbCurrentWorkbook = new System.Windows.Forms.Label();
            this.btnSetWorkbook = new System.Windows.Forms.Button();
            this.tcMarketplaceSheet = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // btnPublish
            // 
            this.btnPublish.Location = new System.Drawing.Point(585, 87);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(145, 28);
            this.btnPublish.TabIndex = 6;
            this.btnPublish.Text = "Publish";
            this.btnPublish.UseVisualStyleBackColor = true;
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // cbMarketplaces
            // 
            this.cbMarketplaces.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMarketplaces.FormattingEnabled = true;
            this.cbMarketplaces.Location = new System.Drawing.Point(420, 87);
            this.cbMarketplaces.Name = "cbMarketplaces";
            this.cbMarketplaces.Size = new System.Drawing.Size(150, 28);
            this.cbMarketplaces.TabIndex = 13;
            // 
            // lbCurrentWorkbook
            // 
            this.lbCurrentWorkbook.AutoSize = true;
            this.lbCurrentWorkbook.Location = new System.Drawing.Point(148, 26);
            this.lbCurrentWorkbook.Name = "lbCurrentWorkbook";
            this.lbCurrentWorkbook.Size = new System.Drawing.Size(0, 20);
            this.lbCurrentWorkbook.TabIndex = 14;
            // 
            // btnSetWorkbook
            // 
            this.btnSetWorkbook.Location = new System.Drawing.Point(12, 12);
            this.btnSetWorkbook.Name = "btnSetWorkbook";
            this.btnSetWorkbook.Size = new System.Drawing.Size(145, 28);
            this.btnSetWorkbook.TabIndex = 15;
            this.btnSetWorkbook.Text = "Set Workbook";
            this.btnSetWorkbook.UseVisualStyleBackColor = true;
            this.btnSetWorkbook.Click += new System.EventHandler(this.btnSetWorkbook_Click);
            // 
            // tcMarketplaceSheet
            // 
            this.tcMarketplaceSheet.Location = new System.Drawing.Point(12, 121);
            this.tcMarketplaceSheet.Name = "tcMarketplaceSheet";
            this.tcMarketplaceSheet.SelectedIndex = 0;
            this.tcMarketplaceSheet.Size = new System.Drawing.Size(718, 153);
            this.tcMarketplaceSheet.TabIndex = 16;
            // 
            // PublisherForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 684);
            this.Controls.Add(this.tcMarketplaceSheet);
            this.Controls.Add(this.btnSetWorkbook);
            this.Controls.Add(this.lbCurrentWorkbook);
            this.Controls.Add(this.cbMarketplaces);
            this.Controls.Add(this.btnPublish);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "PublisherForm";
            this.Text = "Marketplace Publisher";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPublish;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ComboBox cbMarketplaces;
        private System.Windows.Forms.Label lbCurrentWorkbook;
        private System.Windows.Forms.Button btnSetWorkbook;
        private System.Windows.Forms.TabControl tcMarketplaceSheet;
    }
}

