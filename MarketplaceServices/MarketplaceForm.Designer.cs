namespace MarketplaceManager
{
    partial class MarketplaceForm
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
            this.btnSyncListings = new System.Windows.Forms.Button();
            this.dgvMarketplaces = new System.Windows.Forms.DataGridView();
            this.iDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Host = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ordersLastSyncDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.listingsLastSyncDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.WaitingShipment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.marketplaceViewBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnSyncOrders = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnFixOverpublished = new System.Windows.Forms.Button();
            this.tbCheckItem = new System.Windows.Forms.TextBox();
            this.btnCheckItem = new System.Windows.Forms.Button();
            this.btnSummary = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMarketplaces)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.marketplaceViewBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSyncListings
            // 
            this.btnSyncListings.Location = new System.Drawing.Point(117, 334);
            this.btnSyncListings.Name = "btnSyncListings";
            this.btnSyncListings.Size = new System.Drawing.Size(99, 32);
            this.btnSyncListings.TabIndex = 1;
            this.btnSyncListings.Text = "Sync Listings";
            this.btnSyncListings.UseVisualStyleBackColor = true;
            this.btnSyncListings.Click += new System.EventHandler(this.btnSyncListings_Click);
            // 
            // dgvMarketplaces
            // 
            this.dgvMarketplaces.AllowUserToAddRows = false;
            this.dgvMarketplaces.AllowUserToDeleteRows = false;
            this.dgvMarketplaces.AutoGenerateColumns = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvMarketplaces.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvMarketplaces.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMarketplaces.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.iDDataGridViewTextBoxColumn,
            this.Host,
            this.nameDataGridViewTextBoxColumn,
            this.ordersLastSyncDataGridViewTextBoxColumn,
            this.listingsLastSyncDataGridViewTextBoxColumn,
            this.WaitingShipment});
            this.dgvMarketplaces.DataSource = this.marketplaceViewBindingSource;
            this.dgvMarketplaces.Location = new System.Drawing.Point(12, 50);
            this.dgvMarketplaces.MultiSelect = false;
            this.dgvMarketplaces.Name = "dgvMarketplaces";
            this.dgvMarketplaces.ReadOnly = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvMarketplaces.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvMarketplaces.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMarketplaces.Size = new System.Drawing.Size(631, 278);
            this.dgvMarketplaces.TabIndex = 4;
            // 
            // iDDataGridViewTextBoxColumn
            // 
            this.iDDataGridViewTextBoxColumn.DataPropertyName = "ID";
            this.iDDataGridViewTextBoxColumn.HeaderText = "ID";
            this.iDDataGridViewTextBoxColumn.Name = "iDDataGridViewTextBoxColumn";
            this.iDDataGridViewTextBoxColumn.ReadOnly = true;
            this.iDDataGridViewTextBoxColumn.Visible = false;
            // 
            // Host
            // 
            this.Host.DataPropertyName = "Host";
            this.Host.HeaderText = "Host";
            this.Host.Name = "Host";
            this.Host.ReadOnly = true;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // ordersLastSyncDataGridViewTextBoxColumn
            // 
            this.ordersLastSyncDataGridViewTextBoxColumn.DataPropertyName = "OrdersLastSync";
            this.ordersLastSyncDataGridViewTextBoxColumn.HeaderText = "Orders Last Sync";
            this.ordersLastSyncDataGridViewTextBoxColumn.Name = "ordersLastSyncDataGridViewTextBoxColumn";
            this.ordersLastSyncDataGridViewTextBoxColumn.ReadOnly = true;
            this.ordersLastSyncDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // listingsLastSyncDataGridViewTextBoxColumn
            // 
            this.listingsLastSyncDataGridViewTextBoxColumn.DataPropertyName = "ListingsLastSync";
            this.listingsLastSyncDataGridViewTextBoxColumn.HeaderText = "Listings Last Sync";
            this.listingsLastSyncDataGridViewTextBoxColumn.Name = "listingsLastSyncDataGridViewTextBoxColumn";
            this.listingsLastSyncDataGridViewTextBoxColumn.ReadOnly = true;
            this.listingsLastSyncDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // WaitingShipment
            // 
            this.WaitingShipment.DataPropertyName = "WaitingShipment";
            this.WaitingShipment.HeaderText = "Waiting Shipment";
            this.WaitingShipment.Name = "WaitingShipment";
            this.WaitingShipment.ReadOnly = true;
            this.WaitingShipment.Visible = false;
            // 
            // marketplaceViewBindingSource
            // 
            this.marketplaceViewBindingSource.DataSource = typeof(MarketplaceManager.MarketplaceView);
            // 
            // btnSyncOrders
            // 
            this.btnSyncOrders.Location = new System.Drawing.Point(12, 334);
            this.btnSyncOrders.Name = "btnSyncOrders";
            this.btnSyncOrders.Size = new System.Drawing.Size(99, 32);
            this.btnSyncOrders.TabIndex = 6;
            this.btnSyncOrders.Text = "Sync Orders";
            this.btnSyncOrders.UseVisualStyleBackColor = true;
            this.btnSyncOrders.Click += new System.EventHandler(this.btnSyncOrders_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(414, 334);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(106, 32);
            this.btnGenerate.TabIndex = 7;
            this.btnGenerate.Text = "Generate Report";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnFixOverpublished
            // 
            this.btnFixOverpublished.Location = new System.Drawing.Point(535, 334);
            this.btnFixOverpublished.Name = "btnFixOverpublished";
            this.btnFixOverpublished.Size = new System.Drawing.Size(108, 32);
            this.btnFixOverpublished.TabIndex = 8;
            this.btnFixOverpublished.Text = "Fix Overpublished";
            this.btnFixOverpublished.UseVisualStyleBackColor = true;
            // 
            // tbCheckItem
            // 
            this.tbCheckItem.Location = new System.Drawing.Point(12, 13);
            this.tbCheckItem.Name = "tbCheckItem";
            this.tbCheckItem.Size = new System.Drawing.Size(187, 20);
            this.tbCheckItem.TabIndex = 10;
            // 
            // btnCheckItem
            // 
            this.btnCheckItem.Location = new System.Drawing.Point(222, 11);
            this.btnCheckItem.Name = "btnCheckItem";
            this.btnCheckItem.Size = new System.Drawing.Size(94, 23);
            this.btnCheckItem.TabIndex = 11;
            this.btnCheckItem.Text = "Check Item";
            this.btnCheckItem.UseVisualStyleBackColor = true;
            this.btnCheckItem.Click += new System.EventHandler(this.btnCheckItem_Click);
            // 
            // btnSummary
            // 
            this.btnSummary.Location = new System.Drawing.Point(544, 6);
            this.btnSummary.Name = "btnSummary";
            this.btnSummary.Size = new System.Drawing.Size(99, 32);
            this.btnSummary.TabIndex = 12;
            this.btnSummary.Text = "Summary";
            this.btnSummary.UseVisualStyleBackColor = true;
            this.btnSummary.Click += new System.EventHandler(this.btnSummary_Click);
            // 
            // MarketplaceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 404);
            this.Controls.Add(this.btnSummary);
            this.Controls.Add(this.btnCheckItem);
            this.Controls.Add(this.tbCheckItem);
            this.Controls.Add(this.btnFixOverpublished);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnSyncOrders);
            this.Controls.Add(this.dgvMarketplaces);
            this.Controls.Add(this.btnSyncListings);
            this.Name = "MarketplaceForm";
            this.Text = "Marketplace Manager";
            this.Load += new System.EventHandler(this.SyncForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMarketplaces)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.marketplaceViewBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSyncListings;
        private System.Windows.Forms.DataGridView dgvMarketplaces;
        private System.Windows.Forms.Button btnSyncOrders;
        private System.Windows.Forms.DataGridViewTextBoxColumn expiringSoonQtyDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource marketplaceViewBindingSource;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.DataGridViewTextBoxColumn iDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Host;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ordersLastSyncDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn listingsLastSyncDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn activeQtyDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn WaitingShipment;
        private System.Windows.Forms.Button btnFixOverpublished;
        private System.Windows.Forms.TextBox tbCheckItem;
        private System.Windows.Forms.Button btnCheckItem;
        private System.Windows.Forms.Button btnSummary;
    }
}