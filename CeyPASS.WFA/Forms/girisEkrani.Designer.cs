namespace CeyPASS.WFA
{
    partial class girisEkrani
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
        ///<summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(girisEkrani));
            this.pnlBackground = new System.Windows.Forms.Panel();
            this.btnCanliIzleme = new System.Windows.Forms.Button();
            this.pnlLoginCard = new System.Windows.Forms.Panel();
            this.btnSifremiUnuttum = new System.Windows.Forms.Button();
            this.btnGiris = new System.Windows.Forms.Button();
            this.pnlForm = new System.Windows.Forms.Panel();
            this.chxSifreGoster = new System.Windows.Forms.CheckBox();
            this.pnlPassword = new System.Windows.Forms.Panel();
            this.txtSifre = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.pnlUsername = new System.Windows.Forms.Panel();
            this.txtKullaniciAdi = new System.Windows.Forms.TextBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.pnlLogo = new System.Windows.Forms.Panel();
            this.sirketLogo = new System.Windows.Forms.PictureBox();
            this.pnlBackground.SuspendLayout();
            this.pnlLoginCard.SuspendLayout();
            this.pnlForm.SuspendLayout();
            this.pnlPassword.SuspendLayout();
            this.pnlUsername.SuspendLayout();
            this.pnlLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sirketLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.BackgroundImage = global::CeyPASS.WFA.Properties.Resources.ceyportTekirdag;
            this.pnlBackground.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlBackground.Controls.Add(this.btnCanliIzleme);
            this.pnlBackground.Controls.Add(this.pnlLoginCard);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(898, 642);
            this.pnlBackground.TabIndex = 0;
            this.pnlBackground.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlBackground_Paint);
            // 
            // btnCanliIzleme
            // 
            this.btnCanliIzleme.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCanliIzleme.BackColor = System.Drawing.Color.Transparent;
            this.btnCanliIzleme.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCanliIzleme.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnCanliIzleme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCanliIzleme.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.btnCanliIzleme.ForeColor = System.Drawing.Color.Black;
            this.btnCanliIzleme.Image = global::CeyPASS.WFA.Properties.Resources.icons8_security_check_48;
            this.btnCanliIzleme.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCanliIzleme.Location = new System.Drawing.Point(665, 19);
            this.btnCanliIzleme.Name = "btnCanliIzleme";
            this.btnCanliIzleme.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.btnCanliIzleme.Size = new System.Drawing.Size(220, 55);
            this.btnCanliIzleme.TabIndex = 10;
            this.btnCanliIzleme.TabStop = false;
            this.btnCanliIzleme.Text = "Canlı İzleme Paneli";
            this.btnCanliIzleme.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCanliIzleme.UseVisualStyleBackColor = false;
            this.btnCanliIzleme.Click += new System.EventHandler(this.btnCanliIzleme_Click);
            this.btnCanliIzleme.MouseEnter += new System.EventHandler(this.btnCanliIzleme_MouseEnter);
            this.btnCanliIzleme.MouseLeave += new System.EventHandler(this.btnCanliIzleme_MouseLeave);
            // 
            // pnlLoginCard
            // 
            this.pnlLoginCard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlLoginCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.pnlLoginCard.Controls.Add(this.btnSifremiUnuttum);
            this.pnlLoginCard.Controls.Add(this.btnGiris);
            this.pnlLoginCard.Controls.Add(this.pnlForm);
            this.pnlLoginCard.Controls.Add(this.lblSubtitle);
            this.pnlLoginCard.Controls.Add(this.lblWelcome);
            this.pnlLoginCard.Controls.Add(this.pnlLogo);
            this.pnlLoginCard.Location = new System.Drawing.Point(239, 51);
            this.pnlLoginCard.Name = "pnlLoginCard";
            this.pnlLoginCard.Size = new System.Drawing.Size(420, 540);
            this.pnlLoginCard.TabIndex = 0;
            this.pnlLoginCard.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlLoginCard_Paint);
            // 
            // btnSifremiUnuttum
            // 
            this.btnSifremiUnuttum.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSifremiUnuttum.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSifremiUnuttum.FlatAppearance.BorderSize = 0;
            this.btnSifremiUnuttum.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSifremiUnuttum.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSifremiUnuttum.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(212)))));
            this.btnSifremiUnuttum.Location = new System.Drawing.Point(0, 430);
            this.btnSifremiUnuttum.Name = "btnSifremiUnuttum";
            this.btnSifremiUnuttum.Padding = new System.Windows.Forms.Padding(40, 5, 40, 0);
            this.btnSifremiUnuttum.Size = new System.Drawing.Size(420, 40);
            this.btnSifremiUnuttum.TabIndex = 5;
            this.btnSifremiUnuttum.Text = "Şifremi Unuttum";
            this.btnSifremiUnuttum.UseVisualStyleBackColor = true;
            this.btnSifremiUnuttum.Click += new System.EventHandler(this.btnSifremiUnuttum_Click);
            // 
            // btnGiris
            // 
            this.btnGiris.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnGiris.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGiris.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnGiris.FlatAppearance.BorderSize = 0;
            this.btnGiris.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGiris.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnGiris.ForeColor = System.Drawing.Color.White;
            this.btnGiris.Location = new System.Drawing.Point(0, 380);
            this.btnGiris.Margin = new System.Windows.Forms.Padding(40, 20, 40, 0);
            this.btnGiris.Name = "btnGiris";
            this.btnGiris.Padding = new System.Windows.Forms.Padding(40, 0, 40, 0);
            this.btnGiris.Size = new System.Drawing.Size(420, 50);
            this.btnGiris.TabIndex = 4;
            this.btnGiris.Text = "GİRİŞ YAP";
            this.btnGiris.UseVisualStyleBackColor = false;
            this.btnGiris.Click += new System.EventHandler(this.btnGiris_Click);
            this.btnGiris.MouseEnter += new System.EventHandler(this.btnGiris_MouseEnter);
            this.btnGiris.MouseLeave += new System.EventHandler(this.btnGiris_MouseLeave);
            // 
            // pnlForm
            // 
            this.pnlForm.Controls.Add(this.chxSifreGoster);
            this.pnlForm.Controls.Add(this.pnlPassword);
            this.pnlForm.Controls.Add(this.pnlUsername);
            this.pnlForm.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlForm.Location = new System.Drawing.Point(0, 180);
            this.pnlForm.Name = "pnlForm";
            this.pnlForm.Padding = new System.Windows.Forms.Padding(40, 10, 40, 0);
            this.pnlForm.Size = new System.Drawing.Size(420, 200);
            this.pnlForm.TabIndex = 3;
            // 
            // chxSifreGoster
            // 
            this.chxSifreGoster.AutoSize = true;
            this.chxSifreGoster.Dock = System.Windows.Forms.DockStyle.Top;
            this.chxSifreGoster.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chxSifreGoster.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.chxSifreGoster.Location = new System.Drawing.Point(40, 150);
            this.chxSifreGoster.Name = "chxSifreGoster";
            this.chxSifreGoster.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.chxSifreGoster.Size = new System.Drawing.Size(340, 29);
            this.chxSifreGoster.TabIndex = 3;
            this.chxSifreGoster.Text = "Şifreyi göster";
            this.chxSifreGoster.UseVisualStyleBackColor = true;
            this.chxSifreGoster.CheckedChanged += new System.EventHandler(this.chxSifreGoster_CheckedChanged);
            // 
            // pnlPassword
            // 
            this.pnlPassword.Controls.Add(this.txtSifre);
            this.pnlPassword.Controls.Add(this.lblPassword);
            this.pnlPassword.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPassword.Location = new System.Drawing.Point(40, 80);
            this.pnlPassword.Name = "pnlPassword";
            this.pnlPassword.Size = new System.Drawing.Size(340, 70);
            this.pnlPassword.TabIndex = 1;
            // 
            // txtSifre
            // 
            this.txtSifre.BackColor = System.Drawing.Color.White;
            this.txtSifre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSifre.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtSifre.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtSifre.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.txtSifre.Location = new System.Drawing.Point(0, 25);
            this.txtSifre.Name = "txtSifre";
            this.txtSifre.PasswordChar = '●';
            this.txtSifre.Size = new System.Drawing.Size(340, 32);
            this.txtSifre.TabIndex = 2;
            this.txtSifre.Enter += new System.EventHandler(this.txtSifre_Enter);
            this.txtSifre.Leave += new System.EventHandler(this.txtSifre_Leave);
            // 
            // lblPassword
            // 
            this.lblPassword.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.lblPassword.Location = new System.Drawing.Point(0, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(340, 25);
            this.lblPassword.TabIndex = 0;
            this.lblPassword.Text = "Şifre";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // pnlUsername
            // 
            this.pnlUsername.Controls.Add(this.txtKullaniciAdi);
            this.pnlUsername.Controls.Add(this.lblUsername);
            this.pnlUsername.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlUsername.Location = new System.Drawing.Point(40, 10);
            this.pnlUsername.Name = "pnlUsername";
            this.pnlUsername.Size = new System.Drawing.Size(340, 70);
            this.pnlUsername.TabIndex = 0;
            // 
            // txtKullaniciAdi
            // 
            this.txtKullaniciAdi.BackColor = System.Drawing.Color.White;
            this.txtKullaniciAdi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtKullaniciAdi.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtKullaniciAdi.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtKullaniciAdi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.txtKullaniciAdi.Location = new System.Drawing.Point(0, 25);
            this.txtKullaniciAdi.Name = "txtKullaniciAdi";
            this.txtKullaniciAdi.Size = new System.Drawing.Size(340, 32);
            this.txtKullaniciAdi.TabIndex = 1;
            this.txtKullaniciAdi.Enter += new System.EventHandler(this.txtKullaniciAdi_Enter);
            this.txtKullaniciAdi.Leave += new System.EventHandler(this.txtKullaniciAdi_Leave);
            // 
            // lblUsername
            // 
            this.lblUsername.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.lblUsername.Location = new System.Drawing.Point(0, 0);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(340, 25);
            this.lblUsername.TabIndex = 0;
            this.lblUsername.Text = "Kullanıcı Adı";
            this.lblUsername.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblSubtitle.Location = new System.Drawing.Point(0, 150);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Padding = new System.Windows.Forms.Padding(40, 0, 40, 10);
            this.lblSubtitle.Size = new System.Drawing.Size(420, 30);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "Devam etmek için giriş yapın";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblWelcome — Tahoma: Ş, Ğ, İ vb. Türkçe karakterlerin doğru görünmesi için
            // 
            this.lblWelcome.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblWelcome.Font = new System.Drawing.Font("Tahoma", 20F, System.Drawing.FontStyle.Bold);
            this.lblWelcome.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.lblWelcome.Location = new System.Drawing.Point(0, 100);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Padding = new System.Windows.Forms.Padding(40, 10, 40, 0);
            this.lblWelcome.Size = new System.Drawing.Size(420, 50);
            this.lblWelcome.TabIndex = 1;
            this.lblWelcome.Text = "Hoş Geldiniz";
            this.lblWelcome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlLogo
            // 
            this.pnlLogo.Controls.Add(this.sirketLogo);
            this.pnlLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogo.Location = new System.Drawing.Point(0, 0);
            this.pnlLogo.Name = "pnlLogo";
            this.pnlLogo.Padding = new System.Windows.Forms.Padding(40, 30, 40, 0);
            this.pnlLogo.Size = new System.Drawing.Size(420, 100);
            this.pnlLogo.TabIndex = 0;
            // 
            // sirketLogo
            // 
            this.sirketLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sirketLogo.Image = global::CeyPASS.WFA.Properties.Resources.ceyLogo;
            this.sirketLogo.Location = new System.Drawing.Point(40, 30);
            this.sirketLogo.Name = "sirketLogo";
            this.sirketLogo.Size = new System.Drawing.Size(340, 70);
            this.sirketLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.sirketLogo.TabIndex = 0;
            this.sirketLogo.TabStop = false;
            // 
            // girisEkrani
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(898, 642);
            this.Controls.Add(this.pnlBackground);
            this.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(916, 689);
            this.MinimumSize = new System.Drawing.Size(916, 689);
            this.Name = "girisEkrani";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.girisEkrani_FormClosing);
            this.Load += new System.EventHandler(this.girisEkrani_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.girisEkrani_KeyDown);
            this.pnlBackground.ResumeLayout(false);
            this.pnlLoginCard.ResumeLayout(false);
            this.pnlForm.ResumeLayout(false);
            this.pnlForm.PerformLayout();
            this.pnlPassword.ResumeLayout(false);
            this.pnlPassword.PerformLayout();
            this.pnlUsername.ResumeLayout(false);
            this.pnlUsername.PerformLayout();
            this.pnlLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sirketLogo)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Panel pnlBackground;
        private System.Windows.Forms.Panel pnlLoginCard;
        private System.Windows.Forms.Panel pnlLogo;
        private System.Windows.Forms.PictureBox sirketLogo;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel pnlForm;
        private System.Windows.Forms.Panel pnlUsername;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtKullaniciAdi;
        private System.Windows.Forms.Panel pnlPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtSifre;
        private System.Windows.Forms.CheckBox chxSifreGoster;
        private System.Windows.Forms.Button btnGiris;
        private System.Windows.Forms.Button btnSifremiUnuttum;
        public System.Windows.Forms.Button btnCanliIzleme;
    }
}


