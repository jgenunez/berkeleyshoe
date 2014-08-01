namespace LocationApp
{
    partial class MainForm
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
            this.tvHistory = new System.Windows.Forms.TreeView();
            this.btnScan = new System.Windows.Forms.Button();
            this.tbCurrentLoc = new System.Windows.Forms.TextBox();
            this.btnCleanLoc = new System.Windows.Forms.Button();
            this.lbUser = new System.Windows.Forms.Label();
            this.btnUpdateLog = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tvHistory
            // 
            this.tvHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvHistory.Location = new System.Drawing.Point(12, 106);
            this.tvHistory.Name = "tvHistory";
            this.tvHistory.Size = new System.Drawing.Size(358, 428);
            this.tvHistory.TabIndex = 1;
            this.tvHistory.TabStop = false;
            // 
            // btnScan
            // 
            this.btnScan.Location = new System.Drawing.Point(295, 11);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(75, 23);
            this.btnScan.TabIndex = 1;
            this.btnScan.Text = "Start Scan";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // tbCurrentLoc
            // 
            this.tbCurrentLoc.Location = new System.Drawing.Point(179, 13);
            this.tbCurrentLoc.Name = "tbCurrentLoc";
            this.tbCurrentLoc.Size = new System.Drawing.Size(110, 20);
            this.tbCurrentLoc.TabIndex = 0;
            // 
            // btnCleanLoc
            // 
            this.btnCleanLoc.Location = new System.Drawing.Point(260, 77);
            this.btnCleanLoc.Name = "btnCleanLoc";
            this.btnCleanLoc.Size = new System.Drawing.Size(110, 23);
            this.btnCleanLoc.TabIndex = 6;
            this.btnCleanLoc.TabStop = false;
            this.btnCleanLoc.Text = "Clean Location";
            this.btnCleanLoc.UseVisualStyleBackColor = true;
            this.btnCleanLoc.Click += new System.EventHandler(this.btnCleanLoc_Click);
            // 
            // lbUser
            // 
            this.lbUser.AutoSize = true;
            this.lbUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbUser.Location = new System.Drawing.Point(12, 15);
            this.lbUser.Name = "lbUser";
            this.lbUser.Size = new System.Drawing.Size(0, 16);
            this.lbUser.TabIndex = 8;
            // 
            // btnUpdateLog
            // 
            this.btnUpdateLog.Location = new System.Drawing.Point(15, 77);
            this.btnUpdateLog.Name = "btnUpdateLog";
            this.btnUpdateLog.Size = new System.Drawing.Size(78, 23);
            this.btnUpdateLog.TabIndex = 9;
            this.btnUpdateLog.Text = "Load Log";
            this.btnUpdateLog.UseVisualStyleBackColor = true;
            this.btnUpdateLog.Click += new System.EventHandler(this.btnUpdateLog_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 546);
            this.Controls.Add(this.btnUpdateLog);
            this.Controls.Add(this.lbUser);
            this.Controls.Add(this.btnCleanLoc);
            this.Controls.Add(this.tbCurrentLoc);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.tvHistory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "Location Utility";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvHistory;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.TextBox tbCurrentLoc;
        private System.Windows.Forms.Button btnCleanLoc;
        private System.Windows.Forms.Label lbUser;
        private System.Windows.Forms.Button btnUpdateLog;
    }
}

