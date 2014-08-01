namespace InventoryApp
{
    partial class LoginForm
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
            this.tbUser = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbPhysicalInventory = new System.Windows.Forms.ComboBox();
            this.lbUser = new System.Windows.Forms.Label();
            this.lbPhysicalInventory = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbUser
            // 
            this.tbUser.Location = new System.Drawing.Point(108, 25);
            this.tbUser.Name = "tbUser";
            this.tbUser.Size = new System.Drawing.Size(167, 20);
            this.tbUser.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(70, 111);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "Login";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(200, 111);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // cbPhysicalInventory
            // 
            this.cbPhysicalInventory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPhysicalInventory.FormattingEnabled = true;
            this.cbPhysicalInventory.Location = new System.Drawing.Point(108, 64);
            this.cbPhysicalInventory.Name = "cbPhysicalInventory";
            this.cbPhysicalInventory.Size = new System.Drawing.Size(167, 21);
            this.cbPhysicalInventory.TabIndex = 3;
            // 
            // lbUser
            // 
            this.lbUser.AutoSize = true;
            this.lbUser.Location = new System.Drawing.Point(27, 28);
            this.lbUser.Name = "lbUser";
            this.lbUser.Size = new System.Drawing.Size(66, 13);
            this.lbUser.TabIndex = 4;
            this.lbUser.Text = "User Name :";
            // 
            // lbPhysicalInventory
            // 
            this.lbPhysicalInventory.AutoSize = true;
            this.lbPhysicalInventory.Location = new System.Drawing.Point(13, 67);
            this.lbPhysicalInventory.Name = "lbPhysicalInventory";
            this.lbPhysicalInventory.Size = new System.Drawing.Size(80, 13);
            this.lbPhysicalInventory.TabIndex = 5;
            this.lbPhysicalInventory.Text = "Inventory Ref  :";
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 146);
            this.Controls.Add(this.lbPhysicalInventory);
            this.Controls.Add(this.lbUser);
            this.Controls.Add(this.cbPhysicalInventory);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tbUser);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.Text = "Login";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbUser;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox cbPhysicalInventory;
        private System.Windows.Forms.Label lbUser;
        private System.Windows.Forms.Label lbPhysicalInventory;
    }
}