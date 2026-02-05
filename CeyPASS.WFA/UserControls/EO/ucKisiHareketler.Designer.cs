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
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlGridContainer = new System.Windows.Forms.Panel();
            this.dgKisiHareketler = new System.Windows.Forms.DataGridView();
            this.pnlFilters = new System.Windows.Forms.Panel();
            this.tlpFilters = new System.Windows.Forms.TableLayoutPanel();
            this.pnlDateFilters = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpHareketBaslangicTarihi = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpHareketBitisTarihi = new System.Windows.Forms.DateTimePicker();
            this.pnlCheckboxFilters = new System.Windows.Forms.Panel();
            this.chbAktifHareketler = new System.Windows.Forms.CheckBox();
            this.chbPasifHareketler = new System.Windows.Forms.CheckBox();
            this.chbYemekhaneHareketleri = new System.Windows.Forms.CheckBox();
            this.pnlActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnHareketGuncelle = new System.Windows.Forms.Button();
            this.btnHareketSil = new System.Windows.Forms.Button();
            this.btnHareketEkle = new System.Windows.Forms.Button();
            this.btnHareketleriGetir = new System.Windows.Forms.Button();
            this.pnlFirmaFilter = new System.Windows.Forms.Panel();
            this.lblFirma = new System.Windows.Forms.Label();
            this.cmbFirma = new System.Windows.Forms.ComboBox();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.chkKisiler = new System.Windows.Forms.CheckedListBox();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.lblPersonelListesi = new System.Windows.Forms.Label();
            this.pnlMain.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlGridContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgKisiHareketler)).BeginInit();
            this.pnlFilters.SuspendLayout();
            this.tlpFilters.SuspendLayout();
            this.pnlDateFilters.SuspendLayout();
            this.pnlCheckboxFilters.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.pnlFirmaFilter.SuspendLayout();
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
            this.pnlMain.Size = new System.Drawing.Size(1650, 869);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.pnlGridContainer);
            this.pnlContent.Controls.Add(this.pnlFilters);
            this.pnlContent.Controls.Add(this.pnlFirmaFilter);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(360, 10);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.pnlContent.Size = new System.Drawing.Size(1280, 849);
            this.pnlContent.TabIndex = 1;
            // 
            // pnlGridContainer
            // 
            this.pnlGridContainer.BackColor = System.Drawing.Color.White;
            this.pnlGridContainer.Controls.Add(this.dgKisiHareketler);
            this.pnlGridContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridContainer.Location = new System.Drawing.Point(10, 180);
            this.pnlGridContainer.Name = "pnlGridContainer";
            this.pnlGridContainer.Padding = new System.Windows.Forms.Padding(10);
            this.pnlGridContainer.Size = new System.Drawing.Size(1270, 669);
            this.pnlGridContainer.TabIndex = 2;
            // 
            // dgKisiHareketler
            // 
            this.dgKisiHareketler.BackgroundColor = System.Drawing.Color.White;
            this.dgKisiHareketler.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgKisiHareketler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgKisiHareketler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgKisiHareketler.Location = new System.Drawing.Point(10, 10);
            this.dgKisiHareketler.Name = "dgKisiHareketler";
            this.dgKisiHareketler.RowHeadersWidth = 51;
            this.dgKisiHareketler.RowTemplate.Height = 24;
            this.dgKisiHareketler.Size = new System.Drawing.Size(1250, 649);
            this.dgKisiHareketler.TabIndex = 0;
            // 
            // pnlFilters
            // 
            this.pnlFilters.BackColor = System.Drawing.Color.White;
            this.pnlFilters.Controls.Add(this.tlpFilters);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Location = new System.Drawing.Point(10, 50);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(1270, 130); // Yükseklik artırıldı
            this.pnlFilters.TabIndex = 1;
            // 
            // tlpFilters
            // 
            this.tlpFilters.ColumnCount = 3;
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpFilters.Controls.Add(this.pnlDateFilters, 0, 0);
            this.tlpFilters.Controls.Add(this.pnlCheckboxFilters, 1, 0);
            this.tlpFilters.Controls.Add(this.pnlActions, 2, 0);
            this.tlpFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFilters.Location = new System.Drawing.Point(0, 0);
            this.tlpFilters.Name = "tlpFilters";
            this.tlpFilters.RowCount = 1;
            this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFilters.Size = new System.Drawing.Size(1270, 130);
            this.tlpFilters.TabIndex = 0;
            // 
            // pnlDateFilters
            // 
            this.pnlDateFilters.Controls.Add(this.label2);
            this.pnlDateFilters.Controls.Add(this.dtpHareketBaslangicTarihi);
            this.pnlDateFilters.Controls.Add(this.label3);
            this.pnlDateFilters.Controls.Add(this.dtpHareketBitisTarihi);
            this.pnlDateFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDateFilters.Location = new System.Drawing.Point(3, 3);
            this.pnlDateFilters.Name = "pnlDateFilters";
            this.pnlDateFilters.Padding = new System.Windows.Forms.Padding(10);
            this.pnlDateFilters.Size = new System.Drawing.Size(438, 124);
            this.pnlDateFilters.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(10, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "Başlangıç Tarihi";
            // 
            // dtpHareketBaslangicTarihi
            // 
            this.dtpHareketBaslangicTarihi.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpHareketBaslangicTarihi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpHareketBaslangicTarihi.Location = new System.Drawing.Point(10, 33);
            this.dtpHareketBaslangicTarihi.Name = "dtpHareketBaslangicTarihi";
            this.dtpHareketBaslangicTarihi.Size = new System.Drawing.Size(418, 30);
            this.dtpHareketBaslangicTarihi.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.Gray;
            this.label3.Location = new System.Drawing.Point(10, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Bitiş Tarihi";
            // 
            // dtpHareketBitisTarihi
            // 
            this.dtpHareketBitisTarihi.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpHareketBitisTarihi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpHareketBitisTarihi.Location = new System.Drawing.Point(10, 89);
            this.dtpHareketBitisTarihi.Name = "dtpHareketBitisTarihi";
            this.dtpHareketBitisTarihi.Size = new System.Drawing.Size(418, 30);
            this.dtpHareketBitisTarihi.TabIndex = 3;
            // 
            // pnlCheckboxFilters
            // 
            this.pnlCheckboxFilters.Controls.Add(this.chbAktifHareketler);
            this.pnlCheckboxFilters.Controls.Add(this.chbPasifHareketler);
            this.pnlCheckboxFilters.Controls.Add(this.chbYemekhaneHareketleri);
            this.pnlCheckboxFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCheckboxFilters.Location = new System.Drawing.Point(447, 3);
            this.pnlCheckboxFilters.Name = "pnlCheckboxFilters";
            this.pnlCheckboxFilters.Padding = new System.Windows.Forms.Padding(10);
            this.pnlCheckboxFilters.Size = new System.Drawing.Size(311, 124);
            this.pnlCheckboxFilters.TabIndex = 1;
            // 
            // chbAktifHareketler
            // 
            this.chbAktifHareketler.AutoSize = true;
            this.chbAktifHareketler.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chbAktifHareketler.Location = new System.Drawing.Point(10, 20);
            this.chbAktifHareketler.Name = "chbAktifHareketler";
            this.chbAktifHareketler.Size = new System.Drawing.Size(148, 27);
            this.chbAktifHareketler.TabIndex = 0;
            this.chbAktifHareketler.Text = "Aktif Hareketler";
            this.chbAktifHareketler.UseVisualStyleBackColor = true;
            // 
            // chbPasifHareketler
            // 
            this.chbPasifHareketler.AutoSize = true;
            this.chbPasifHareketler.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chbPasifHareketler.Location = new System.Drawing.Point(10, 50);
            this.chbPasifHareketler.Name = "chbPasifHareketler";
            this.chbPasifHareketler.Size = new System.Drawing.Size(148, 27);
            this.chbPasifHareketler.TabIndex = 1;
            this.chbPasifHareketler.Text = "Pasif Hareketler";
            this.chbPasifHareketler.UseVisualStyleBackColor = true;
            // 
            // chbYemekhaneHareketleri
            // 
            this.chbYemekhaneHareketleri.AutoSize = true;
            this.chbYemekhaneHareketleri.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chbYemekhaneHareketleri.Location = new System.Drawing.Point(10, 80);
            this.chbYemekhaneHareketleri.Name = "chbYemekhaneHareketleri";
            this.chbYemekhaneHareketleri.Size = new System.Drawing.Size(213, 27);
            this.chbYemekhaneHareketleri.TabIndex = 2;
            this.chbYemekhaneHareketleri.Text = "Yemekhane Hareketleri";
            this.chbYemekhaneHareketleri.UseVisualStyleBackColor = true;
            // 
            // pnlActions
            // 
            this.pnlActions.Controls.Add(this.btnHareketleriGetir);
            this.pnlActions.Controls.Add(this.btnHareketEkle);
            this.pnlActions.Controls.Add(this.btnHareketSil);
            this.pnlActions.Controls.Add(this.btnHareketGuncelle);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActions.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlActions.Location = new System.Drawing.Point(764, 3);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(10);
            this.pnlActions.Size = new System.Drawing.Size(503, 124);
            this.pnlActions.TabIndex = 2;
            this.pnlActions.WrapContents = false;
            // 
            // btnHareketGuncelle
            // 
            this.btnHareketGuncelle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.btnHareketGuncelle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHareketGuncelle.FlatAppearance.BorderSize = 0;
            this.btnHareketGuncelle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHareketGuncelle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnHareketGuncelle.ForeColor = System.Drawing.Color.White;
            this.btnHareketGuncelle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_update_50;
            this.btnHareketGuncelle.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnHareketGuncelle.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnHareketGuncelle.Name = "btnHareketGuncelle";
            this.btnHareketGuncelle.Size = new System.Drawing.Size(90, 90);
            this.btnHareketGuncelle.TabIndex = 3;
            this.btnHareketGuncelle.Text = "Güncelle";
            this.btnHareketGuncelle.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnHareketGuncelle.UseVisualStyleBackColor = false;
            // 
            // btnHareketSil
            // 
            this.btnHareketSil.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnHareketSil.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHareketSil.FlatAppearance.BorderSize = 0;
            this.btnHareketSil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHareketSil.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnHareketSil.ForeColor = System.Drawing.Color.White;
            this.btnHareketSil.Image = global::CeyPASS.WFA.Properties.Resources.icons8_minus_50;
            this.btnHareketSil.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnHareketSil.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnHareketSil.Name = "btnHareketSil";
            this.btnHareketSil.Size = new System.Drawing.Size(90, 90);
            this.btnHareketSil.TabIndex = 2;
            this.btnHareketSil.Text = "Sil";
            this.btnHareketSil.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnHareketSil.UseVisualStyleBackColor = false;
            // 
            // btnHareketEkle
            // 
            this.btnHareketEkle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnHareketEkle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHareketEkle.FlatAppearance.BorderSize = 0;
            this.btnHareketEkle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHareketEkle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnHareketEkle.ForeColor = System.Drawing.Color.White;
            this.btnHareketEkle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_add_50;
            this.btnHareketEkle.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnHareketEkle.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnHareketEkle.Name = "btnHareketEkle";
            this.btnHareketEkle.Size = new System.Drawing.Size(90, 90);
            this.btnHareketEkle.TabIndex = 1;
            this.btnHareketEkle.Text = "Ekle";
            this.btnHareketEkle.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnHareketEkle.UseVisualStyleBackColor = false;
            // 
            // btnHareketleriGetir
            // 
            this.btnHareketleriGetir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnHareketleriGetir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHareketleriGetir.FlatAppearance.BorderSize = 0;
            this.btnHareketleriGetir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHareketleriGetir.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnHareketleriGetir.ForeColor = System.Drawing.Color.White;
            this.btnHareketleriGetir.Image = global::CeyPASS.WFA.Properties.Resources.icons8_search_50;
            this.btnHareketleriGetir.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnHareketleriGetir.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnHareketleriGetir.Name = "btnHareketleriGetir";
            this.btnHareketleriGetir.Size = new System.Drawing.Size(90, 90);
            this.btnHareketleriGetir.TabIndex = 0;
            this.btnHareketleriGetir.Text = "Listele";
            this.btnHareketleriGetir.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnHareketleriGetir.UseVisualStyleBackColor = false;
            // 
            // pnlFirmaFilter
            // 
            this.pnlFirmaFilter.BackColor = System.Drawing.Color.White;
            this.pnlFirmaFilter.Controls.Add(this.lblFirma);
            this.pnlFirmaFilter.Controls.Add(this.cmbFirma);
            this.pnlFirmaFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFirmaFilter.Location = new System.Drawing.Point(360, 10);
            this.pnlFirmaFilter.Name = "pnlFirmaFilter";
            this.pnlFirmaFilter.Padding = new System.Windows.Forms.Padding(10);
            this.pnlFirmaFilter.Size = new System.Drawing.Size(1280, 40); // Yükseklik azaltıldı
            this.pnlFirmaFilter.TabIndex = 0;
            this.pnlFirmaFilter.Visible = false; // Varsayılan gizli
            // 
            // lblFirma
            // 
            this.lblFirma.AutoSize = true;
            this.lblFirma.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblFirma.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblFirma.Location = new System.Drawing.Point(10, 8);
            this.lblFirma.Name = "lblFirma";
            this.lblFirma.Size = new System.Drawing.Size(58, 23);
            this.lblFirma.TabIndex = 0;
            this.lblFirma.Text = "Firma:";
            // 
            // cmbFirma
            // 
            this.cmbFirma.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFirma.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbFirma.FormattingEnabled = true;
            this.cmbFirma.Location = new System.Drawing.Point(80, 5);
            this.cmbFirma.Name = "cmbFirma";
            this.cmbFirma.Size = new System.Drawing.Size(250, 31);
            this.cmbFirma.TabIndex = 1;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Controls.Add(this.chkKisiler);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(10, 10);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(1);
            this.pnlLeft.Size = new System.Drawing.Size(350, 849);
            this.pnlLeft.TabIndex = 0;
            // 
            // chkKisiler
            // 
            this.chkKisiler.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkKisiler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkKisiler.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkKisiler.FormattingEnabled = true;
            this.chkKisiler.HorizontalScrollbar = true;
            this.chkKisiler.Location = new System.Drawing.Point(1, 51);
            this.chkKisiler.Name = "chkKisiler";
            this.chkKisiler.Size = new System.Drawing.Size(348, 797);
            this.chkKisiler.TabIndex = 1;
            // 
            // pnlLeftHeader
            // 
            this.pnlLeftHeader.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlLeftHeader.Controls.Add(this.lblPersonelListesi);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(1, 1);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(348, 50);
            this.pnlLeftHeader.TabIndex = 0;
            // 
            // lblPersonelListesi
            // 
            this.lblPersonelListesi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPersonelListesi.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblPersonelListesi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblPersonelListesi.Location = new System.Drawing.Point(0, 0);
            this.lblPersonelListesi.Name = "lblPersonelListesi";
            this.lblPersonelListesi.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblPersonelListesi.Size = new System.Drawing.Size(348, 50);
            this.lblPersonelListesi.TabIndex = 0;
            this.lblPersonelListesi.Text = "Personel Listesi";
            this.lblPersonelListesi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ucKisiHareketler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "ucKisiHareketler";
            this.Size = new System.Drawing.Size(1650, 869);
            this.Load += new System.EventHandler(this.ucKisiHareketler_Load);
            this.pnlMain.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlGridContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgKisiHareketler)).EndInit();
            this.pnlFilters.ResumeLayout(false);
            this.tlpFilters.ResumeLayout(false);
            this.pnlDateFilters.ResumeLayout(false);
            this.pnlDateFilters.PerformLayout();
            this.pnlCheckboxFilters.ResumeLayout(false);
            this.pnlCheckboxFilters.PerformLayout();
            this.pnlActions.ResumeLayout(false);
            this.pnlFirmaFilter.ResumeLayout(false);
            this.pnlFirmaFilter.PerformLayout();
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeftHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        // Ana Konteynerlar
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlContent;

        // Sol Menü
        private System.Windows.Forms.Panel pnlLeftHeader;
        private System.Windows.Forms.Label lblPersonelListesi;
        private System.Windows.Forms.CheckedListBox chkKisiler;

        // Filtreler
        private System.Windows.Forms.Panel pnlFirmaFilter;
        private System.Windows.Forms.Label lblFirma;
        private System.Windows.Forms.ComboBox cmbFirma;

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




    
