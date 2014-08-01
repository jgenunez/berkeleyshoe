using BerkeleyEntities;
namespace AmazonPublisher
{
    partial class AmazonPublishing
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.labelPath = new System.Windows.Forms.Label();
            this.btnPublish = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.cbMarketplace = new System.Windows.Forms.ComboBox();
            this.dgvResult = new System.Windows.Forms.DataGridView();
            this.PurchaseOrder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Brand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemLookupCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.priceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Confirmed = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ErrorCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.titleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bsiquantitiesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnPending = new System.Windows.Forms.Button();
            this.dgvError = new System.Windows.Forms.DataGridView();
            this.submissionTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.errorMessageDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.messageNumberDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.submissionIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bsiquantitiesmessageBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnRepublish = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsiquantitiesBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvError)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsiquantitiesmessageBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPath.Location = new System.Drawing.Point(144, 19);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(0, 20);
            this.labelPath.TabIndex = 2;
            // 
            // btnPublish
            // 
            this.btnPublish.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPublish.Location = new System.Drawing.Point(544, 4);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(110, 32);
            this.btnPublish.TabIndex = 3;
            this.btnPublish.Text = "Publish";
            this.btnPublish.UseVisualStyleBackColor = true;
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfirm.Location = new System.Drawing.Point(544, 46);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(110, 32);
            this.btnConfirm.TabIndex = 5;
            this.btnConfirm.Text = "Confirm";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // cbMarketplace
            // 
            this.cbMarketplace.Enabled = false;
            this.cbMarketplace.FormattingEnabled = true;
            this.cbMarketplace.Items.AddRange(new object[] {
            "ShopUsLast",
            "HarvardStation"});
            this.cbMarketplace.Location = new System.Drawing.Point(12, 57);
            this.cbMarketplace.Name = "cbMarketplace";
            this.cbMarketplace.Size = new System.Drawing.Size(121, 21);
            this.cbMarketplace.TabIndex = 6;
            this.cbMarketplace.SelectedValueChanged += new System.EventHandler(this.cbMarketplace_SelectedValueChanged);
            // 
            // dgvResult
            // 
            this.dgvResult.AllowUserToAddRows = false;
            this.dgvResult.AllowUserToDeleteRows = false;
            this.dgvResult.AutoGenerateColumns = false;
            this.dgvResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PurchaseOrder,
            this.Brand,
            this.itemLookupCodeDataGridViewTextBoxColumn,
            this.quantity,
            this.priceDataGridViewTextBoxColumn,
            this.Confirmed,
            this.ErrorCount,
            this.titleDataGridViewTextBoxColumn});
            this.dgvResult.DataSource = this.bsiquantitiesBindingSource;
            this.dgvResult.Location = new System.Drawing.Point(12, 117);
            this.dgvResult.Name = "dgvResult";
            this.dgvResult.ReadOnly = true;
            this.dgvResult.RowHeadersVisible = false;
            this.dgvResult.Size = new System.Drawing.Size(642, 405);
            this.dgvResult.TabIndex = 7;
            this.dgvResult.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvResult_RowEnter);
            // 
            // PurchaseOrder
            // 
            this.PurchaseOrder.DataPropertyName = "PurchaseOrder";
            this.PurchaseOrder.HeaderText = "PurchaseOrder";
            this.PurchaseOrder.Name = "PurchaseOrder";
            this.PurchaseOrder.ReadOnly = true;
            // 
            // Brand
            // 
            this.Brand.DataPropertyName = "Brand";
            this.Brand.HeaderText = "Brand";
            this.Brand.Name = "Brand";
            this.Brand.ReadOnly = true;
            // 
            // itemLookupCodeDataGridViewTextBoxColumn
            // 
            this.itemLookupCodeDataGridViewTextBoxColumn.DataPropertyName = "itemLookupCode";
            this.itemLookupCodeDataGridViewTextBoxColumn.HeaderText = "ItemLookupCode";
            this.itemLookupCodeDataGridViewTextBoxColumn.Name = "itemLookupCodeDataGridViewTextBoxColumn";
            this.itemLookupCodeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // quantity
            // 
            this.quantity.DataPropertyName = "quantity";
            this.quantity.HeaderText = "Quantity";
            this.quantity.Name = "quantity";
            this.quantity.ReadOnly = true;
            // 
            // priceDataGridViewTextBoxColumn
            // 
            this.priceDataGridViewTextBoxColumn.DataPropertyName = "price";
            this.priceDataGridViewTextBoxColumn.HeaderText = "Price";
            this.priceDataGridViewTextBoxColumn.Name = "priceDataGridViewTextBoxColumn";
            this.priceDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // Confirmed
            // 
            this.Confirmed.DataPropertyName = "Confirmed";
            this.Confirmed.HeaderText = "Confirmed";
            this.Confirmed.Name = "Confirmed";
            this.Confirmed.ReadOnly = true;
            // 
            // ErrorCount
            // 
            this.ErrorCount.DataPropertyName = "ErrorCount";
            this.ErrorCount.HeaderText = "ErrorCount";
            this.ErrorCount.Name = "ErrorCount";
            this.ErrorCount.ReadOnly = true;
            // 
            // titleDataGridViewTextBoxColumn
            // 
            this.titleDataGridViewTextBoxColumn.DataPropertyName = "title";
            this.titleDataGridViewTextBoxColumn.HeaderText = "Title";
            this.titleDataGridViewTextBoxColumn.Name = "titleDataGridViewTextBoxColumn";
            this.titleDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // bsiquantitiesBindingSource
            // 
            this.bsiquantitiesBindingSource.DataSource = typeof(bsi_quantities);
            // 
            // btnPending
            // 
            this.btnPending.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPending.Location = new System.Drawing.Point(12, 7);
            this.btnPending.Name = "btnPending";
            this.btnPending.Size = new System.Drawing.Size(126, 32);
            this.btnPending.TabIndex = 8;
            this.btnPending.Text = "Get Pending";
            this.btnPending.UseVisualStyleBackColor = true;
            this.btnPending.Click += new System.EventHandler(this.btnPending_Click);
            // 
            // dgvError
            // 
            this.dgvError.AllowUserToAddRows = false;
            this.dgvError.AllowUserToDeleteRows = false;
            this.dgvError.AutoGenerateColumns = false;
            this.dgvError.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvError.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.submissionTypeDataGridViewTextBoxColumn,
            this.dataGridViewCheckBoxColumn1,
            this.errorMessageDataGridViewTextBoxColumn,
            this.messageNumberDataGridViewTextBoxColumn,
            this.submissionIdDataGridViewTextBoxColumn});
            this.dgvError.DataSource = this.bsiquantitiesmessageBindingSource;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvError.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvError.Location = new System.Drawing.Point(660, 117);
            this.dgvError.Name = "dgvError";
            this.dgvError.ReadOnly = true;
            this.dgvError.RowHeadersVisible = false;
            this.dgvError.Size = new System.Drawing.Size(447, 255);
            this.dgvError.TabIndex = 9;
            // 
            // submissionTypeDataGridViewTextBoxColumn
            // 
            this.submissionTypeDataGridViewTextBoxColumn.DataPropertyName = "submissionType";
            this.submissionTypeDataGridViewTextBoxColumn.HeaderText = "Submission Type";
            this.submissionTypeDataGridViewTextBoxColumn.Name = "submissionTypeDataGridViewTextBoxColumn";
            this.submissionTypeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.DataPropertyName = "confirmed";
            this.dataGridViewCheckBoxColumn1.HeaderText = "confirmed";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            this.dataGridViewCheckBoxColumn1.ReadOnly = true;
            // 
            // errorMessageDataGridViewTextBoxColumn
            // 
            this.errorMessageDataGridViewTextBoxColumn.DataPropertyName = "errorMessage";
            this.errorMessageDataGridViewTextBoxColumn.HeaderText = "Message";
            this.errorMessageDataGridViewTextBoxColumn.Name = "errorMessageDataGridViewTextBoxColumn";
            this.errorMessageDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // messageNumberDataGridViewTextBoxColumn
            // 
            this.messageNumberDataGridViewTextBoxColumn.DataPropertyName = "messageNumber";
            this.messageNumberDataGridViewTextBoxColumn.HeaderText = "Number";
            this.messageNumberDataGridViewTextBoxColumn.Name = "messageNumberDataGridViewTextBoxColumn";
            this.messageNumberDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // submissionIdDataGridViewTextBoxColumn
            // 
            this.submissionIdDataGridViewTextBoxColumn.DataPropertyName = "submissionId";
            this.submissionIdDataGridViewTextBoxColumn.HeaderText = "Submission Id";
            this.submissionIdDataGridViewTextBoxColumn.Name = "submissionIdDataGridViewTextBoxColumn";
            this.submissionIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // bsiquantitiesmessageBindingSource
            // 
            this.bsiquantitiesmessageBindingSource.DataSource = typeof(bsi_quantities_message);
            // 
            // btnRepublish
            // 
            this.btnRepublish.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRepublish.Location = new System.Drawing.Point(987, 7);
            this.btnRepublish.Name = "btnRepublish";
            this.btnRepublish.Size = new System.Drawing.Size(110, 32);
            this.btnRepublish.TabIndex = 10;
            this.btnRepublish.Text = "RePublish";
            this.btnRepublish.UseVisualStyleBackColor = true;
            this.btnRepublish.Click += new System.EventHandler(this.btnRepublish_Click);
            // 
            // AmazonPublishing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1160, 534);
            this.Controls.Add(this.btnRepublish);
            this.Controls.Add(this.dgvError);
            this.Controls.Add(this.btnPending);
            this.Controls.Add(this.dgvResult);
            this.Controls.Add(this.cbMarketplace);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.btnPublish);
            this.Controls.Add(this.labelPath);
            this.Name = "AmazonPublishing";
            this.Text = "AmazonPublishing";
            ((System.ComponentModel.ISupportInitialize)(this.dgvResult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsiquantitiesBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvError)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsiquantitiesmessageBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.Button btnPublish;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.ComboBox cbMarketplace;
        private System.Windows.Forms.DataGridView dgvResult;
        private System.Windows.Forms.Button btnPending;
        private System.Windows.Forms.BindingSource bsiquantitiesBindingSource;
        private System.Windows.Forms.DataGridView dgvError;
        private System.Windows.Forms.BindingSource bsiquantitiesmessageBindingSource;
        private System.Windows.Forms.Button btnRepublish;
        private System.Windows.Forms.DataGridViewTextBoxColumn submissionTypeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorMessageDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn messageNumberDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn submissionIdDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PurchaseOrder;
        private System.Windows.Forms.DataGridViewTextBoxColumn Brand;
        private System.Windows.Forms.DataGridViewTextBoxColumn itemLookupCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn priceDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Confirmed;
        private System.Windows.Forms.DataGridViewTextBoxColumn ErrorCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn titleDataGridViewTextBoxColumn;
    }
}