namespace CeyPASS.WFA.Forms
{
    partial class canliIzlemeGirisEkrani
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

        #region
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(canliIzlemeGirisEkrani));
            this.pnlBackground = new System.Windows.Forms.Panel();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.canliEkranGirisButon = new System.Windows.Forms.Button();
            this.pnlForm = new System.Windows.Forms.Panel();
            this.pnlBolge = new System.Windows.Forms.Panel();
            this.canliIzlemeBolgeBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pnlPass = new System.Windows.Forms.Panel();
            this.canliEkranSifre = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pnlUser = new System.Windows.Forms.Panel();
            this.canliEkranKullaniciAdi = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlLogo = new System.Windows.Forms.Panel();
            this.sirketLogo = new System.Windows.Forms.PictureBox();
            this.pnlBackground.SuspendLayout();
            this.pnlCard.SuspendLayout();
            this.pnlForm.SuspendLayout();
            this.pnlBolge.SuspendLayout();
            this.pnlPass.SuspendLayout();
            this.pnlUser.SuspendLayout();
            this.pnlLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sirketLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(239)))), ((int)(((byte)(242)))));
            this.pnlBackground.Controls.Add(this.pnlCard);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(480, 420);
            this.pnlBackground.TabIndex = 0;
            // 
            // pnlCard
            // 
            this.pnlCard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlCard.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlCard.Controls.Add(this.canliEkranGirisButon);
            this.pnlCard.Controls.Add(this.pnlForm);
            this.pnlCard.Controls.Add(this.lblSubtitle);
            this.pnlCard.Controls.Add(this.lblTitle);
            this.pnlCard.Controls.Add(this.pnlLogo);
            this.pnlCard.Location = new System.Drawing.Point(30, 28);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Padding = new System.Windows.Forms.Padding(28, 24, 28, 24);
            this.pnlCard.Size = new System.Drawing.Size(420, 364);
            this.pnlCard.TabIndex = 0;
            // 
            // canliEkranGirisButon
            // 
            this.canliEkranGirisButon.BackColor = System.Drawing.Color.Red;
            this.canliEkranGirisButon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.canliEkranGirisButon.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.canliEkranGirisButon.FlatAppearance.BorderSize = 0;
            this.canliEkranGirisButon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.canliEkranGirisButon.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.canliEkranGirisButon.ForeColor = System.Drawing.Color.White;
            this.canliEkranGirisButon.Location = new System.Drawing.Point(28, 296);
            this.canliEkranGirisButon.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.canliEkranGirisButon.Name = "canliEkranGirisButon";
            this.canliEkranGirisButon.Size = new System.Drawing.Size(364, 44);
            this.canliEkranGirisButon.TabIndex = 4;
            this.canliEkranGirisButon.Text = "GİRİŞ";
            this.canliEkranGirisButon.UseVisualStyleBackColor = false;
            this.canliEkranGirisButon.Click += new System.EventHandler(this.canliEkranGirisButon_Click);
            // 
            // pnlForm
            // 
            this.pnlForm.Controls.Add(this.pnlPass);
            this.pnlForm.Controls.Add(this.pnlUser);
            this.pnlForm.Controls.Add(this.pnlBolge);
            this.pnlForm.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlForm.Location = new System.Drawing.Point(28, 114);
            this.pnlForm.Name = "pnlForm";
            this.pnlForm.Padding = new System.Windows.Forms.Padding(0, 8, 0, 16);
            this.pnlForm.Size = new System.Drawing.Size(364, 164);
            this.pnlForm.TabIndex = 3;
            // 
            // pnlBolge
            // 
            this.pnlBolge.Controls.Add(this.canliIzlemeBolgeBox);
            this.pnlBolge.Controls.Add(this.label3);
            this.pnlBolge.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBolge.Location = new System.Drawing.Point(0, 8);
            this.pnlBolge.Name = "pnlBolge";
            this.pnlBolge.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlBolge.Size = new System.Drawing.Size(364, 48);
            this.pnlBolge.TabIndex = 0;
            // 
            // canliIzlemeBolgeBox
            // 
            this.canliIzlemeBolgeBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canliIzlemeBolgeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.canliIzlemeBolgeBox.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.canliIzlemeBolgeBox.FormattingEnabled = true;
            this.canliIzlemeBolgeBox.Location = new System.Drawing.Point(90, 0);
            this.canliIzlemeBolgeBox.Name = "canliIzlemeBolgeBox";
            this.canliIzlemeBolgeBox.Size = new System.Drawing.Size(274, 29);
            this.canliIzlemeBolgeBox.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 36);
            this.label3.TabIndex = 8;
            this.label3.Text = "Bölge";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlPass
            // 
            this.pnlPass.Controls.Add(this.canliEkranSifre);
            this.pnlPass.Controls.Add(this.label2);
            this.pnlPass.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPass.Location = new System.Drawing.Point(0, 104);
            this.pnlPass.Name = "pnlPass";
            this.pnlPass.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlPass.Size = new System.Drawing.Size(364, 48);
            this.pnlPass.TabIndex = 2;
            // 
            // canliEkranSifre
            // 
            this.canliEkranSifre.BackColor = System.Drawing.Color.White;
            this.canliEkranSifre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.canliEkranSifre.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canliEkranSifre.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.canliEkranSifre.Location = new System.Drawing.Point(90, 0);
            this.canliEkranSifre.Name = "canliEkranSifre";
            this.canliEkranSifre.PasswordChar = '●';
            this.canliEkranSifre.Size = new System.Drawing.Size(274, 29);
            this.canliEkranSifre.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 36);
            this.label2.TabIndex = 3;
            this.label2.Text = "Şifre";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlUser
            // 
            this.pnlUser.Controls.Add(this.canliEkranKullaniciAdi);
            this.pnlUser.Controls.Add(this.label1);
            this.pnlUser.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlUser.Location = new System.Drawing.Point(0, 56);
            this.pnlUser.Name = "pnlUser";
            this.pnlUser.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlUser.Size = new System.Drawing.Size(364, 48);
            this.pnlUser.TabIndex = 1;
            // 
            // canliEkranKullaniciAdi
            // 
            this.canliEkranKullaniciAdi.BackColor = System.Drawing.Color.White;
            this.canliEkranKullaniciAdi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canliEkranKullaniciAdi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.canliEkranKullaniciAdi.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.canliEkranKullaniciAdi.FormattingEnabled = true;
            this.canliEkranKullaniciAdi.Location = new System.Drawing.Point(90, 0);
            this.canliEkranKullaniciAdi.Name = "canliEkranKullaniciAdi";
            this.canliEkranKullaniciAdi.Size = new System.Drawing.Size(274, 29);
            this.canliEkranKullaniciAdi.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 36);
            this.label1.TabIndex = 1;
            this.label1.Text = "Kullanıcı";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblSubtitle.Location = new System.Drawing.Point(28, 80);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Padding = new System.Windows.Forms.Padding(0, 4, 0, 8);
            this.lblSubtitle.Size = new System.Drawing.Size(364, 34);
            this.lblSubtitle.TabIndex = 2;
            this.lblSubtitle.Text = "Canlı izleme ekranına bağlanmak için bilgilerinizi girin.";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.lblTitle.Location = new System.Drawing.Point(28, 48);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(364, 32);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Canlı İzleme Girişi";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlLogo
            // 
            this.pnlLogo.Controls.Add(this.sirketLogo);
            this.pnlLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogo.Location = new System.Drawing.Point(28, 24);
            this.pnlLogo.Name = "pnlLogo";
            this.pnlLogo.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlLogo.Size = new System.Drawing.Size(364, 24);
            this.pnlLogo.TabIndex = 0;
            // 
            // sirketLogo
            // 
            this.sirketLogo.Dock = System.Windows.Forms.DockStyle.Left;
            this.sirketLogo.Image = global::CeyPASS.WFA.Properties.Resources.ceyLogo;
            this.sirketLogo.Location = new System.Drawing.Point(0, 0);
            this.sirketLogo.Name = "sirketLogo";
            this.sirketLogo.Size = new System.Drawing.Size(100, 16);
            this.sirketLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.sirketLogo.TabIndex = 5;
            this.sirketLogo.TabStop = false;
            // 
            // canliIzlemeGirisEkrani
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(480, 420);
            this.Controls.Add(this.pnlBackground);
            this.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(496, 459);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(496, 459);
            this.Name = "canliIzlemeGirisEkrani";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Canlı İzleme Ekranı Giriş";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.canliIzlemeGirisEkrani_FormClosing);
            this.Load += new System.EventHandler(this.canliIzlemeGirisEkrani_Load);
            this.pnlBackground.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.pnlForm.ResumeLayout(false);
            this.pnlBolge.ResumeLayout(false);
            this.pnlPass.ResumeLayout(false);
            this.pnlPass.PerformLayout();
            this.pnlUser.ResumeLayout(false);
            this.pnlUser.PerformLayout();
            this.pnlLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sirketLogo)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Panel pnlBackground;
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Panel pnlLogo;
        private System.Windows.Forms.Panel pnlForm;
        private System.Windows.Forms.Panel pnlUser;
        private System.Windows.Forms.Panel pnlPass;
        private System.Windows.Forms.Panel pnlBolge;

        private System.Windows.Forms.PictureBox sirketLogo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;

        private System.Windows.Forms.ComboBox canliEkranKullaniciAdi;
        private System.Windows.Forms.TextBox canliEkranSifre;
        private System.Windows.Forms.ComboBox canliIzlemeBolgeBox;
        private System.Windows.Forms.Button canliEkranGirisButon;


    }
}