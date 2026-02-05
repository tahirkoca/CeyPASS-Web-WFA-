namespace CeyPASS.WFA
{
    partial class sifremiUnuttumEkrani
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(sifremiUnuttumEkrani));
            this.pnlBackground = new System.Windows.Forms.Panel();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.btnYeniSifreKaydet = new System.Windows.Forms.Button();
            this.pnlForm = new System.Windows.Forms.Panel();
            this.pnlYeniSifreTekrar = new System.Windows.Forms.Panel();
            this.txtYeniSifreKontrol = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pnlYeniSifre = new System.Windows.Forms.Panel();
            this.txtYeniSifre = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pnlDogrulama = new System.Windows.Forms.Panel();
            this.txtDogrulamaKodu = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlBackground.SuspendLayout();
            this.pnlCard.SuspendLayout();
            this.pnlForm.SuspendLayout();
            this.pnlYeniSifreTekrar.SuspendLayout();
            this.pnlYeniSifre.SuspendLayout();
            this.pnlDogrulama.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(239)))), ((int)(((byte)(242)))));
            this.pnlBackground.Controls.Add(this.pnlCard);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(478, 352);
            this.pnlBackground.TabIndex = 0;
            // 
            // pnlCard
            // 
            this.pnlCard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlCard.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlCard.Controls.Add(this.btnYeniSifreKaydet);
            this.pnlCard.Controls.Add(this.pnlForm);
            this.pnlCard.Controls.Add(this.lblSubtitle);
            this.pnlCard.Controls.Add(this.lblTitle);
            this.pnlCard.Location = new System.Drawing.Point(39, 26);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Padding = new System.Windows.Forms.Padding(25);
            this.pnlCard.Size = new System.Drawing.Size(400, 300);
            this.pnlCard.TabIndex = 0;
            // 
            // btnYeniSifreKaydet
            // 
            this.btnYeniSifreKaydet.BackColor = System.Drawing.Color.Red;
            this.btnYeniSifreKaydet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnYeniSifreKaydet.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnYeniSifreKaydet.FlatAppearance.BorderSize = 0;
            this.btnYeniSifreKaydet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnYeniSifreKaydet.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnYeniSifreKaydet.ForeColor = System.Drawing.Color.White;
            this.btnYeniSifreKaydet.Location = new System.Drawing.Point(25, 230);
            this.btnYeniSifreKaydet.Name = "btnYeniSifreKaydet";
            this.btnYeniSifreKaydet.Size = new System.Drawing.Size(350, 45);
            this.btnYeniSifreKaydet.TabIndex = 3;
            this.btnYeniSifreKaydet.Text = "KAYDET";
            this.btnYeniSifreKaydet.UseVisualStyleBackColor = false;
            this.btnYeniSifreKaydet.Click += new System.EventHandler(this.btnYeniSifreKaydet_Click);
            // 
            // pnlForm
            // 
            this.pnlForm.Controls.Add(this.pnlYeniSifreTekrar);
            this.pnlForm.Controls.Add(this.pnlYeniSifre);
            this.pnlForm.Controls.Add(this.pnlDogrulama);
            this.pnlForm.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlForm.Location = new System.Drawing.Point(25, 85);
            this.pnlForm.Name = "pnlForm";
            this.pnlForm.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.pnlForm.Size = new System.Drawing.Size(350, 145);
            this.pnlForm.TabIndex = 2;
            // 
            // pnlYeniSifreTekrar
            // 
            this.pnlYeniSifreTekrar.Controls.Add(this.txtYeniSifreKontrol);
            this.pnlYeniSifreTekrar.Controls.Add(this.label4);
            this.pnlYeniSifreTekrar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlYeniSifreTekrar.Location = new System.Drawing.Point(0, 96);
            this.pnlYeniSifreTekrar.Name = "pnlYeniSifreTekrar";
            this.pnlYeniSifreTekrar.Size = new System.Drawing.Size(350, 48);
            this.pnlYeniSifreTekrar.TabIndex = 2;
            // 
            // txtYeniSifreKontrol
            // 
            this.txtYeniSifreKontrol.BackColor = System.Drawing.Color.White;
            this.txtYeniSifreKontrol.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtYeniSifreKontrol.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtYeniSifreKontrol.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtYeniSifreKontrol.Location = new System.Drawing.Point(0, 16);
            this.txtYeniSifreKontrol.Name = "txtYeniSifreKontrol";
            this.txtYeniSifreKontrol.Size = new System.Drawing.Size(350, 32);
            this.txtYeniSifreKontrol.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Top;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(350, 22);
            this.label4.TabIndex = 0;
            this.label4.Text = "Yeni Şifre (Tekrar)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // pnlYeniSifre
            // 
            this.pnlYeniSifre.Controls.Add(this.txtYeniSifre);
            this.pnlYeniSifre.Controls.Add(this.label3);
            this.pnlYeniSifre.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlYeniSifre.Location = new System.Drawing.Point(0, 48);
            this.pnlYeniSifre.Name = "pnlYeniSifre";
            this.pnlYeniSifre.Size = new System.Drawing.Size(350, 48);
            this.pnlYeniSifre.TabIndex = 1;
            // 
            // txtYeniSifre
            // 
            this.txtYeniSifre.BackColor = System.Drawing.Color.White;
            this.txtYeniSifre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtYeniSifre.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtYeniSifre.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtYeniSifre.Location = new System.Drawing.Point(0, 16);
            this.txtYeniSifre.Name = "txtYeniSifre";
            this.txtYeniSifre.Size = new System.Drawing.Size(350, 32);
            this.txtYeniSifre.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(350, 22);
            this.label3.TabIndex = 0;
            this.label3.Text = "Yeni Şifre";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // pnlDogrulama
            // 
            this.pnlDogrulama.Controls.Add(this.txtDogrulamaKodu);
            this.pnlDogrulama.Controls.Add(this.label1);
            this.pnlDogrulama.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDogrulama.Location = new System.Drawing.Point(0, 5);
            this.pnlDogrulama.Name = "pnlDogrulama";
            this.pnlDogrulama.Size = new System.Drawing.Size(350, 43);
            this.pnlDogrulama.TabIndex = 0;
            // 
            // txtDogrulamaKodu
            // 
            this.txtDogrulamaKodu.BackColor = System.Drawing.Color.White;
            this.txtDogrulamaKodu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDogrulamaKodu.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtDogrulamaKodu.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtDogrulamaKodu.Location = new System.Drawing.Point(0, 11);
            this.txtDogrulamaKodu.Name = "txtDogrulamaKodu";
            this.txtDogrulamaKodu.Size = new System.Drawing.Size(350, 32);
            this.txtDogrulamaKodu.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(350, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Doğrulama Kodu";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.lblSubtitle.Location = new System.Drawing.Point(25, 55);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.lblSubtitle.Size = new System.Drawing.Size(350, 30);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Mailinize gelen doğrulama kodunu ve yeni şifrenizi girin.";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.lblTitle.Location = new System.Drawing.Point(25, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(350, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Şifre Yenileme";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sifremiUnuttumEkrani
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 352);
            this.Controls.Add(this.pnlBackground);
            this.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(496, 399);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(496, 399);
            this.Name = "sifremiUnuttumEkrani";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Şifremi Unuttum";
            this.Load += new System.EventHandler(this.sifremiUnuttumEkrani_Load);
            this.pnlBackground.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.pnlForm.ResumeLayout(false);
            this.pnlYeniSifreTekrar.ResumeLayout(false);
            this.pnlYeniSifreTekrar.PerformLayout();
            this.pnlYeniSifre.ResumeLayout(false);
            this.pnlYeniSifre.PerformLayout();
            this.pnlDogrulama.ResumeLayout(false);
            this.pnlDogrulama.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Panel pnlBackground;
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Panel pnlForm;
        private System.Windows.Forms.Panel pnlDogrulama;
        private System.Windows.Forms.Panel pnlYeniSifre;
        private System.Windows.Forms.Panel pnlYeniSifreTekrar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnYeniSifreKaydet;
        private System.Windows.Forms.TextBox txtYeniSifreKontrol;
        private System.Windows.Forms.TextBox txtYeniSifre;
        private System.Windows.Forms.TextBox txtDogrulamaKodu;

    }
}