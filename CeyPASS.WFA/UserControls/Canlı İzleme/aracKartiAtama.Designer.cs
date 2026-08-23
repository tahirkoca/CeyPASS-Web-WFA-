namespace CeyPASS.WFA.UserControls.Canlı_İzleme
{
    partial class aracKartiAtama
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.tlpForm = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbPuantajsizKartlar = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAdSoyad = new System.Windows.Forms.TextBox();
            this.lblTCKimlikNo = new System.Windows.Forms.Label();
            this.txtTCKimlikNo = new System.Windows.Forms.TextBox();
            this.lblPlaka = new System.Windows.Forms.Label();
            this.txtPlaka = new System.Windows.Forms.TextBox();
            this.lblKimeGeldigi = new System.Windows.Forms.Label();
            this.txtZiyaretEdilenKisi = new System.Windows.Forms.TextBox();
            this.lblTcBilgi = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dtpGirisSaati = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.dtpCikisSaati = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.txtAciklama = new System.Windows.Forms.TextBox();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnIptal = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.gecmisZiyaretciPanel = new CeyPASS.WFA.UserControls.Canlı_İzleme.gecmisZiyaretciPanel();
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
            this.pnlMain.Size = new System.Drawing.Size(1080, 600);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlCard
            // 
            this.pnlCard.BackColor = System.Drawing.Color.White;
            this.pnlCard.Controls.Add(this.tlpForm);
            this.pnlCard.Controls.Add(this.gecmisZiyaretciPanel);
            this.pnlCard.Controls.Add(this.pnlFooter);
            this.pnlCard.Controls.Add(this.lblHeader);
            this.pnlCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCard.Location = new System.Drawing.Point(10, 10);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(1060, 580);
            this.pnlCard.TabIndex = 0;
            // 
            // tlpForm
            // 
            this.tlpForm.ColumnCount = 1;
            this.tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpForm.Controls.Add(this.label2, 0, 0);
            this.tlpForm.Controls.Add(this.cmbPuantajsizKartlar, 0, 1);
            this.tlpForm.Controls.Add(this.label3, 0, 2);
            this.tlpForm.Controls.Add(this.txtAdSoyad, 0, 3);
            this.tlpForm.Controls.Add(this.lblTCKimlikNo, 0, 4);
            this.tlpForm.Controls.Add(this.txtTCKimlikNo, 0, 5);
            this.tlpForm.Controls.Add(this.lblPlaka, 0, 6);
            this.tlpForm.Controls.Add(this.txtPlaka, 0, 7);
            this.tlpForm.Controls.Add(this.lblKimeGeldigi, 0, 8);
            this.tlpForm.Controls.Add(this.txtZiyaretEdilenKisi, 0, 9);
            this.tlpForm.Controls.Add(this.lblTcBilgi, 0, 10);
            this.tlpForm.Controls.Add(this.label4, 0, 11);
            this.tlpForm.Controls.Add(this.dtpGirisSaati, 0, 12);
            this.tlpForm.Controls.Add(this.label5, 0, 13);
            this.tlpForm.Controls.Add(this.dtpCikisSaati, 0, 14);
            this.tlpForm.Controls.Add(this.label6, 0, 15);
            this.tlpForm.Controls.Add(this.txtAciklama, 0, 16);
            this.tlpForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpForm.Location = new System.Drawing.Point(0, 50);
            this.tlpForm.Name = "tlpForm";
            this.tlpForm.Padding = new System.Windows.Forms.Padding(20, 10, 20, 0);
            this.tlpForm.RowCount = 17;
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpForm.Size = new System.Drawing.Size(700, 510);
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
            this.cmbPuantajsizKartlar.Size = new System.Drawing.Size(654, 31);
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
            this.label3.Size = new System.Drawing.Size(72, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Ad Soyad";
            // 
            // txtAdSoyad
            // 
            this.txtAdSoyad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAdSoyad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAdSoyad.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtAdSoyad.Location = new System.Drawing.Point(23, 92);
            this.txtAdSoyad.Name = "txtAdSoyad";
            this.txtAdSoyad.Size = new System.Drawing.Size(654, 30);
            this.txtAdSoyad.TabIndex = 3;
            // 
            // lblTCKimlikNo
            // 
            this.lblTCKimlikNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTCKimlikNo.AutoSize = true;
            this.lblTCKimlikNo.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTCKimlikNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblTCKimlikNo.Location = new System.Drawing.Point(23, 126);
            this.lblTCKimlikNo.Name = "lblTCKimlikNo";
            this.lblTCKimlikNo.Size = new System.Drawing.Size(109, 20);
            this.lblTCKimlikNo.TabIndex = 4;
            this.lblTCKimlikNo.Text = "T.C. Kimlik No *";
            // 
            // txtTCKimlikNo
            // 
            this.txtTCKimlikNo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTCKimlikNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTCKimlikNo.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTCKimlikNo.Location = new System.Drawing.Point(23, 149);
            this.txtTCKimlikNo.MaxLength = 11;
            this.txtTCKimlikNo.Name = "txtTCKimlikNo";
            this.txtTCKimlikNo.Size = new System.Drawing.Size(654, 30);
            this.txtTCKimlikNo.TabIndex = 5;
            // 
            // lblPlaka
            // 
            this.lblPlaka.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPlaka.AutoSize = true;
            this.lblPlaka.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblPlaka.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblPlaka.Location = new System.Drawing.Point(23, 183);
            this.lblPlaka.Name = "lblPlaka";
            this.lblPlaka.Size = new System.Drawing.Size(95, 20);
            this.lblPlaka.TabIndex = 6;
            this.lblPlaka.Text = "Araç Plakası *";
            // 
            // txtPlaka
            // 
            this.txtPlaka.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPlaka.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPlaka.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPlaka.Location = new System.Drawing.Point(23, 206);
            this.txtPlaka.MaxLength = 20;
            this.txtPlaka.Name = "txtPlaka";
            this.txtPlaka.Size = new System.Drawing.Size(654, 30);
            this.txtPlaka.TabIndex = 7;
            // 
            // lblKimeGeldigi
            // 
            this.lblKimeGeldigi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblKimeGeldigi.AutoSize = true;
            this.lblKimeGeldigi.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblKimeGeldigi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblKimeGeldigi.Location = new System.Drawing.Point(23, 240);
            this.lblKimeGeldigi.Name = "lblKimeGeldigi";
            this.lblKimeGeldigi.Size = new System.Drawing.Size(97, 20);
            this.lblKimeGeldigi.TabIndex = 8;
            this.lblKimeGeldigi.Text = "Kime Geldiği";
            // 
            // txtZiyaretEdilenKisi
            // 
            this.txtZiyaretEdilenKisi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtZiyaretEdilenKisi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtZiyaretEdilenKisi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtZiyaretEdilenKisi.Location = new System.Drawing.Point(23, 263);
            this.txtZiyaretEdilenKisi.Name = "txtZiyaretEdilenKisi";
            this.txtZiyaretEdilenKisi.Size = new System.Drawing.Size(654, 30);
            this.txtZiyaretEdilenKisi.TabIndex = 9;
            // 
            // lblTcBilgi
            // 
            this.lblTcBilgi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTcBilgi.AutoSize = true;
            this.lblTcBilgi.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblTcBilgi.ForeColor = System.Drawing.Color.Gray;
            this.lblTcBilgi.Location = new System.Drawing.Point(23, 296);
            this.lblTcBilgi.MaximumSize = new System.Drawing.Size(654, 0);
            this.lblTcBilgi.Name = "lblTcBilgi";
            this.lblTcBilgi.Size = new System.Drawing.Size(420, 19);
            this.lblTcBilgi.TabIndex = 10;
            this.lblTcBilgi.Text = "T.C. Kimlik No (11 hane) ve plaka zorunludur; T.C.'yi elle ya da barkodla girebilirsiniz. T.C. girip alandan çıkınca veya Enter'a basınca boş alanlar otomatik doldurulur, dolu alanlar değiştirilmez.";
            this.lblTcBilgi.UseMnemonic = false;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label4.Location = new System.Drawing.Point(23, 324);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 20);
            this.label4.TabIndex = 11;
            this.label4.Text = "Giriş Saati";
            // 
            // dtpGirisSaati
            // 
            this.dtpGirisSaati.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpGirisSaati.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpGirisSaati.Location = new System.Drawing.Point(23, 347);
            this.dtpGirisSaati.Name = "dtpGirisSaati";
            this.dtpGirisSaati.Size = new System.Drawing.Size(654, 30);
            this.dtpGirisSaati.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label5.Location = new System.Drawing.Point(23, 381);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 20);
            this.label5.TabIndex = 13;
            this.label5.Text = "Çıkış Saati";
            // 
            // dtpCikisSaati
            // 
            this.dtpCikisSaati.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpCikisSaati.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpCikisSaati.Location = new System.Drawing.Point(23, 404);
            this.dtpCikisSaati.Name = "dtpCikisSaati";
            this.dtpCikisSaati.Size = new System.Drawing.Size(654, 30);
            this.dtpCikisSaati.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label6.Location = new System.Drawing.Point(23, 438);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 20);
            this.label6.TabIndex = 15;
            this.label6.Text = "Açıklama";
            // 
            // txtAciklama
            // 
            this.txtAciklama.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAciklama.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAciklama.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtAciklama.Location = new System.Drawing.Point(23, 461);
            this.txtAciklama.Multiline = true;
            this.txtAciklama.Name = "txtAciklama";
            this.txtAciklama.Size = new System.Drawing.Size(654, 46);
            this.txtAciklama.TabIndex = 16;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlFooter.Controls.Add(this.btnIptal);
            this.pnlFooter.Controls.Add(this.btnKaydet);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 530);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.pnlFooter.Size = new System.Drawing.Size(1060, 70);
            this.pnlFooter.TabIndex = 2;
            // 
            // btnIptal
            // 
            this.btnIptal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnIptal.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIptal.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnIptal.FlatAppearance.BorderSize = 0;
            this.btnIptal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIptal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnIptal.ForeColor = System.Drawing.Color.White;
            this.btnIptal.Location = new System.Drawing.Point(20, 10);
            this.btnIptal.Name = "btnIptal";
            this.btnIptal.Size = new System.Drawing.Size(110, 50);
            this.btnIptal.TabIndex = 1;
            this.btnIptal.Text = "İptal";
            this.btnIptal.UseVisualStyleBackColor = false;
            this.btnIptal.Click += new System.EventHandler(this.btnIptal_Click);
            // 
            // btnKaydet
            // 
            this.btnKaydet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(126)))), ((int)(((byte)(20)))));
            this.btnKaydet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnKaydet.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnKaydet.FlatAppearance.BorderSize = 0;
            this.btnKaydet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKaydet.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnKaydet.ForeColor = System.Drawing.Color.White;
            this.btnKaydet.Location = new System.Drawing.Point(570, 10);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(110, 50);
            this.btnKaydet.TabIndex = 0;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = false;
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);
            // 
            // lblHeader
            // 
            this.lblHeader.BackColor = System.Drawing.Color.White;
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblHeader.Size = new System.Drawing.Size(1060, 50);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Araç Kartı Ver";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gecmisZiyaretciPanel
            // 
            this.gecmisZiyaretciPanel.Location = new System.Drawing.Point(0, 50);
            this.gecmisZiyaretciPanel.Name = "gecmisZiyaretciPanel";
            this.gecmisZiyaretciPanel.Size = new System.Drawing.Size(360, 460);
            this.gecmisZiyaretciPanel.TabIndex = 3;
            this.gecmisZiyaretciPanel.Visible = false;
            // 
            // aracKartiAtama
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "aracKartiAtama";
            this.Size = new System.Drawing.Size(1080, 600);
            this.Load += new System.EventHandler(this.aracKartiAtama_Load);
            this.pnlMain.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.tlpForm.ResumeLayout(false);
            this.tlpForm.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TableLayoutPanel tlpForm;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbPuantajsizKartlar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAdSoyad;
        private System.Windows.Forms.Label lblTCKimlikNo;
        private System.Windows.Forms.TextBox txtTCKimlikNo;
        private System.Windows.Forms.Label lblPlaka;
        private System.Windows.Forms.TextBox txtPlaka;
        private System.Windows.Forms.Label lblKimeGeldigi;
        private System.Windows.Forms.TextBox txtZiyaretEdilenKisi;
        private System.Windows.Forms.Label lblTcBilgi;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dtpGirisSaati;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtpCikisSaati;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtAciklama;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnIptal;
        private System.Windows.Forms.Button btnKaydet;
        private gecmisZiyaretciPanel gecmisZiyaretciPanel;
    }
}
