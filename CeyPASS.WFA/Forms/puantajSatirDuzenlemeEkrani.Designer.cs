namespace CeyPASS.WFA.Forms
{
    partial class puantajSatirDuzenlemeEkrani
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(puantajSatirDuzenlemeEkrani));
            this.pnlBackground = new System.Windows.Forms.Panel();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnIptal = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.pnlAciklama = new System.Windows.Forms.Panel();
            this.txtAciklama = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pnlCalismaSaati = new System.Windows.Forms.Panel();
            this.nudCalismaSaati = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.pnlCalismaTipi = new System.Windows.Forms.Panel();
            this.cmbCalismaTipi = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pnlTarih = new System.Windows.Forms.Panel();
            this.lblTarih = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblHeader = new System.Windows.Forms.Label();
            this.pnlBackground.SuspendLayout();
            this.pnlCard.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlAciklama.SuspendLayout();
            this.pnlCalismaSaati.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCalismaSaati)).BeginInit();
            this.pnlCalismaTipi.SuspendLayout();
            this.pnlTarih.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(239)))), ((int)(((byte)(242)))));
            this.pnlBackground.Controls.Add(this.pnlCard);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(518, 352);
            this.pnlBackground.TabIndex = 0;
            // 
            // pnlCard
            // 
            this.pnlCard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlCard.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlCard.Controls.Add(this.pnlButtons);
            this.pnlCard.Controls.Add(this.pnlAciklama);
            this.pnlCard.Controls.Add(this.pnlCalismaSaati);
            this.pnlCard.Controls.Add(this.pnlCalismaTipi);
            this.pnlCard.Controls.Add(this.pnlTarih);
            this.pnlCard.Controls.Add(this.lblHeader);
            this.pnlCard.Location = new System.Drawing.Point(39, 26);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Padding = new System.Windows.Forms.Padding(20);
            this.pnlCard.Size = new System.Drawing.Size(440, 300);
            this.pnlCard.TabIndex = 0;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnIptal);
            this.pnlButtons.Controls.Add(this.btnKaydet);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(20, 236);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.pnlButtons.Size = new System.Drawing.Size(400, 44);
            this.pnlButtons.TabIndex = 5;
            // 
            // btnIptal
            // 
            this.btnIptal.BackColor = System.Drawing.Color.Red;
            this.btnIptal.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnIptal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnIptal.ForeColor = System.Drawing.Color.White;
            this.btnIptal.Location = new System.Drawing.Point(210, 10);
            this.btnIptal.Name = "btnIptal";
            this.btnIptal.Size = new System.Drawing.Size(190, 34);
            this.btnIptal.TabIndex = 4;
            this.btnIptal.Text = "İptal";
            this.btnIptal.UseVisualStyleBackColor = false;
            this.btnIptal.Click += new System.EventHandler(this.btnIptal_Click);
            // 
            // btnKaydet
            // 
            this.btnKaydet.BackColor = System.Drawing.Color.Gold;
            this.btnKaydet.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnKaydet.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnKaydet.Location = new System.Drawing.Point(0, 10);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(190, 34);
            this.btnKaydet.TabIndex = 3;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = false;
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);
            // 
            // pnlAciklama
            // 
            this.pnlAciklama.Controls.Add(this.txtAciklama);
            this.pnlAciklama.Controls.Add(this.label4);
            this.pnlAciklama.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAciklama.Location = new System.Drawing.Point(20, 160);
            this.pnlAciklama.Name = "pnlAciklama";
            this.pnlAciklama.Size = new System.Drawing.Size(400, 76);
            this.pnlAciklama.TabIndex = 4;
            // 
            // txtAciklama
            // 
            this.txtAciklama.BackColor = System.Drawing.Color.White;
            this.txtAciklama.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAciklama.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAciklama.Location = new System.Drawing.Point(110, 0);
            this.txtAciklama.Multiline = true;
            this.txtAciklama.Name = "txtAciklama";
            this.txtAciklama.Size = new System.Drawing.Size(290, 76);
            this.txtAciklama.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Left;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 76);
            this.label4.TabIndex = 7;
            this.label4.Text = "Açıklama";
            // 
            // pnlCalismaSaati
            // 
            this.pnlCalismaSaati.Controls.Add(this.nudCalismaSaati);
            this.pnlCalismaSaati.Controls.Add(this.label3);
            this.pnlCalismaSaati.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCalismaSaati.Location = new System.Drawing.Point(20, 125);
            this.pnlCalismaSaati.Name = "pnlCalismaSaati";
            this.pnlCalismaSaati.Size = new System.Drawing.Size(400, 35);
            this.pnlCalismaSaati.TabIndex = 3;
            // 
            // nudCalismaSaati
            // 
            this.nudCalismaSaati.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudCalismaSaati.DecimalPlaces = 2;
            this.nudCalismaSaati.Location = new System.Drawing.Point(113, 7);
            this.nudCalismaSaati.Name = "nudCalismaSaati";
            this.nudCalismaSaati.Size = new System.Drawing.Size(284, 25);
            this.nudCalismaSaati.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 35);
            this.label3.TabIndex = 6;
            this.label3.Text = "Çalışma Saati";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlCalismaTipi
            // 
            this.pnlCalismaTipi.Controls.Add(this.cmbCalismaTipi);
            this.pnlCalismaTipi.Controls.Add(this.label2);
            this.pnlCalismaTipi.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCalismaTipi.Location = new System.Drawing.Point(20, 90);
            this.pnlCalismaTipi.Name = "pnlCalismaTipi";
            this.pnlCalismaTipi.Size = new System.Drawing.Size(400, 35);
            this.pnlCalismaTipi.TabIndex = 2;
            // 
            // cmbCalismaTipi
            // 
            this.cmbCalismaTipi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCalismaTipi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCalismaTipi.FormattingEnabled = true;
            this.cmbCalismaTipi.Location = new System.Drawing.Point(113, 6);
            this.cmbCalismaTipi.Name = "cmbCalismaTipi";
            this.cmbCalismaTipi.Size = new System.Drawing.Size(284, 25);
            this.cmbCalismaTipi.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 35);
            this.label2.TabIndex = 1;
            this.label2.Text = "Çalışma Tipi";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlTarih
            // 
            this.pnlTarih.Controls.Add(this.lblTarih);
            this.pnlTarih.Controls.Add(this.label1);
            this.pnlTarih.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTarih.Location = new System.Drawing.Point(20, 55);
            this.pnlTarih.Name = "pnlTarih";
            this.pnlTarih.Size = new System.Drawing.Size(400, 35);
            this.pnlTarih.TabIndex = 1;
            // 
            // lblTarih
            // 
            this.lblTarih.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTarih.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblTarih.Location = new System.Drawing.Point(110, 0);
            this.lblTarih.Name = "lblTarih";
            this.lblTarih.Size = new System.Drawing.Size(290, 35);
            this.lblTarih.TabIndex = 0;
            this.lblTarih.Text = "Tarih";
            this.lblTarih.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 35);
            this.label1.TabIndex = 0;
            this.label1.Text = "Seçili Tarih";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.lblHeader.Location = new System.Drawing.Point(20, 20);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(400, 35);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Puantaj Satır Düzenleme";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // puantajSatirDuzenlemeEkrani
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(518, 352);
            this.Controls.Add(this.pnlBackground);
            this.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(536, 399);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(536, 399);
            this.Name = "puantajSatirDuzenlemeEkrani";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Puantaj Satır Düzenleyici";
            this.Load += new System.EventHandler(this.puantajSatirDuzenlemeEkrani_Load);
            this.pnlBackground.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlAciklama.ResumeLayout(false);
            this.pnlAciklama.PerformLayout();
            this.pnlCalismaSaati.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudCalismaSaati)).EndInit();
            this.pnlCalismaTipi.ResumeLayout(false);
            this.pnlTarih.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlBackground;
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Panel pnlAciklama;
        private System.Windows.Forms.Panel pnlCalismaSaati;
        private System.Windows.Forms.Panel pnlCalismaTipi;
        private System.Windows.Forms.Panel pnlTarih;
        private System.Windows.Forms.Label lblHeader;

        private System.Windows.Forms.Label lblTarih;
        private System.Windows.Forms.NumericUpDown nudCalismaSaati;
        private System.Windows.Forms.TextBox txtAciklama;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Button btnIptal;
        private System.Windows.Forms.ComboBox cmbCalismaTipi;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}