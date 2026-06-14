namespace CeyPASS.WFA.UserControls.EO
{
    partial class ucKisiHareketler
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
            pnlMain = new Panel();
            pnlContent = new Panel();
            pnlGridContainer = new Panel();
            dgKisiHareketler = new DataGridView();
            pnlFilters = new Panel();
            tlpFilters = new TableLayoutPanel();
            pnlDateFilters = new Panel();
            label2 = new Label();
            dtpHareketBaslangicTarihi = new DateTimePicker();
            label3 = new Label();
            dtpHareketBitisTarihi = new DateTimePicker();
            pnlCheckboxFilters = new Panel();
            chbAktifHareketler = new CheckBox();
            chbPasifHareketler = new CheckBox();
            chbYemekhaneHareketleri = new CheckBox();
            pnlActions = new FlowLayoutPanel();
            btnHareketleriGetir = new Button();
            btnHareketEkle = new Button();
            btnHareketSil = new Button();
            btnHareketGuncelle = new Button();
            pnlFirmaFilter = new Panel();
            tlpFirmaFilter = new TableLayoutPanel();
            lblFirma = new Label();
            cmbFirma = new ComboBox();
            lblIsyeri = new Label();
            cmbIsyeriFilter = new ComboBox();
            pnlLeft = new Panel();
            chkKisiler = new CheckedListBox();
            pnlLeftHeader = new Panel();
            lblPersonelListesi = new Label();
            pnlKartTipi = new Panel();
            cmbKartTipi = new ComboBox();
            lblKartTipi = new Label();
            pnlMain.SuspendLayout();
            pnlContent.SuspendLayout();
            pnlGridContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgKisiHareketler).BeginInit();
            pnlFilters.SuspendLayout();
            tlpFilters.SuspendLayout();
            pnlDateFilters.SuspendLayout();
            pnlCheckboxFilters.SuspendLayout();
            pnlActions.SuspendLayout();
            pnlFirmaFilter.SuspendLayout();
            tlpFirmaFilter.SuspendLayout();
            pnlLeft.SuspendLayout();
            pnlLeftHeader.SuspendLayout();
            pnlKartTipi.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.BackColor = Color.FromArgb(240, 242, 245);
            pnlMain.Controls.Add(pnlContent);
            pnlMain.Controls.Add(pnlLeft);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Margin = new Padding(3, 4, 3, 4);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(10, 12, 10, 12);
            pnlMain.Size = new Size(1650, 1086);
            pnlMain.TabIndex = 0;
            // 
            // pnlContent
            // 
            pnlContent.Controls.Add(pnlGridContainer);
            pnlContent.Controls.Add(pnlFilters);
            pnlContent.Controls.Add(pnlFirmaFilter);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(360, 12);
            pnlContent.Margin = new Padding(3, 4, 3, 4);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(10, 0, 0, 0);
            pnlContent.Size = new Size(1280, 1062);
            pnlContent.TabIndex = 1;
            // 
            // pnlGridContainer
            // 
            pnlGridContainer.BackColor = Color.White;
            pnlGridContainer.Controls.Add(dgKisiHareketler);
            pnlGridContainer.Dock = DockStyle.Fill;
            pnlGridContainer.Location = new Point(10, 217);
            pnlGridContainer.Margin = new Padding(3, 4, 3, 4);
            pnlGridContainer.Name = "pnlGridContainer";
            pnlGridContainer.Padding = new Padding(10, 12, 10, 12);
            pnlGridContainer.Size = new Size(1270, 845);
            pnlGridContainer.TabIndex = 2;
            // 
            // dgKisiHareketler
            // 
            dgKisiHareketler.BackgroundColor = Color.White;
            dgKisiHareketler.BorderStyle = BorderStyle.None;
            dgKisiHareketler.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgKisiHareketler.Dock = DockStyle.Fill;
            dgKisiHareketler.Location = new Point(10, 12);
            dgKisiHareketler.Margin = new Padding(3, 4, 3, 4);
            dgKisiHareketler.Name = "dgKisiHareketler";
            dgKisiHareketler.RowHeadersWidth = 51;
            dgKisiHareketler.RowTemplate.Height = 24;
            dgKisiHareketler.Size = new Size(1250, 821);
            dgKisiHareketler.TabIndex = 0;
            // 
            // pnlFilters
            // 
            pnlFilters.BackColor = Color.White;
            pnlFilters.Controls.Add(tlpFilters);
            pnlFilters.Dock = DockStyle.Top;
            pnlFilters.Location = new Point(10, 55);
            pnlFilters.Margin = new Padding(3, 4, 3, 4);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Size = new Size(1270, 162);
            pnlFilters.TabIndex = 1;
            // 
            // tlpFilters
            // 
            tlpFilters.ColumnCount = 3;
            tlpFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tlpFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpFilters.Controls.Add(pnlDateFilters, 0, 0);
            tlpFilters.Controls.Add(pnlCheckboxFilters, 1, 0);
            tlpFilters.Controls.Add(pnlActions, 2, 0);
            tlpFilters.Dock = DockStyle.Fill;
            tlpFilters.Location = new Point(0, 0);
            tlpFilters.Margin = new Padding(3, 4, 3, 4);
            tlpFilters.Name = "tlpFilters";
            tlpFilters.RowCount = 1;
            tlpFilters.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpFilters.Size = new Size(1270, 162);
            tlpFilters.TabIndex = 0;
            // 
            // pnlDateFilters
            // 
            pnlDateFilters.Controls.Add(label2);
            pnlDateFilters.Controls.Add(dtpHareketBaslangicTarihi);
            pnlDateFilters.Controls.Add(label3);
            pnlDateFilters.Controls.Add(dtpHareketBitisTarihi);
            pnlDateFilters.Dock = DockStyle.Fill;
            pnlDateFilters.Location = new Point(3, 4);
            pnlDateFilters.Margin = new Padding(3, 4, 3, 4);
            pnlDateFilters.Name = "pnlDateFilters";
            pnlDateFilters.Padding = new Padding(10, 12, 10, 12);
            pnlDateFilters.Size = new Size(438, 154);
            pnlDateFilters.TabIndex = 0;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.ForeColor = Color.Gray;
            label2.Location = new Point(10, 12);
            label2.Name = "label2";
            label2.Size = new Size(118, 20);
            label2.TabIndex = 0;
            label2.Text = "Başlangıç Tarihi";
            // 
            // dtpHareketBaslangicTarihi
            // 
            dtpHareketBaslangicTarihi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dtpHareketBaslangicTarihi.Font = new Font("Segoe UI", 10F);
            dtpHareketBaslangicTarihi.Location = new Point(10, 41);
            dtpHareketBaslangicTarihi.Margin = new Padding(3, 4, 3, 4);
            dtpHareketBaslangicTarihi.Name = "dtpHareketBaslangicTarihi";
            dtpHareketBaslangicTarihi.Size = new Size(418, 30);
            dtpHareketBaslangicTarihi.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.ForeColor = Color.Gray;
            label3.Location = new Point(10, 82);
            label3.Name = "label3";
            label3.Size = new Size(83, 20);
            label3.TabIndex = 2;
            label3.Text = "Bitiş Tarihi";
            // 
            // dtpHareketBitisTarihi
            // 
            dtpHareketBitisTarihi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dtpHareketBitisTarihi.Font = new Font("Segoe UI", 10F);
            dtpHareketBitisTarihi.Location = new Point(10, 111);
            dtpHareketBitisTarihi.Margin = new Padding(3, 4, 3, 4);
            dtpHareketBitisTarihi.Name = "dtpHareketBitisTarihi";
            dtpHareketBitisTarihi.Size = new Size(418, 30);
            dtpHareketBitisTarihi.TabIndex = 3;
            // 
            // pnlCheckboxFilters
            // 
            pnlCheckboxFilters.Controls.Add(chbAktifHareketler);
            pnlCheckboxFilters.Controls.Add(chbPasifHareketler);
            pnlCheckboxFilters.Controls.Add(chbYemekhaneHareketleri);
            pnlCheckboxFilters.Dock = DockStyle.Fill;
            pnlCheckboxFilters.Location = new Point(447, 4);
            pnlCheckboxFilters.Margin = new Padding(3, 4, 3, 4);
            pnlCheckboxFilters.Name = "pnlCheckboxFilters";
            pnlCheckboxFilters.Padding = new Padding(10, 12, 10, 12);
            pnlCheckboxFilters.Size = new Size(311, 154);
            pnlCheckboxFilters.TabIndex = 1;
            // 
            // chbAktifHareketler
            // 
            chbAktifHareketler.AutoSize = true;
            chbAktifHareketler.Font = new Font("Segoe UI", 10F);
            chbAktifHareketler.Location = new Point(10, 25);
            chbAktifHareketler.Margin = new Padding(3, 4, 3, 4);
            chbAktifHareketler.Name = "chbAktifHareketler";
            chbAktifHareketler.Size = new Size(149, 27);
            chbAktifHareketler.TabIndex = 0;
            chbAktifHareketler.Text = "Aktif Hareketler";
            chbAktifHareketler.UseVisualStyleBackColor = true;
            // 
            // chbPasifHareketler
            // 
            chbPasifHareketler.AutoSize = true;
            chbPasifHareketler.Font = new Font("Segoe UI", 10F);
            chbPasifHareketler.Location = new Point(10, 62);
            chbPasifHareketler.Margin = new Padding(3, 4, 3, 4);
            chbPasifHareketler.Name = "chbPasifHareketler";
            chbPasifHareketler.Size = new Size(149, 27);
            chbPasifHareketler.TabIndex = 1;
            chbPasifHareketler.Text = "Pasif Hareketler";
            chbPasifHareketler.UseVisualStyleBackColor = true;
            // 
            // chbYemekhaneHareketleri
            // 
            chbYemekhaneHareketleri.AutoSize = true;
            chbYemekhaneHareketleri.Font = new Font("Segoe UI", 10F);
            chbYemekhaneHareketleri.Location = new Point(10, 100);
            chbYemekhaneHareketleri.Margin = new Padding(3, 4, 3, 4);
            chbYemekhaneHareketleri.Name = "chbYemekhaneHareketleri";
            chbYemekhaneHareketleri.Size = new Size(206, 27);
            chbYemekhaneHareketleri.TabIndex = 2;
            chbYemekhaneHareketleri.Text = "Yemekhane Hareketleri";
            chbYemekhaneHareketleri.UseVisualStyleBackColor = true;
            // 
            // pnlActions
            // 
            pnlActions.Controls.Add(btnHareketleriGetir);
            pnlActions.Controls.Add(btnHareketEkle);
            pnlActions.Controls.Add(btnHareketSil);
            pnlActions.Controls.Add(btnHareketGuncelle);
            pnlActions.Dock = DockStyle.Fill;
            pnlActions.Location = new Point(764, 4);
            pnlActions.Margin = new Padding(3, 4, 3, 4);
            pnlActions.Name = "pnlActions";
            pnlActions.Padding = new Padding(10, 12, 10, 12);
            pnlActions.Size = new Size(503, 154);
            pnlActions.TabIndex = 2;
            pnlActions.WrapContents = false;
            // 
            // btnHareketleriGetir
            // 
            btnHareketleriGetir.BackColor = Color.FromArgb(0, 123, 255);
            btnHareketleriGetir.Cursor = Cursors.Hand;
            btnHareketleriGetir.FlatAppearance.BorderSize = 0;
            btnHareketleriGetir.FlatStyle = FlatStyle.Flat;
            btnHareketleriGetir.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnHareketleriGetir.ForeColor = Color.White;
            btnHareketleriGetir.Image = Properties.Resources.icons8_search_50;
            btnHareketleriGetir.ImageAlign = ContentAlignment.TopCenter;
            btnHareketleriGetir.Location = new Point(13, 16);
            btnHareketleriGetir.Margin = new Padding(3, 4, 10, 4);
            btnHareketleriGetir.Name = "btnHareketleriGetir";
            btnHareketleriGetir.Size = new Size(90, 112);
            btnHareketleriGetir.TabIndex = 0;
            btnHareketleriGetir.Text = "Listele";
            btnHareketleriGetir.TextAlign = ContentAlignment.BottomCenter;
            btnHareketleriGetir.UseVisualStyleBackColor = false;
            // 
            // btnHareketEkle
            // 
            btnHareketEkle.BackColor = Color.FromArgb(40, 167, 69);
            btnHareketEkle.Cursor = Cursors.Hand;
            btnHareketEkle.FlatAppearance.BorderSize = 0;
            btnHareketEkle.FlatStyle = FlatStyle.Flat;
            btnHareketEkle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnHareketEkle.ForeColor = Color.White;
            btnHareketEkle.Image = Properties.Resources.icons8_add_50;
            btnHareketEkle.ImageAlign = ContentAlignment.TopCenter;
            btnHareketEkle.Location = new Point(116, 16);
            btnHareketEkle.Margin = new Padding(3, 4, 10, 4);
            btnHareketEkle.Name = "btnHareketEkle";
            btnHareketEkle.Size = new Size(90, 112);
            btnHareketEkle.TabIndex = 1;
            btnHareketEkle.Text = "Ekle";
            btnHareketEkle.TextAlign = ContentAlignment.BottomCenter;
            btnHareketEkle.UseVisualStyleBackColor = false;
            // 
            // btnHareketSil
            // 
            btnHareketSil.BackColor = Color.FromArgb(220, 53, 69);
            btnHareketSil.Cursor = Cursors.Hand;
            btnHareketSil.FlatAppearance.BorderSize = 0;
            btnHareketSil.FlatStyle = FlatStyle.Flat;
            btnHareketSil.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnHareketSil.ForeColor = Color.White;
            btnHareketSil.Image = Properties.Resources.icons8_minus_50;
            btnHareketSil.ImageAlign = ContentAlignment.TopCenter;
            btnHareketSil.Location = new Point(219, 16);
            btnHareketSil.Margin = new Padding(3, 4, 10, 4);
            btnHareketSil.Name = "btnHareketSil";
            btnHareketSil.Size = new Size(90, 112);
            btnHareketSil.TabIndex = 2;
            btnHareketSil.Text = "Sil";
            btnHareketSil.TextAlign = ContentAlignment.BottomCenter;
            btnHareketSil.UseVisualStyleBackColor = false;
            // 
            // btnHareketGuncelle
            // 
            btnHareketGuncelle.BackColor = Color.FromArgb(23, 162, 184);
            btnHareketGuncelle.Cursor = Cursors.Hand;
            btnHareketGuncelle.FlatAppearance.BorderSize = 0;
            btnHareketGuncelle.FlatStyle = FlatStyle.Flat;
            btnHareketGuncelle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnHareketGuncelle.ForeColor = Color.White;
            btnHareketGuncelle.Image = Properties.Resources.icons8_update_50;
            btnHareketGuncelle.ImageAlign = ContentAlignment.TopCenter;
            btnHareketGuncelle.Location = new Point(322, 16);
            btnHareketGuncelle.Margin = new Padding(3, 4, 10, 4);
            btnHareketGuncelle.Name = "btnHareketGuncelle";
            btnHareketGuncelle.Size = new Size(90, 112);
            btnHareketGuncelle.TabIndex = 3;
            btnHareketGuncelle.Text = "Güncelle";
            btnHareketGuncelle.TextAlign = ContentAlignment.BottomCenter;
            btnHareketGuncelle.UseVisualStyleBackColor = false;
            // 
            // pnlFirmaFilter
            // 
            pnlFirmaFilter.BackColor = Color.White;
            pnlFirmaFilter.Controls.Add(tlpFirmaFilter);
            pnlFirmaFilter.Dock = DockStyle.Top;
            pnlFirmaFilter.Location = new Point(10, 0);
            pnlFirmaFilter.Margin = new Padding(3, 4, 3, 4);
            pnlFirmaFilter.Name = "pnlFirmaFilter";
            pnlFirmaFilter.Padding = new Padding(10, 8, 10, 8);
            pnlFirmaFilter.Size = new Size(1270, 55);
            pnlFirmaFilter.TabIndex = 0;
            pnlFirmaFilter.Visible = true;
            // 
            // tlpFirmaFilter
            // 
            tlpFirmaFilter.AutoSize = true;
            tlpFirmaFilter.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpFirmaFilter.ColumnCount = 4;
            tlpFirmaFilter.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpFirmaFilter.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpFirmaFilter.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpFirmaFilter.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpFirmaFilter.Controls.Add(lblFirma, 0, 0);
            tlpFirmaFilter.Controls.Add(cmbFirma, 1, 0);
            tlpFirmaFilter.Controls.Add(lblIsyeri, 2, 0);
            tlpFirmaFilter.Controls.Add(cmbIsyeriFilter, 3, 0);
            tlpFirmaFilter.Dock = DockStyle.Left;
            tlpFirmaFilter.Location = new Point(10, 8);
            tlpFirmaFilter.Margin = new Padding(3, 4, 3, 4);
            tlpFirmaFilter.Name = "tlpFirmaFilter";
            tlpFirmaFilter.RowCount = 1;
            tlpFirmaFilter.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpFirmaFilter.TabIndex = 0;
            // 
            // lblFirma
            // 
            lblFirma.Anchor = AnchorStyles.Left;
            lblFirma.AutoSize = true;
            lblFirma.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblFirma.ForeColor = Color.FromArgb(64, 64, 64);
            lblFirma.Location = new Point(3, 10);
            lblFirma.Margin = new Padding(3, 5, 12, 0);
            lblFirma.Name = "lblFirma";
            lblFirma.Size = new Size(142, 23);
            lblFirma.TabIndex = 0;
            lblFirma.Text = "Personel firması:";
            // 
            // cmbFirma
            // 
            cmbFirma.Anchor = AnchorStyles.Left;
            cmbFirma.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFirma.Font = new Font("Segoe UI", 10F);
            cmbFirma.FormattingEnabled = true;
            cmbFirma.Location = new Point(157, 4);
            cmbFirma.Margin = new Padding(0, 4, 0, 0);
            cmbFirma.Name = "cmbFirma";
            cmbFirma.Size = new Size(220, 31);
            cmbFirma.TabIndex = 1;
            // 
            // lblIsyeri
            // 
            lblIsyeri.Anchor = AnchorStyles.Left;
            lblIsyeri.AutoSize = true;
            lblIsyeri.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblIsyeri.ForeColor = Color.FromArgb(64, 64, 64);
            lblIsyeri.Location = new Point(389, 10);
            lblIsyeri.Margin = new Padding(12, 5, 12, 0);
            lblIsyeri.Name = "lblIsyeri";
            lblIsyeri.Size = new Size(65, 23);
            lblIsyeri.TabIndex = 2;
            lblIsyeri.Text = "İşyeri:";
            // 
            // cmbIsyeriFilter
            // 
            cmbIsyeriFilter.Anchor = AnchorStyles.Left;
            cmbIsyeriFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbIsyeriFilter.Font = new Font("Segoe UI", 10F);
            cmbIsyeriFilter.FormattingEnabled = true;
            cmbIsyeriFilter.Location = new Point(466, 4);
            cmbIsyeriFilter.Margin = new Padding(0, 4, 0, 0);
            cmbIsyeriFilter.Name = "cmbIsyeriFilter";
            cmbIsyeriFilter.Size = new Size(220, 31);
            cmbIsyeriFilter.TabIndex = 3;
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.White;
            pnlLeft.Controls.Add(chkKisiler);
            pnlLeft.Controls.Add(pnlLeftHeader);
            pnlLeft.Controls.Add(pnlKartTipi);
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(10, 12);
            pnlLeft.Margin = new Padding(3, 4, 3, 4);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Padding = new Padding(1);
            pnlLeft.Size = new Size(350, 1062);
            pnlLeft.TabIndex = 0;
            // 
            // chkKisiler
            // 
            chkKisiler.BorderStyle = BorderStyle.None;
            chkKisiler.Dock = DockStyle.Fill;
            chkKisiler.Font = new Font("Segoe UI", 10F);
            chkKisiler.FormattingEnabled = true;
            chkKisiler.HorizontalScrollbar = true;
            chkKisiler.Location = new Point(1, 118);
            chkKisiler.Margin = new Padding(3, 4, 3, 4);
            chkKisiler.Name = "chkKisiler";
            chkKisiler.Size = new Size(348, 943);
            chkKisiler.TabIndex = 1;
            // 
            // pnlLeftHeader
            // 
            pnlLeftHeader.BackColor = Color.WhiteSmoke;
            pnlLeftHeader.Controls.Add(lblPersonelListesi);
            pnlLeftHeader.Dock = DockStyle.Top;
            pnlLeftHeader.Location = new Point(1, 56);
            pnlLeftHeader.Margin = new Padding(3, 4, 3, 4);
            pnlLeftHeader.Name = "pnlLeftHeader";
            pnlLeftHeader.Size = new Size(348, 62);
            pnlLeftHeader.TabIndex = 0;
            // 
            // lblPersonelListesi
            // 
            lblPersonelListesi.Dock = DockStyle.Fill;
            lblPersonelListesi.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblPersonelListesi.ForeColor = Color.FromArgb(64, 64, 64);
            lblPersonelListesi.Location = new Point(0, 0);
            lblPersonelListesi.Name = "lblPersonelListesi";
            lblPersonelListesi.Padding = new Padding(10, 0, 0, 0);
            lblPersonelListesi.Size = new Size(348, 62);
            lblPersonelListesi.TabIndex = 0;
            lblPersonelListesi.Text = "Personel Listesi";
            lblPersonelListesi.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlKartTipi
            // 
            pnlKartTipi.BackColor = Color.WhiteSmoke;
            pnlKartTipi.Controls.Add(cmbKartTipi);
            pnlKartTipi.Controls.Add(lblKartTipi);
            pnlKartTipi.Dock = DockStyle.Top;
            pnlKartTipi.Location = new Point(1, 1);
            pnlKartTipi.Margin = new Padding(3, 4, 3, 4);
            pnlKartTipi.Name = "pnlKartTipi";
            pnlKartTipi.Padding = new Padding(8, 8, 8, 8);
            pnlKartTipi.Size = new Size(348, 55);
            pnlKartTipi.TabIndex = 2;
            // 
            // cmbKartTipi
            // 
            cmbKartTipi.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbKartTipi.Font = new Font("Segoe UI", 9.5F);
            cmbKartTipi.FormattingEnabled = true;
            cmbKartTipi.Location = new Point(90, 10);
            cmbKartTipi.Margin = new Padding(3, 4, 3, 4);
            cmbKartTipi.Name = "cmbKartTipi";
            cmbKartTipi.Size = new Size(250, 29);
            cmbKartTipi.TabIndex = 1;
            // 
            // lblKartTipi
            // 
            lblKartTipi.AutoSize = true;
            lblKartTipi.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblKartTipi.ForeColor = Color.Gray;
            lblKartTipi.Location = new Point(8, 15);
            lblKartTipi.Name = "lblKartTipi";
            lblKartTipi.Size = new Size(73, 20);
            lblKartTipi.TabIndex = 0;
            lblKartTipi.Text = "Kart Tipi:";
            // 
            // ucKisiHareketler
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlMain);
            Margin = new Padding(3, 4, 3, 4);
            Name = "ucKisiHareketler";
            Size = new Size(1650, 1086);
            Load += ucKisiHareketler_Load;
            pnlMain.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            pnlGridContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgKisiHareketler).EndInit();
            pnlFilters.ResumeLayout(false);
            tlpFilters.ResumeLayout(false);
            pnlDateFilters.ResumeLayout(false);
            pnlDateFilters.PerformLayout();
            pnlCheckboxFilters.ResumeLayout(false);
            pnlCheckboxFilters.PerformLayout();
            pnlActions.ResumeLayout(false);
            pnlFirmaFilter.ResumeLayout(false);
            tlpFirmaFilter.ResumeLayout(false);
            tlpFirmaFilter.PerformLayout();
            pnlLeft.ResumeLayout(false);
            pnlLeftHeader.ResumeLayout(false);
            pnlKartTipi.ResumeLayout(false);
            pnlKartTipi.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        // Ana Konteynerlar
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlContent;

        // Sol Menü
        private System.Windows.Forms.Panel pnlKartTipi;
        private System.Windows.Forms.Label lblKartTipi;
        private System.Windows.Forms.ComboBox cmbKartTipi;
        private System.Windows.Forms.Panel pnlLeftHeader;
        private System.Windows.Forms.Label lblPersonelListesi;
        private System.Windows.Forms.CheckedListBox chkKisiler;

        // Filtreler
        private System.Windows.Forms.Panel pnlFirmaFilter;
        private System.Windows.Forms.TableLayoutPanel tlpFirmaFilter;
        private System.Windows.Forms.Label lblFirma;
        private System.Windows.Forms.ComboBox cmbFirma;
        private System.Windows.Forms.Label lblIsyeri;
        private System.Windows.Forms.ComboBox cmbIsyeriFilter;

        private System.Windows.Forms.Panel pnlFilters;
        private System.Windows.Forms.TableLayoutPanel tlpFilters;

        private System.Windows.Forms.Panel pnlDateFilters;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpHareketBaslangicTarihi;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtpHareketBitisTarihi;

        private System.Windows.Forms.Panel pnlCheckboxFilters;
        private System.Windows.Forms.CheckBox chbAktifHareketler;
        private System.Windows.Forms.CheckBox chbPasifHareketler;
        private System.Windows.Forms.CheckBox chbYemekhaneHareketleri;

        private System.Windows.Forms.FlowLayoutPanel pnlActions;
        private System.Windows.Forms.Button btnHareketGuncelle;
        private System.Windows.Forms.Button btnHareketSil;
        private System.Windows.Forms.Button btnHareketEkle;
        private System.Windows.Forms.Button btnHareketleriGetir;

        // Grid
        private System.Windows.Forms.Panel pnlGridContainer;
        private System.Windows.Forms.DataGridView dgKisiHareketler;










    }
}




    
