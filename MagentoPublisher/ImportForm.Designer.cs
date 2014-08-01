namespace MagentoUpload
{
    partial class MagentoImportForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnImport = new System.Windows.Forms.Button();
            this.pbStatus = new System.Windows.Forms.ProgressBar();
            this.dgvPendingWorkOrder = new System.Windows.Forms.DataGridView();
            this.pendingWorkOrderBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnLoadPending = new System.Windows.Forms.Button();
            this.lbCurrentUpload = new System.Windows.Forms.Label();
            this.workOrderDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.userDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pendingCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalCountDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendingWorkOrder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pendingWorkOrderBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(12, 12);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 0;
            this.btnImport.Text = "Upload";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // pbStatus
            // 
            this.pbStatus.Location = new System.Drawing.Point(146, 54);
            this.pbStatus.Name = "pbStatus";
            this.pbStatus.Size = new System.Drawing.Size(249, 23);
            this.pbStatus.TabIndex = 4;
            this.pbStatus.Visible = false;
            // 
            // dgvPendingWorkOrder
            // 
            this.dgvPendingWorkOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvPendingWorkOrder.AutoGenerateColumns = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvPendingWorkOrder.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvPendingWorkOrder.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPendingWorkOrder.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.workOrderDataGridViewTextBoxColumn,
            this.userDataGridViewTextBoxColumn,
            this.pendingCountDataGridViewTextBoxColumn,
            this.totalCountDataGridViewTextBoxColumn});
            this.dgvPendingWorkOrder.DataSource = this.pendingWorkOrderBindingSource;
            this.dgvPendingWorkOrder.Location = new System.Drawing.Point(15, 83);
            this.dgvPendingWorkOrder.Name = "dgvPendingWorkOrder";
            this.dgvPendingWorkOrder.RowHeadersVisible = false;
            this.dgvPendingWorkOrder.Size = new System.Drawing.Size(380, 218);
            this.dgvPendingWorkOrder.TabIndex = 7;
            // 
            // pendingWorkOrderBindingSource
            // 
            this.pendingWorkOrderBindingSource.DataSource = typeof(MagentoUpload.PendingWorkOrder);
            // 
            // btnLoadPending
            // 
            this.btnLoadPending.Location = new System.Drawing.Point(12, 54);
            this.btnLoadPending.Name = "btnLoadPending";
            this.btnLoadPending.Size = new System.Drawing.Size(75, 23);
            this.btnLoadPending.TabIndex = 8;
            this.btnLoadPending.Text = "Refresh";
            this.btnLoadPending.UseVisualStyleBackColor = true;
            this.btnLoadPending.Click += new System.EventHandler(this.btnLoadPending_Click);
            // 
            // lbCurrentUpload
            // 
            this.lbCurrentUpload.AutoSize = true;
            this.lbCurrentUpload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbCurrentUpload.Location = new System.Drawing.Point(143, 36);
            this.lbCurrentUpload.Name = "lbCurrentUpload";
            this.lbCurrentUpload.Size = new System.Drawing.Size(0, 15);
            this.lbCurrentUpload.TabIndex = 9;
            // 
            // workOrderDataGridViewTextBoxColumn
            // 
            this.workOrderDataGridViewTextBoxColumn.DataPropertyName = "WorkOrder";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.workOrderDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.workOrderDataGridViewTextBoxColumn.HeaderText = "WorkOrder";
            this.workOrderDataGridViewTextBoxColumn.Name = "workOrderDataGridViewTextBoxColumn";
            this.workOrderDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // userDataGridViewTextBoxColumn
            // 
            this.userDataGridViewTextBoxColumn.DataPropertyName = "User";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.userDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.userDataGridViewTextBoxColumn.HeaderText = "User";
            this.userDataGridViewTextBoxColumn.Name = "userDataGridViewTextBoxColumn";
            this.userDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // pendingCountDataGridViewTextBoxColumn
            // 
            this.pendingCountDataGridViewTextBoxColumn.DataPropertyName = "PendingCount";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.pendingCountDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle4;
            this.pendingCountDataGridViewTextBoxColumn.HeaderText = "Pending";
            this.pendingCountDataGridViewTextBoxColumn.Name = "pendingCountDataGridViewTextBoxColumn";
            this.pendingCountDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // totalCountDataGridViewTextBoxColumn
            // 
            this.totalCountDataGridViewTextBoxColumn.DataPropertyName = "TotalCount";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.totalCountDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle5;
            this.totalCountDataGridViewTextBoxColumn.HeaderText = "Total";
            this.totalCountDataGridViewTextBoxColumn.Name = "totalCountDataGridViewTextBoxColumn";
            this.totalCountDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // MagentoImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 315);
            this.Controls.Add(this.lbCurrentUpload);
            this.Controls.Add(this.btnLoadPending);
            this.Controls.Add(this.dgvPendingWorkOrder);
            this.Controls.Add(this.pbStatus);
            this.Controls.Add(this.btnImport);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "MagentoImportForm";
            this.Text = "Magento Import Utility";
            this.Load += new System.EventHandler(this.MagentoImportForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPendingWorkOrder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pendingWorkOrderBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.ProgressBar pbStatus;
        private System.Windows.Forms.DataGridView dgvPendingWorkOrder;
        private System.Windows.Forms.BindingSource pendingWorkOrderBindingSource;
        private System.Windows.Forms.Button btnLoadPending;
        private System.Windows.Forms.Label lbCurrentUpload;
        private System.Windows.Forms.DataGridViewTextBoxColumn workOrderDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn userDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pendingCountDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalCountDataGridViewTextBoxColumn;
    }
}

