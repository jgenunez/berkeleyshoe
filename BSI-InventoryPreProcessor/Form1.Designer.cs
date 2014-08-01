namespace BSI_InventoryPreProcessor
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtOriginalFile = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnSearchPictures = new System.Windows.Forms.Button();
            this.txtPicturesPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.chkPublishWOPics = new System.Windows.Forms.CheckBox();
            this.btnUpdateMarketplaces = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbMarkets = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eBayPageSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chkOverridePosting = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Original file:";
            // 
            // txtOriginalFile
            // 
            this.txtOriginalFile.Location = new System.Drawing.Point(126, 46);
            this.txtOriginalFile.Name = "txtOriginalFile";
            this.txtOriginalFile.Size = new System.Drawing.Size(609, 26);
            this.txtOriginalFile.TabIndex = 1;
            // 
            // btnSearch
            // 
            this.btnSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearch.Location = new System.Drawing.Point(741, 48);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(116, 23);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnSearchPictures
            // 
            this.btnSearchPictures.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearchPictures.Location = new System.Drawing.Point(741, 81);
            this.btnSearchPictures.Name = "btnSearchPictures";
            this.btnSearchPictures.Size = new System.Drawing.Size(116, 23);
            this.btnSearchPictures.TabIndex = 5;
            this.btnSearchPictures.Text = "Set pictures path";
            this.btnSearchPictures.UseVisualStyleBackColor = true;
            // 
            // txtPicturesPath
            // 
            this.txtPicturesPath.Location = new System.Drawing.Point(126, 78);
            this.txtPicturesPath.Name = "txtPicturesPath";
            this.txtPicturesPath.Size = new System.Drawing.Size(609, 26);
            this.txtPicturesPath.TabIndex = 4;
            this.txtPicturesPath.Text = "P:\\PRODUCTS";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Pictures path:";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(126, 142);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(155, 54);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtStatus
            // 
            this.txtStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStatus.Location = new System.Drawing.Point(12, 202);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatus.Size = new System.Drawing.Size(844, 470);
            this.txtStatus.TabIndex = 8;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // chkPublishWOPics
            // 
            this.chkPublishWOPics.AutoSize = true;
            this.chkPublishWOPics.Enabled = false;
            this.chkPublishWOPics.Location = new System.Drawing.Point(448, 142);
            this.chkPublishWOPics.Name = "chkPublishWOPics";
            this.chkPublishWOPics.Size = new System.Drawing.Size(179, 24);
            this.chkPublishWOPics.TabIndex = 9;
            this.chkPublishWOPics.Text = "Save without pictures";
            this.chkPublishWOPics.UseVisualStyleBackColor = true;
            // 
            // btnUpdateMarketplaces
            // 
            this.btnUpdateMarketplaces.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUpdateMarketplaces.Location = new System.Drawing.Point(448, 106);
            this.btnUpdateMarketplaces.Name = "btnUpdateMarketplaces";
            this.btnUpdateMarketplaces.Size = new System.Drawing.Size(162, 30);
            this.btnUpdateMarketplaces.TabIndex = 10;
            this.btnUpdateMarketplaces.Text = "Update marketplaces";
            this.btnUpdateMarketplaces.UseVisualStyleBackColor = true;
            this.btnUpdateMarketplaces.Visible = false;
            this.btnUpdateMarketplaces.Click += new System.EventHandler(this.btnUpdateMarketplaces_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(126, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 20);
            this.label3.TabIndex = 12;
            this.label3.Text = "Marketplace:";
            // 
            // cmbMarkets
            // 
            this.cmbMarkets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMarkets.FormattingEnabled = true;
            this.cmbMarkets.Location = new System.Drawing.Point(232, 108);
            this.cmbMarkets.Name = "cmbMarkets";
            this.cmbMarkets.Size = new System.Drawing.Size(210, 28);
            this.cmbMarkets.TabIndex = 13;
            this.cmbMarkets.SelectedIndexChanged += new System.EventHandler(this.cmbMarkets_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configurationToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(868, 24);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.eBayPageSizeToolStripMenuItem});
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size(93, 20);
            this.configurationToolStripMenuItem.Text = "Configuration";
            // 
            // eBayPageSizeToolStripMenuItem
            // 
            this.eBayPageSizeToolStripMenuItem.Name = "eBayPageSizeToolStripMenuItem";
            this.eBayPageSizeToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.eBayPageSizeToolStripMenuItem.Text = "eBay Page Size";
            this.eBayPageSizeToolStripMenuItem.Click += new System.EventHandler(this.eBayPageSizeToolStripMenuItem_Click);
            // 
            // chkOverridePosting
            // 
            this.chkOverridePosting.AutoSize = true;
            this.chkOverridePosting.Location = new System.Drawing.Point(448, 172);
            this.chkOverridePosting.Name = "chkOverridePosting";
            this.chkOverridePosting.Size = new System.Drawing.Size(346, 24);
            this.chkOverridePosting.TabIndex = 15;
            this.chkOverridePosting.Text = "Override product information already in tables";
            this.chkOverridePosting.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(868, 684);
            this.Controls.Add(this.chkOverridePosting);
            this.Controls.Add(this.cmbMarkets);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnUpdateMarketplaces);
            this.Controls.Add(this.chkPublishWOPics);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnSearchPictures);
            this.Controls.Add(this.txtPicturesPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtOriginalFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "BSI Inventory Pre Processor";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtOriginalFile;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnSearchPictures;
        private System.Windows.Forms.TextBox txtPicturesPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.CheckBox chkPublishWOPics;
        private System.Windows.Forms.Button btnUpdateMarketplaces;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbMarkets;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eBayPageSizeToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkOverridePosting;
    }
}

