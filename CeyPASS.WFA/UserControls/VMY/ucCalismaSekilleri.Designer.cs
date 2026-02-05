namespace CeyPASS.WFA.UserControls.VMY
{
    partial class ucCalismaSekilleri
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
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.tlpForm = new System.Windows.Forms.TableLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.txtVardiyaAdi = new System.Windows.Forms.TextBox();
            this.lblDescVardiyaAdi = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpVardiyaBaslangicSaati = new System.Windows.Forms.DateTimePicker();
            this.lblDescBaslangic = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpVardiyaBitisSaati = new System.Windows.Forms.DateTimePicker();
            this.lblDescBitis = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpVardiyaBaslangicToleransSaati = new System.Windows.Forms.DateTimePicker();
            this.lblDescBaslangicTol = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dtpVardiyaBitisToleransSaati = new System.Windows.Forms.DateTimePicker();
            this.lblDescBitisTol = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.dtpYemekAktiflemeSaati = new System.Windows.Forms.DateTimePicker();
            this.lblDescYemek = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnVazgec = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.pnlToolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnVardiyaGuncelle = new System.Windows.Forms.Button();
            this.btnVardiyaSil = new System.Windows.Forms.Button();
            this.btnVardiyaEkle = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.chkVardiyalar = new System.Windows.Forms.CheckedListBox();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.lblListHeader = new System.Windows.Forms.Label();
            this.pnlMain.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlCard.SuspendLayout();
            this.tlpForm.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlLeftHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(245)))));
            this.pnlMain.Controls.Add(this.pnlContent);
            this.pnlMain.Controls.Add(this.pnlLeft);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMain.Size = new System.Drawing.Size(1200, 800);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.pnlCard);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(310, 10);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.pnlContent.Size = new System.Drawing.Size(880, 780);
            this.pnlContent.TabIndex = 1;
            // 
            // pnlCard
            // 
            this.pnlCard.BackColor = System.Drawing.Color.White;
            this.pnlCard.Controls.Add(this.tlpForm);
            this.pnlCard.Controls.Add(this.pnlFooter);
            this.pnlCard.Controls.Add(this.pnlToolbar);
            this.pnlCard.Controls.Add(this.lblHeader);
            this.pnlCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCard.Location = new System.Drawing.Point(20, 0);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(860, 780);
            this.pnlCard.TabIndex = 0;
            // 
            // tlpForm
            // 
            this.tlpForm.ColumnCount = 2;
            this.tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpForm.Controls.Add(this.label8, 0, 0);
            this.tlpForm.Controls.Add(this.txtVardiyaAdi, 0, 1);
            this.tlpForm.Controls.Add(this.lblDescVardiyaAdi, 1, 1);
            this.tlpForm.Controls.Add(this.label1, 0, 2);
            this.tlpForm.Controls.Add(this.dtpVardiyaBaslangicSaati, 0, 3);
            this.tlpForm.Controls.Add(this.lblDescBaslangic, 1, 3);
            this.tlpForm.Controls.Add(this.label2, 0, 4);
            this.tlpForm.Controls.Add(this.dtpVardiyaBitisSaati, 0, 5);
            this.tlpForm.Controls.Add(this.lblDescBitis, 1, 5);
            this.tlpForm.Controls.Add(this.label3, 0, 6);
            this.tlpForm.Controls.Add(this.dtpVardiyaBaslangicToleransSaati, 0, 7);
            this.tlpForm.Controls.Add(this.lblDescBaslangicTol, 1, 7);
            this.tlpForm.Controls.Add(this.label4, 0, 8);
            this.tlpForm.Controls.Add(this.dtpVardiyaBitisToleransSaati, 0, 9);
            this.tlpForm.Controls.Add(this.lblDescBitisTol, 1, 9);
            this.tlpForm.Controls.Add(this.label5, 0, 10);
            this.tlpForm.Controls.Add(this.dtpYemekAktiflemeSaati, 0, 11);
            this.tlpForm.Controls.Add(this.lblDescYemek, 1, 11);
            this.tlpForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpForm.Location = new System.Drawing.Point(0, 213);
            this.tlpForm.Name = "tlpForm";
            this.tlpForm.Padding = new System.Windows.Forms.Padding(40, 10, 40, 0);
            this.tlpForm.RowCount = 12;
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpForm.Size = new System.Drawing.Size(860, 467);
            this.tlpForm.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label8.ForeColor = System.Drawing.Color.Gray;
            this.label8.Location = new System.Drawing.Point(43, 12);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(100, 23);
            this.label8.TabIndex = 0;
            this.label8.Text = "Vardiya Adı";
            // 
            // txtVardiyaAdi
            // 
            this.txtVardiyaAdi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtVardiyaAdi.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtVardiyaAdi.Location = new System.Drawing.Point(43, 38);
            this.txtVardiyaAdi.Name = "txtVardiyaAdi";
            this.txtVardiyaAdi.Size = new System.Drawing.Size(306, 32);
            this.txtVardiyaAdi.TabIndex = 1;
            // 
            // lblDescVardiyaAdi
            // 
            this.lblDescVardiyaAdi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescVardiyaAdi.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblDescVardiyaAdi.ForeColor = System.Drawing.Color.DarkGray;
            this.lblDescVardiyaAdi.Location = new System.Drawing.Point(355, 35);
            this.lblDescVardiyaAdi.Name = "lblDescVardiyaAdi";
            this.lblDescVardiyaAdi.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblDescVardiyaAdi.Size = new System.Drawing.Size(462, 40);
            this.lblDescVardiyaAdi.TabIndex = 2;
            this.lblDescVardiyaAdi.Text = "* Vardiyanın sistemde görünecek adı. (Örn: Genel Vardiya)";
            this.lblDescVardiyaAdi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(43, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 3;
            this.label1.Text = "Vardiya Başlangıç Saati";
            // 
            // dtpVardiyaBaslangicSaati
            // 
            this.dtpVardiyaBaslangicSaati.CustomFormat = "HH:mm";
            this.dtpVardiyaBaslangicSaati.Dock = System.Windows.Forms.DockStyle.Left;
            this.dtpVardiyaBaslangicSaati.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.dtpVardiyaBaslangicSaati.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpVardiyaBaslangicSaati.Location = new System.Drawing.Point(43, 103);
            this.dtpVardiyaBaslangicSaati.Name = "dtpVardiyaBaslangicSaati";
            this.dtpVardiyaBaslangicSaati.ShowUpDown = true;
            this.dtpVardiyaBaslangicSaati.Size = new System.Drawing.Size(150, 32);
            this.dtpVardiyaBaslangicSaati.TabIndex = 4;
            // 
            // lblDescBaslangic
            // 
            this.lblDescBaslangic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescBaslangic.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblDescBaslangic.ForeColor = System.Drawing.Color.DarkGray;
            this.lblDescBaslangic.Location = new System.Drawing.Point(355, 100);
            this.lblDescBaslangic.Name = "lblDescBaslangic";
            this.lblDescBaslangic.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblDescBaslangic.Size = new System.Drawing.Size(462, 40);
            this.lblDescBaslangic.TabIndex = 5;
            this.lblDescBaslangic.Text = "* Mesainin resmi başlangıç saati.";
            this.lblDescBaslangic.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(43, 142);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 6;
            this.label2.Text = "Vardiya Bitiş Saati";
            // 
            // dtpVardiyaBitisSaati
            // 
            this.dtpVardiyaBitisSaati.CustomFormat = "HH:mm";
            this.dtpVardiyaBitisSaati.Dock = System.Windows.Forms.DockStyle.Left;
            this.dtpVardiyaBitisSaati.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.dtpVardiyaBitisSaati.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpVardiyaBitisSaati.Location = new System.Drawing.Point(43, 168);
            this.dtpVardiyaBitisSaati.Name = "dtpVardiyaBitisSaati";
            this.dtpVardiyaBitisSaati.ShowUpDown = true;
            this.dtpVardiyaBitisSaati.Size = new System.Drawing.Size(150, 32);
            this.dtpVardiyaBitisSaati.TabIndex = 7;
            // 
            // lblDescBitis
            // 
            this.lblDescBitis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescBitis.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblDescBitis.ForeColor = System.Drawing.Color.DarkGray;
            this.lblDescBitis.Location = new System.Drawing.Point(355, 165);
            this.lblDescBitis.Name = "lblDescBitis";
            this.lblDescBitis.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblDescBitis.Size = new System.Drawing.Size(462, 40);
            this.lblDescBitis.TabIndex = 8;
            this.lblDescBitis.Text = "* Mesainin resmi bitiş saati.";
            this.lblDescBitis.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.Gray;
            this.label3.Location = new System.Drawing.Point(43, 207);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 9;
            this.label3.Text = "Vardiya Başlangıç Tolerans Saati";
            // 
            // dtpVardiyaBaslangicToleransSaati
            // 
            this.dtpVardiyaBaslangicToleransSaati.CustomFormat = "HH:mm";
            this.dtpVardiyaBaslangicToleransSaati.Dock = System.Windows.Forms.DockStyle.Left;
            this.dtpVardiyaBaslangicToleransSaati.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.dtpVardiyaBaslangicToleransSaati.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpVardiyaBaslangicToleransSaati.Location = new System.Drawing.Point(43, 233);
            this.dtpVardiyaBaslangicToleransSaati.Name = "dtpVardiyaBaslangicToleransSaati";
            this.dtpVardiyaBaslangicToleransSaati.ShowUpDown = true;
            this.dtpVardiyaBaslangicToleransSaati.Size = new System.Drawing.Size(150, 32);
            this.dtpVardiyaBaslangicToleransSaati.TabIndex = 10;
            // 
            // lblDescBaslangicTol
            // 
            this.lblDescBaslangicTol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescBaslangicTol.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblDescBaslangicTol.ForeColor = System.Drawing.Color.DarkGray;
            this.lblDescBaslangicTol.Location = new System.Drawing.Point(355, 230);
            this.lblDescBaslangicTol.Name = "lblDescBaslangicTol";
            this.lblDescBaslangicTol.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblDescBaslangicTol.Size = new System.Drawing.Size(462, 40);
            this.lblDescBaslangicTol.TabIndex = 11;
            this.lblDescBaslangicTol.Text = "* Personelin geç kalmış sayılmayacağı opsiyonel süre.";
            this.lblDescBaslangicTol.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.Gray;
            this.label4.Location = new System.Drawing.Point(43, 272);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 12;
            this.label4.Text = "Vardiya Bitiş Tolerans Saati";
            // 
            // dtpVardiyaBitisToleransSaati
            // 
            this.dtpVardiyaBitisToleransSaati.CustomFormat = "HH:mm";
            this.dtpVardiyaBitisToleransSaati.Dock = System.Windows.Forms.DockStyle.Left;
            this.dtpVardiyaBitisToleransSaati.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.dtpVardiyaBitisToleransSaati.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpVardiyaBitisToleransSaati.Location = new System.Drawing.Point(43, 298);
            this.dtpVardiyaBitisToleransSaati.Name = "dtpVardiyaBitisToleransSaati";
            this.dtpVardiyaBitisToleransSaati.ShowUpDown = true;
            this.dtpVardiyaBitisToleransSaati.Size = new System.Drawing.Size(150, 32);
            this.dtpVardiyaBitisToleransSaati.TabIndex = 13;
            // 
            // lblDescBitisTol
            // 
            this.lblDescBitisTol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescBitisTol.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblDescBitisTol.ForeColor = System.Drawing.Color.DarkGray;
            this.lblDescBitisTol.Location = new System.Drawing.Point(355, 295);
            this.lblDescBitisTol.Name = "lblDescBitisTol";
            this.lblDescBitisTol.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblDescBitisTol.Size = new System.Drawing.Size(462, 40);
            this.lblDescBitisTol.TabIndex = 14;
            this.lblDescBitisTol.Text = "* Personelin erken çıkmış sayılmayacağı opsiyonel süre.";
            this.lblDescBitisTol.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.Gray;
            this.label5.Location = new System.Drawing.Point(43, 337);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(171, 23);
            this.label5.TabIndex = 15;
            this.label5.Text = "Yemek Aktifleme Saati";
            // 
            // dtpYemekAktiflemeSaati
            // 
            this.dtpYemekAktiflemeSaati.CustomFormat = "HH:mm";
            this.dtpYemekAktiflemeSaati.Dock = System.Windows.Forms.DockStyle.Left;
            this.dtpYemekAktiflemeSaati.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.dtpYemekAktiflemeSaati.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpYemekAktiflemeSaati.Location = new System.Drawing.Point(43, 363);
            this.dtpYemekAktiflemeSaati.Name = "dtpYemekAktiflemeSaati";
            this.dtpYemekAktiflemeSaati.ShowUpDown = true;
            this.dtpYemekAktiflemeSaati.Size = new System.Drawing.Size(150, 32);
            this.dtpYemekAktiflemeSaati.TabIndex = 16;
            // 
            // lblDescYemek
            // 
            this.lblDescYemek.AutoSize = true;
            this.lblDescYemek.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescYemek.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblDescYemek.ForeColor = System.Drawing.Color.DarkGray;
            this.lblDescYemek.Location = new System.Drawing.Point(355, 360);
            this.lblDescYemek.Name = "lblDescYemek";
            this.lblDescYemek.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblDescYemek.Size = new System.Drawing.Size(462, 107);
            this.lblDescYemek.TabIndex = 17;
            this.lblDescYemek.Text = "* Yemekhane turnikelerinde kişinin tekrardan yemek yiyebileceği başlangıç saati.";
            this.lblDescYemek.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlFooter.Controls.Add(this.btnVazgec);
            this.pnlFooter.Controls.Add(this.btnKaydet);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 680);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(20);
            this.pnlFooter.Size = new System.Drawing.Size(860, 100);
            this.pnlFooter.TabIndex = 3;
            // 
            // btnVazgec
            // 
            this.btnVazgec.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnVazgec.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnVazgec.FlatAppearance.BorderSize = 0;
            this.btnVazgec.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVazgec.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnVazgec.ForeColor = System.Drawing.Color.White;
            this.btnVazgec.Location = new System.Drawing.Point(20, 20);
            this.btnVazgec.Name = "btnVazgec";
            this.btnVazgec.Size = new System.Drawing.Size(130, 60);
            this.btnVazgec.TabIndex = 0;
            this.btnVazgec.Text = "Vazgeç";
            this.btnVazgec.UseVisualStyleBackColor = false;
            // 
            // btnKaydet
            // 
            this.btnKaydet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnKaydet.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnKaydet.FlatAppearance.BorderSize = 0;
            this.btnKaydet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKaydet.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnKaydet.ForeColor = System.Drawing.Color.White;
            this.btnKaydet.Location = new System.Drawing.Point(710, 20);
            this.btnKaydet.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(130, 60);
            this.btnKaydet.TabIndex = 1;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = false;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlToolbar.Controls.Add(this.btnVardiyaGuncelle);
            this.pnlToolbar.Controls.Add(this.btnVardiyaSil);
            this.pnlToolbar.Controls.Add(this.btnVardiyaEkle);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 75);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(10);
            this.pnlToolbar.Size = new System.Drawing.Size(860, 138);
            this.pnlToolbar.TabIndex = 1;
            // 
            // btnVardiyaGuncelle
            // 
            this.btnVardiyaGuncelle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.btnVardiyaGuncelle.FlatAppearance.BorderSize = 0;
            this.btnVardiyaGuncelle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVardiyaGuncelle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVardiyaGuncelle.ForeColor = System.Drawing.Color.White;
            this.btnVardiyaGuncelle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_update_50;
            this.btnVardiyaGuncelle.Location = new System.Drawing.Point(616, 10);
            this.btnVardiyaGuncelle.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.btnVardiyaGuncelle.Name = "btnVardiyaGuncelle";
            this.btnVardiyaGuncelle.Padding = new System.Windows.Forms.Padding(5, 0, 15, 0);
            this.btnVardiyaGuncelle.Size = new System.Drawing.Size(219, 96);
            this.btnVardiyaGuncelle.TabIndex = 0;
            this.btnVardiyaGuncelle.Text = "Güncelle";
            this.btnVardiyaGuncelle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnVardiyaGuncelle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnVardiyaGuncelle.UseVisualStyleBackColor = false;
            // 
            // btnVardiyaSil
            // 
            this.btnVardiyaSil.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnVardiyaSil.FlatAppearance.BorderSize = 0;
            this.btnVardiyaSil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVardiyaSil.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVardiyaSil.ForeColor = System.Drawing.Color.White;
            this.btnVardiyaSil.Image = global::CeyPASS.WFA.Properties.Resources.icons8_minus_50;
            this.btnVardiyaSil.Location = new System.Drawing.Point(445, 10);
            this.btnVardiyaSil.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.btnVardiyaSil.Name = "btnVardiyaSil";
            this.btnVardiyaSil.Padding = new System.Windows.Forms.Padding(5, 0, 20, 0);
            this.btnVardiyaSil.Size = new System.Drawing.Size(161, 96);
            this.btnVardiyaSil.TabIndex = 1;
            this.btnVardiyaSil.Text = "Sil";
            this.btnVardiyaSil.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnVardiyaSil.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnVardiyaSil.UseVisualStyleBackColor = false;
            // 
            // btnVardiyaEkle
            // 
            this.btnVardiyaEkle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnVardiyaEkle.FlatAppearance.BorderSize = 0;
            this.btnVardiyaEkle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVardiyaEkle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVardiyaEkle.ForeColor = System.Drawing.Color.White;
            this.btnVardiyaEkle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_add_50;
            this.btnVardiyaEkle.Location = new System.Drawing.Point(277, 10);
            this.btnVardiyaEkle.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.btnVardiyaEkle.Name = "btnVardiyaEkle";
            this.btnVardiyaEkle.Padding = new System.Windows.Forms.Padding(5, 0, 20, 0);
            this.btnVardiyaEkle.Size = new System.Drawing.Size(158, 96);
            this.btnVardiyaEkle.TabIndex = 2;
            this.btnVardiyaEkle.Text = "Ekle";
            this.btnVardiyaEkle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnVardiyaEkle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnVardiyaEkle.UseVisualStyleBackColor = false;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblHeader.Size = new System.Drawing.Size(860, 75);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Vardiya Bilgileri";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Controls.Add(this.chkVardiyalar);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(10, 10);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(1);
            this.pnlLeft.Size = new System.Drawing.Size(300, 780);
            this.pnlLeft.TabIndex = 2;
            // 
            // chkVardiyalar
            // 
            this.chkVardiyalar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkVardiyalar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkVardiyalar.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.chkVardiyalar.FormattingEnabled = true;
            this.chkVardiyalar.HorizontalScrollbar = true;
            this.chkVardiyalar.Location = new System.Drawing.Point(1, 51);
            this.chkVardiyalar.Name = "chkVardiyalar";
            this.chkVardiyalar.Size = new System.Drawing.Size(298, 728);
            this.chkVardiyalar.TabIndex = 1;
            // 
            // pnlLeftHeader
            // 
            this.pnlLeftHeader.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlLeftHeader.Controls.Add(this.lblListHeader);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(1, 1);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(298, 50);
            this.pnlLeftHeader.TabIndex = 0;
            // 
            // lblListHeader
            // 
            this.lblListHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblListHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblListHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblListHeader.Location = new System.Drawing.Point(0, 0);
            this.lblListHeader.Name = "lblListHeader";
            this.lblListHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblListHeader.Size = new System.Drawing.Size(298, 50);
            this.lblListHeader.TabIndex = 0;
            this.lblListHeader.Text = "Vardiya Listesi";
            this.lblListHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ucCalismaSekilleri
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "ucCalismaSekilleri";
            this.Size = new System.Drawing.Size(1200, 800);
            this.pnlMain.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.tlpForm.ResumeLayout(false);
            this.tlpForm.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeftHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlCard;

        private System.Windows.Forms.Panel pnlLeftHeader;
        private System.Windows.Forms.Label lblListHeader;
        private System.Windows.Forms.CheckedListBox chkVardiyalar;

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.FlowLayoutPanel pnlToolbar;
        private System.Windows.Forms.TableLayoutPanel tlpForm;
        private System.Windows.Forms.Panel pnlFooter;

        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtVardiyaAdi;

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpVardiyaBaslangicSaati;

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpVardiyaBitisSaati;

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtpVardiyaBaslangicToleransSaati;

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dtpVardiyaBitisToleransSaati;

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtpYemekAktiflemeSaati;

        // Açıklama Labelları
        private System.Windows.Forms.Label lblDescVardiyaAdi;
        private System.Windows.Forms.Label lblDescBaslangic;
        private System.Windows.Forms.Label lblDescBitis;
        private System.Windows.Forms.Label lblDescBaslangicTol;
        private System.Windows.Forms.Label lblDescBitisTol;
        private System.Windows.Forms.Label lblDescYemek;

        private System.Windows.Forms.Button btnVardiyaEkle;
        private System.Windows.Forms.Button btnVardiyaSil;
        private System.Windows.Forms.Button btnVardiyaGuncelle;

        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Button btnVazgec;
    }
}
