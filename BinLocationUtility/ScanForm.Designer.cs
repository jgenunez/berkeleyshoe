namespace LocationApp
{
    partial class ScanForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tbSKU = new System.Windows.Forms.TextBox();
            this.lbSku = new System.Windows.Forms.Label();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.dgvScanList = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qtyDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OnHand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BeforeChange = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.productBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnCancel = new System.Windows.Forms.Button();
            this.lbLocation = new System.Windows.Forms.Label();
            this.lbCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvScanList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.productBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // tbSKU
            // 
            this.tbSKU.Location = new System.Drawing.Point(12, 25);
            this.tbSKU.Name = "tbSKU";
            this.tbSKU.Size = new System.Drawing.Size(295, 20);
            this.tbSKU.TabIndex = 0;
            this.tbSKU.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbSKU_KeyDown);
            // 
            // lbSku
            // 
            this.lbSku.AutoSize = true;
            this.lbSku.Location = new System.Drawing.Point(9, 9);
            this.lbSku.Name = "lbSku";
            this.lbSku.Size = new System.Drawing.Size(100, 13);
            this.lbSku.TabIndex = 1;
            this.lbSku.Text = "Enter SKU or UPC :";
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(326, 25);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(87, 23);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(218, 509);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(89, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.TabStop = false;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // dgvScanList
            // 
            this.dgvScanList.AllowUserToAddRows = false;
            this.dgvScanList.AllowUserToDeleteRows = false;
            this.dgvScanList.AllowUserToResizeRows = false;
            this.dgvScanList.AutoGenerateColumns = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvScanList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvScanList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvScanList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.qtyDataGridViewTextBoxColumn,
            this.OnHand,
            this.BeforeChange});
            this.dgvScanList.DataSource = this.productBindingSource;
            this.dgvScanList.Location = new System.Drawing.Point(12, 69);
            this.dgvScanList.Name = "dgvScanList";
            this.dgvScanList.RowHeadersVisible = false;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvScanList.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvScanList.Size = new System.Drawing.Size(401, 429);
            this.dgvScanList.TabIndex = 8;
            this.dgvScanList.TabStop = false;
            this.dgvScanList.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvScanList_CellFormatting);
            // 
            // Column1
            // 
            this.Column1.DataPropertyName = "ID";
            this.Column1.HeaderText = "SKU";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // qtyDataGridViewTextBoxColumn
            // 
            this.qtyDataGridViewTextBoxColumn.DataPropertyName = "QtyScan";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.qtyDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.qtyDataGridViewTextBoxColumn.HeaderText = "Scan";
            this.qtyDataGridViewTextBoxColumn.Name = "qtyDataGridViewTextBoxColumn";
            this.qtyDataGridViewTextBoxColumn.Width = 50;
            // 
            // OnHand
            // 
            this.OnHand.DataPropertyName = "OnHand";
            this.OnHand.HeaderText = "OH";
            this.OnHand.Name = "OnHand";
            this.OnHand.ReadOnly = true;
            this.OnHand.Width = 50;
            // 
            // BeforeChange
            // 
            this.BeforeChange.DataPropertyName = "PreviousBIN";
            this.BeforeChange.HeaderText = "Locations";
            this.BeforeChange.Name = "BeforeChange";
            this.BeforeChange.ReadOnly = true;
            this.BeforeChange.Width = 150;
            // 
            // productBindingSource
            // 
            this.productBindingSource.DataSource = typeof(LocationApp.ProductStock);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(326, 509);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.TabStop = false;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lbLocation
            // 
            this.lbLocation.AutoSize = true;
            this.lbLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLocation.Location = new System.Drawing.Point(12, 510);
            this.lbLocation.Name = "lbLocation";
            this.lbLocation.Size = new System.Drawing.Size(0, 18);
            this.lbLocation.TabIndex = 10;
            // 
            // lbCount
            // 
            this.lbCount.AutoSize = true;
            this.lbCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbCount.Location = new System.Drawing.Point(108, 512);
            this.lbCount.Name = "lbCount";
            this.lbCount.Size = new System.Drawing.Size(55, 16);
            this.lbCount.TabIndex = 11;
            this.lbCount.Text = "Total : 0";
            // 
            // ScanForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 540);
            this.Controls.Add(this.lbCount);
            this.Controls.Add(this.lbLocation);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.dgvScanList);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lbSku);
            this.Controls.Add(this.tbSKU);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ScanForm";
            this.Text = "Quick Scan";
            ((System.ComponentModel.ISupportInitialize)(this.dgvScanList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.productBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbSKU;
        private System.Windows.Forms.Label lbSku;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridView dgvScanList;
        private System.Windows.Forms.BindingSource productBindingSource;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lbLocation;
        private System.Windows.Forms.Label lbCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn sKUDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn qtyDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn OnHand;
        private System.Windows.Forms.DataGridViewTextBoxColumn BeforeChange;
    }
}