namespace ImageAnnotationApp.Forms
{
    partial class RegisterForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblConfirmPassword = new System.Windows.Forms.Label();
            this.txtConfirmPassword = new System.Windows.Forms.TextBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblNote = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(160, 24);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(110, 31);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "用户注册";
            //
            // lblUsername
            //
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lblUsername.Location = new System.Drawing.Point(50, 90);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(65, 20);
            this.lblUsername.TabIndex = 1;
            this.lblUsername.Text = "用户名:";
            //
            // txtUsername
            //
            this.txtUsername.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.txtUsername.Location = new System.Drawing.Point(150, 87);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(250, 25);
            this.txtUsername.TabIndex = 0;
            //
            // lblPassword
            //
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lblPassword.Location = new System.Drawing.Point(50, 135);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(51, 20);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "密码:";
            //
            // txtPassword
            //
            this.txtPassword.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.txtPassword.Location = new System.Drawing.Point(150, 132);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(250, 25);
            this.txtPassword.TabIndex = 1;
            //
            // lblConfirmPassword
            //
            this.lblConfirmPassword.AutoSize = true;
            this.lblConfirmPassword.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lblConfirmPassword.Location = new System.Drawing.Point(50, 180);
            this.lblConfirmPassword.Name = "lblConfirmPassword";
            this.lblConfirmPassword.Size = new System.Drawing.Size(79, 20);
            this.lblConfirmPassword.TabIndex = 5;
            this.lblConfirmPassword.Text = "确认密码:";
            //
            // txtConfirmPassword
            //
            this.txtConfirmPassword.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.txtConfirmPassword.Location = new System.Drawing.Point(150, 177);
            this.txtConfirmPassword.Name = "txtConfirmPassword";
            this.txtConfirmPassword.PasswordChar = '*';
            this.txtConfirmPassword.Size = new System.Drawing.Size(250, 25);
            this.txtConfirmPassword.TabIndex = 2;
            //
            // btnRegister
            //
            this.btnRegister.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.FlatAppearance.BorderSize = 0;
            this.btnRegister.ForeColor = System.Drawing.Color.White;
            this.btnRegister.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRegister.Location = new System.Drawing.Point(120, 285);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(130, 42);
            this.btnRegister.TabIndex = 3;
            this.btnRegister.Text = "注册";
            this.btnRegister.UseVisualStyleBackColor = false;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            //
            // btnCancel
            //
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.FlatAppearance.BorderSize = 1;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.btnCancel.Location = new System.Drawing.Point(280, 285);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 42);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // lblNote
            //
            this.lblNote.AutoSize = true;
            this.lblNote.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblNote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(140)))), ((int)(((byte)(0)))));
            this.lblNote.Location = new System.Drawing.Point(50, 220);
            this.lblNote.Name = "lblNote";
            this.lblNote.Size = new System.Drawing.Size(338, 34);
            this.lblNote.TabIndex = 9;
            this.lblNote.Text = "注意: 新注册的用户将以游客身份加入，需要管理员审核后\n才能使用完整功能。";
            //
            // RegisterForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 370);
            this.Controls.Add(this.lblNote);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.txtConfirmPassword);
            this.Controls.Add(this.lblConfirmPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RegisterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "图片标注系统 - 注册";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblConfirmPassword;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblNote;
    }
}
