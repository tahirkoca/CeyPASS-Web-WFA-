namespace CeyPASS.WFA.UserControls.Canlı_İzleme
{
    partial class misafirKartAtama
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

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.tlpForm = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbPuantajsizKartlar = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMisafirAdSoyad = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.dtpGirisSaati = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.dtpCikisSaati = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.txtAciklama = new System.Windows.Forms.TextBox();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnMisafirKayitIptal = new System.Windows.Forms.Button();
            this.btnMisafirKaydet = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.pnlMain.SuspendLayout();
            this.pnlCard.SuspendLayout();
            this.tlpForm.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.pnlMain.Controls.Add(this.pnlCard);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMain.Size = new System.Drawing.Size(600, 500);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlCard
            // 
            this.pnlCard.BackColor = System.Drawing.Color.White;
            this.pnlCard.Controls.Add(this.tlpForm);
            this.pnlCard.Controls.Add(this.pnlFooter);
            this.pnlCard.Controls.Add(this.lblHeader);
            this.pnlCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCard.Location = new System.Drawing.Point(10, 10);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(580, 480);
            this.pnlCard.TabIndex = 0;
            // 
            // tlpForm
            // 
            this.tlpForm.ColumnCount = 1;
            this.tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpForm.Controls.Add(this.label2, 0, 0);
            this.tlpForm.Controls.Add(this.cmbPuantajsizKartlar, 0, 1);
            this.tlpForm.Controls.Add(this.label3, 0, 2);
            this.tlpForm.Controls.Add(this.txtMisafirAdSoyad, 0, 3);
            this.tlpForm.Controls.Add(this.label4, 0, 4);
            this.tlpForm.Controls.Add(this.dtpGirisSaati, 0, 5);
            this.tlpForm.Controls.Add(this.label5, 0, 6);
            this.tlpForm.Controls.Add(this.dtpCikisSaati, 0, 7);
            this.tlpForm.Controls.Add(this.label6, 0, 8);
            this.tlpForm.Controls.Add(this.txtAciklama, 0, 9);
            this.tlpForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpForm.Location = new System.Drawing.Point(0, 50);
            this.tlpForm.Name = "tlpForm";
            this.tlpForm.Padding = new System.Windows.Forms.Padding(20, 10, 20, 0);
            this.tlpForm.RowCount = 10;
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F)); // Label
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F)); // Input
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F)); // Label
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F)); // Input
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F)); // Label
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F)); // Input
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F)); // Label
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F)); // Input
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F)); // Label
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)); // Açıklama (Kalan alanı kaplar)
            this.tlpForm.Size = new System.Drawing.Size(580, 360);
            this.tlpForm.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label2.Location = new System.Drawing.Point(23, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "Atanacak Kart";
            // 
            // cmbPuantajsizKartlar
            // 
            this.cmbPuantajsizKartlar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbPuantajsizKartlar.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbPuantajsizKartlar.FormattingEnabled = true;
            this.cmbPuantajsizKartlar.Location = new System.Drawing.Point(23, 35);
            this.cmbPuantajsizKartlar.Name = "cmbPuantajsizKartlar";
            this.cmbPuantajsizKartlar.Size = new System.Drawing.Size(534, 31);
            this.cmbPuantajsizKartlar.TabIndex = 1;
            this.cmbPuantajsizKartlar.SelectedIndexChanged += new System.EventHandler(this.cmbPuantajsizKartlar_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label3.Location = new System.Drawing.Point(23, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Misafir Adı Soyadı";
            // 
            // txtMisafirAdSoyad
            // 
            this.txtMisafirAdSoyad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMisafirAdSoyad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMisafirAdSoyad.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMisafirAdSoyad.Location = new System.Drawing.Point(23, 92);
            this.txtMisafirAdSoyad.Name = "txtMisafirAdSoyad";
            this.txtMisafirAdSoyad.Size = new System.Drawing.Size(534, 30);
            this.txtMisafirAdSoyad.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label4.Location = new System.Drawing.Point(23, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 20);
            this.label4.TabIndex = 4;
            this.label4.Text = "Giriş Saati";
            // 
            // dtpGirisSaati
            // 
            this.dtpGirisSaati.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpGirisSaati.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpGirisSaati.Location = new System.Drawing.Point(23, 149);
            this.dtpGirisSaati.Name = "dtpGirisSaati";
            this.dtpGirisSaati.Size = new System.Drawing.Size(534, 30);
            this.dtpGirisSaati.TabIndex = 5;
            this.dtpGirisSaati.ValueChanged += new System.EventHandler(this.dtpGirisSaati_ValueChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label5.Location = new System.Drawing.Point(23, 183);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 20);
            this.label5.TabIndex = 6;
            this.label5.Text = "Çıkış Saati";
            // 
            // dtpCikisSaati
            // 
            this.dtpCikisSaati.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpCikisSaati.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpCikisSaati.Location = new System.Drawing.Point(23, 206);
            this.dtpCikisSaati.Name = "dtpCikisSaati";
            this.dtpCikisSaati.Size = new System.Drawing.Size(534, 30);
            this.dtpCikisSaati.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label6.Location = new System.Drawing.Point(23, 240);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 20);
            this.label6.TabIndex = 8;
            this.label6.Text = "Açıklama";
            // 
            // txtAciklama
            // 
            this.txtAciklama.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAciklama.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAciklama.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtAciklama.Location = new System.Drawing.Point(23, 263);
            this.txtAciklama.Multiline = true;
            this.txtAciklama.Name = "txtAciklama";
            this.txtAciklama.Size = new System.Drawing.Size(534, 97);
            this.txtAciklama.TabIndex = 9;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlFooter.Controls.Add(this.btnMisafirKayitIptal);
            this.pnlFooter.Controls.Add(this.btnMisafirKaydet);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 410);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.pnlFooter.Size = new System.Drawing.Size(580, 70);
            this.pnlFooter.TabIndex = 2;
            // 
            // btnMisafirKayitIptal
            // 
            this.btnMisafirKayitIptal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnMisafirKayitIptal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMisafirKayitIptal.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnMisafirKayitIptal.FlatAppearance.BorderSize = 0;
            this.btnMisafirKayitIptal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMisafirKayitIptal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnMisafirKayitIptal.ForeColor = System.Drawing.Color.White;
            this.btnMisafirKayitIptal.Location = new System.Drawing.Point(20, 10);
            this.btnMisafirKayitIptal.Name = "btnMisafirKayitIptal";
            this.btnMisafirKayitIptal.Size = new System.Drawing.Size(110, 50);
            this.btnMisafirKayitIptal.TabIndex = 1;
            this.btnMisafirKayitIptal.Text = "İptal";
            this.btnMisafirKayitIptal.UseVisualStyleBackColor = false;
            this.btnMisafirKayitIptal.Click += new System.EventHandler(this.btnMisafirKayitIptal_Click);
            // 
            // btnMisafirKaydet
            // 
            this.btnMisafirKaydet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnMisafirKaydet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMisafirKaydet.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnMisafirKaydet.FlatAppearance.BorderSize = 0;
            this.btnMisafirKaydet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMisafirKaydet.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnMisafirKaydet.ForeColor = System.Drawing.Color.White;
            this.btnMisafirKaydet.Location = new System.Drawing.Point(450, 10);
            this.btnMisafirKaydet.Name = "btnMisafirKaydet";
            this.btnMisafirKaydet.Size = new System.Drawing.Size(110, 50);
            this.btnMisafirKaydet.TabIndex = 0;
            this.btnMisafirKaydet.Text = "Kaydet";
            this.btnMisafirKaydet.UseVisualStyleBackColor = false;
            this.btnMisafirKaydet.Click += new System.EventHandler(this.btnMisafirKaydet_Click);
            // 
            // lblHeader
            // 
            this.lblHeader.BackColor = System.Drawing.Color.White;
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblHeader.Size = new System.Drawing.Size(580, 50);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Misafir Kartı Atama";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // misafirKartAtama
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "misafirKartAtama";
            this.Size = new System.Drawing.Size(600, 500);
            this.Load += new System.EventHandler(this.misafirKartAtama_Load);
            this.pnlMain.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.tlpForm.ResumeLayout(false);
            this.tlpForm.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TableLayoutPanel tlpForm;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbPuantajsizKartlar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMisafirAdSoyad;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dtpGirisSaati;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtpCikisSaati;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtAciklama;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnMisafirKayitIptal;
        private System.Windows.Forms.Button btnMisafirKaydet;
    }
}
