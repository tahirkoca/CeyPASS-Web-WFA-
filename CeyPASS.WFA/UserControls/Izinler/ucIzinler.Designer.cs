namespace CeyPASS.WFA.UserControls.Izinler
{
    partial class ucIzinler
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

            // Grid
            this.pnlGridContainer = new System.Windows.Forms.Panel();
            this.dgIzinlerTablosu = new System.Windows.Forms.DataGridView();

            // Üst Bar (Filtreler)
            this.pnlTopBar = new System.Windows.Forms.Panel();
            this.tlpFilters = new System.Windows.Forms.TableLayoutPanel();

            this.pnlDateFilters = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpIzinBaslangicTarihi = new System.Windows.Forms.DateTimePicker();
            this.dtpIzinBaslangicSaati = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpIzinBitisTarihi = new System.Windows.Forms.DateTimePicker();
            this.dtpIzinBitisSaati = new System.Windows.Forms.DateTimePicker();

            this.pnlSelectionFilters = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbFirmalarSecimi = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbIzinlerSecimi = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbKisilerSecimi = new System.Windows.Forms.ComboBox();

            // Buton Paneli
            this.pnlActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnIzinGuncelle = new System.Windows.Forms.Button();
            this.btnIzinSil = new System.Windows.Forms.Button();
            this.btnIzinEkle = new System.Windows.Forms.Button();
            this.btnIzinleriGoster = new System.Windows.Forms.Button();

            // Footer
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnVazgec = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.txtAciklama = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.chkSaatlikIzinMi = new System.Windows.Forms.CheckBox();

            this.pnlMain.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlGridContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgIzinlerTablosu)).BeginInit();
            this.pnlTopBar.SuspendLayout();
            this.tlpFilters.SuspendLayout();
            this.pnlDateFilters.SuspendLayout();
            this.pnlSelectionFilters.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(245)))));
            this.pnlMain.Controls.Add(this.pnlContent);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMain.Size = new System.Drawing.Size(1456, 855);
            this.pnlMain.TabIndex = 0;

            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.pnlGridContainer);
            this.pnlContent.Controls.Add(this.pnlTopBar);
            this.pnlContent.Controls.Add(this.pnlFooter);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(10, 10);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(1436, 835);
            this.pnlContent.TabIndex = 1;

            // 
            // pnlTopBar
            // 
            this.pnlTopBar.BackColor = System.Drawing.Color.White;
            this.pnlTopBar.Controls.Add(this.tlpFilters);
            this.pnlTopBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopBar.Location = new System.Drawing.Point(0, 0);
            this.pnlTopBar.Name = "pnlTopBar";
            this.pnlTopBar.Size = new System.Drawing.Size(1436, 150);
            this.pnlTopBar.TabIndex = 0;

            // 
            // tlpFilters
            // 
            this.tlpFilters.ColumnCount = 3;
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F)); // Tarihler
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F)); // Seçimler
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F)); // Butonlar
            this.tlpFilters.Controls.Add(this.pnlDateFilters, 0, 0);
            this.tlpFilters.Controls.Add(this.pnlSelectionFilters, 1, 0);
            this.tlpFilters.Controls.Add(this.pnlActions, 2, 0);
            this.tlpFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFilters.Location = new System.Drawing.Point(0, 0);
            this.tlpFilters.Name = "tlpFilters";
            this.tlpFilters.RowCount = 1;
            this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFilters.Size = new System.Drawing.Size(1436, 150);
            this.tlpFilters.TabIndex = 0;

            // 
            // pnlDateFilters
            // 
            this.pnlDateFilters.Controls.Add(this.label1);
            this.pnlDateFilters.Controls.Add(this.dtpIzinBaslangicTarihi);
            this.pnlDateFilters.Controls.Add(this.dtpIzinBaslangicSaati);
            this.pnlDateFilters.Controls.Add(this.label2);
            this.pnlDateFilters.Controls.Add(this.dtpIzinBitisTarihi);
            this.pnlDateFilters.Controls.Add(this.dtpIzinBitisSaati);
            this.pnlDateFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDateFilters.Padding = new System.Windows.Forms.Padding(10);
            this.pnlDateFilters.TabIndex = 0;

            this.label1.Text = "Başlangıç Tarihi ve Saati";
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(10, 10);
            this.label1.Size = new System.Drawing.Size(200, 20);

            this.dtpIzinBaslangicTarihi.Location = new System.Drawing.Point(10, 33);
            this.dtpIzinBaslangicTarihi.Size = new System.Drawing.Size(200, 30);
            this.dtpIzinBaslangicTarihi.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.dtpIzinBaslangicSaati.Location = new System.Drawing.Point(220, 33);
            this.dtpIzinBaslangicSaati.Size = new System.Drawing.Size(80, 30);
            this.dtpIzinBaslangicSaati.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpIzinBaslangicSaati.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpIzinBaslangicSaati.CustomFormat = "HH:mm";
            this.dtpIzinBaslangicSaati.ShowUpDown = true;

            this.label2.Text = "Bitiş Tarihi ve Saati";
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(10, 75);
            this.label2.Size = new System.Drawing.Size(200, 20);

            this.dtpIzinBitisTarihi.Location = new System.Drawing.Point(10, 98);
            this.dtpIzinBitisTarihi.Size = new System.Drawing.Size(200, 30);
            this.dtpIzinBitisTarihi.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.dtpIzinBitisSaati.Location = new System.Drawing.Point(220, 98);
            this.dtpIzinBitisSaati.Size = new System.Drawing.Size(80, 30);
            this.dtpIzinBitisSaati.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpIzinBitisSaati.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpIzinBitisSaati.CustomFormat = "HH:mm";
            this.dtpIzinBitisSaati.ShowUpDown = true;

            // 
            // pnlSelectionFilters
            // 
            this.pnlSelectionFilters.Controls.Add(this.label7);
            this.pnlSelectionFilters.Controls.Add(this.cmbFirmalarSecimi);
            this.pnlSelectionFilters.Controls.Add(this.label4);
            this.pnlSelectionFilters.Controls.Add(this.cmbIzinlerSecimi);
            this.pnlSelectionFilters.Controls.Add(this.label3);
            this.pnlSelectionFilters.Controls.Add(this.cmbKisilerSecimi);
            this.pnlSelectionFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSelectionFilters.Padding = new System.Windows.Forms.Padding(10);
            this.pnlSelectionFilters.TabIndex = 1;

            this.label7.Text = "Firma";
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label7.Location = new System.Drawing.Point(10, 10);
            this.label7.Size = new System.Drawing.Size(100, 20);

            this.cmbFirmalarSecimi.Location = new System.Drawing.Point(10, 33);
            this.cmbFirmalarSecimi.Size = new System.Drawing.Size(180, 30);
            this.cmbFirmalarSecimi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbFirmalarSecimi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.label4.Text = "İzin Tipi";
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(200, 10);
            this.label4.Size = new System.Drawing.Size(100, 20);

            this.cmbIzinlerSecimi.Location = new System.Drawing.Point(200, 33);
            this.cmbIzinlerSecimi.Size = new System.Drawing.Size(180, 30);
            this.cmbIzinlerSecimi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbIzinlerSecimi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.label3.Text = "Personel";
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(10, 75);
            this.label3.Size = new System.Drawing.Size(100, 20);

            this.cmbKisilerSecimi.Location = new System.Drawing.Point(10, 98);
            this.cmbKisilerSecimi.Size = new System.Drawing.Size(370, 30);
            this.cmbKisilerSecimi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbKisilerSecimi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKisilerSecimi.SelectedIndexChanged += new System.EventHandler(this.cmbKisilerSecimi_SelectedIndexChanged);

            // 
            // pnlActions
            // 
            this.pnlActions.Controls.Add(this.btnIzinGuncelle);
            this.pnlActions.Controls.Add(this.btnIzinSil);
            this.pnlActions.Controls.Add(this.btnIzinEkle);
            this.pnlActions.Controls.Add(this.btnIzinleriGoster);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlActions.Padding = new System.Windows.Forms.Padding(10);
            this.pnlActions.TabIndex = 2;

            // GÜNCELLE BUTONU (Kare, İkon Üstte, Cyan)
            this.btnIzinGuncelle.Text = "Güncelle";
            this.btnIzinGuncelle.Size = new System.Drawing.Size(90, 90);
            this.btnIzinGuncelle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_update_50;
            this.btnIzinGuncelle.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnIzinGuncelle.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnIzinGuncelle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnIzinGuncelle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.btnIzinGuncelle.ForeColor = System.Drawing.Color.White;
            this.btnIzinGuncelle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIzinGuncelle.FlatAppearance.BorderSize = 0;
            this.btnIzinGuncelle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnIzinGuncelle.Margin = new System.Windows.Forms.Padding(3, 15, 10, 3);
            this.btnIzinGuncelle.Click += new System.EventHandler(this.btnIzinGuncelle_Click);

            // SİL BUTONU (Kare, İkon Üstte, Kırmızı)
            this.btnIzinSil.Text = "Sil";
            this.btnIzinSil.Size = new System.Drawing.Size(90, 90);
            this.btnIzinSil.Image = global::CeyPASS.WFA.Properties.Resources.icons8_minus_50;
            this.btnIzinSil.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnIzinSil.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnIzinSil.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnIzinSil.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnIzinSil.ForeColor = System.Drawing.Color.White;
            this.btnIzinSil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIzinSil.FlatAppearance.BorderSize = 0;
            this.btnIzinSil.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnIzinSil.Margin = new System.Windows.Forms.Padding(3, 15, 10, 3);
            this.btnIzinSil.Click += new System.EventHandler(this.btnIzinSil_Click);

            // EKLE BUTONU (Kare, İkon Üstte, Yeşil)
            this.btnIzinEkle.Text = "Ekle";
            this.btnIzinEkle.Size = new System.Drawing.Size(90, 90);
            this.btnIzinEkle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_add_50;
            this.btnIzinEkle.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnIzinEkle.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnIzinEkle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnIzinEkle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnIzinEkle.ForeColor = System.Drawing.Color.White;
            this.btnIzinEkle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIzinEkle.FlatAppearance.BorderSize = 0;
            this.btnIzinEkle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnIzinEkle.Margin = new System.Windows.Forms.Padding(3, 15, 10, 3);
            this.btnIzinEkle.Click += new System.EventHandler(this.btnIzinEkle_Click);

            // LİSTELE BUTONU (Kare, İkon Üstte, Mavi)
            this.btnIzinleriGoster.Text = "Listele";
            this.btnIzinleriGoster.Size = new System.Drawing.Size(90, 90);
            this.btnIzinleriGoster.Image = global::CeyPASS.WFA.Properties.Resources.icons8_search_50;
            this.btnIzinleriGoster.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnIzinleriGoster.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnIzinleriGoster.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnIzinleriGoster.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnIzinleriGoster.ForeColor = System.Drawing.Color.White;
            this.btnIzinleriGoster.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIzinleriGoster.FlatAppearance.BorderSize = 0;
            this.btnIzinleriGoster.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnIzinleriGoster.Margin = new System.Windows.Forms.Padding(3, 15, 10, 3);
            this.btnIzinleriGoster.Click += new System.EventHandler(this.btnIzinleriGoster_Click);

            // 
            // pnlGridContainer
            // 
            this.pnlGridContainer.BackColor = System.Drawing.Color.White;
            this.pnlGridContainer.Controls.Add(this.dgIzinlerTablosu);
            this.pnlGridContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridContainer.Location = new System.Drawing.Point(0, 150);
            this.pnlGridContainer.Name = "pnlGridContainer";
            this.pnlGridContainer.Padding = new System.Windows.Forms.Padding(10);
            this.pnlGridContainer.Size = new System.Drawing.Size(1436, 605);
            this.pnlGridContainer.TabIndex = 2;

            // 
            // dgIzinlerTablosu
            // 
            this.dgIzinlerTablosu.BackgroundColor = System.Drawing.Color.White;
            this.dgIzinlerTablosu.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgIzinlerTablosu.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgIzinlerTablosu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgIzinlerTablosu.Location = new System.Drawing.Point(10, 10);
            this.dgIzinlerTablosu.Name = "dgIzinlerTablosu";
            this.dgIzinlerTablosu.RowHeadersWidth = 51;
            this.dgIzinlerTablosu.RowTemplate.Height = 24;
            this.dgIzinlerTablosu.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgIzinlerTablosu.Size = new System.Drawing.Size(1416, 585);
            this.dgIzinlerTablosu.TabIndex = 0;
            this.dgIzinlerTablosu.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgIzinlerTablosu_DataBindingComplete);
            this.dgIzinlerTablosu.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgIzinlerTablosu_DataError);

            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlFooter.Controls.Add(this.btnVazgec);
            this.pnlFooter.Controls.Add(this.btnKaydet);
            this.pnlFooter.Controls.Add(this.txtAciklama);
            this.pnlFooter.Controls.Add(this.label8);
            this.pnlFooter.Controls.Add(this.chkSaatlikIzinMi);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 755);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(10);
            this.pnlFooter.Size = new System.Drawing.Size(1436, 80);
            this.pnlFooter.TabIndex = 3;

            this.chkSaatlikIzinMi.Text = "Saatlik İzin";
            this.chkSaatlikIzinMi.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.chkSaatlikIzinMi.Location = new System.Drawing.Point(20, 25);
            this.chkSaatlikIzinMi.Size = new System.Drawing.Size(150, 30);
            this.chkSaatlikIzinMi.CheckedChanged += new System.EventHandler(this.chkSaatlikIzinMi_CheckedChanged);

            this.label8.Text = "Açıklama:";
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(180, 10);
            this.label8.Size = new System.Drawing.Size(100, 20);

            this.txtAciklama.Location = new System.Drawing.Point(180, 30);
            this.txtAciklama.Size = new System.Drawing.Size(600, 30);
            this.txtAciklama.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.Size = new System.Drawing.Size(120, 40);
            this.btnKaydet.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnKaydet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnKaydet.ForeColor = System.Drawing.Color.White;
            this.btnKaydet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKaydet.FlatAppearance.BorderSize = 0;
            this.btnKaydet.Location = new System.Drawing.Point(1150, 20);
            this.btnKaydet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);

            this.btnVazgec.Text = "Vazgeç";
            this.btnVazgec.Size = new System.Drawing.Size(120, 40);
            this.btnVazgec.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnVazgec.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(108)))), ((int)(((byte)(117)))), ((int)(((byte)(125)))));
            this.btnVazgec.ForeColor = System.Drawing.Color.White;
            this.btnVazgec.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVazgec.FlatAppearance.BorderSize = 0;
            this.btnVazgec.Location = new System.Drawing.Point(1290, 20);
            this.btnVazgec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnVazgec.Click += new System.EventHandler(this.btnVazgec_Click);

            // 
            // ucIzinler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "ucIzinler";
            this.Size = new System.Drawing.Size(1456, 855);
            this.Load += new System.EventHandler(this.ucIzinler_Load);

            this.pnlMain.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlGridContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgIzinlerTablosu)).EndInit();
            this.pnlTopBar.ResumeLayout(false);
            this.tlpFilters.ResumeLayout(false);
            this.pnlDateFilters.ResumeLayout(false);
            this.pnlSelectionFilters.ResumeLayout(false);
            this.pnlActions.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlContent;

        private System.Windows.Forms.Panel pnlTopBar;
        private System.Windows.Forms.TableLayoutPanel tlpFilters;
        private System.Windows.Forms.Panel pnlDateFilters;
        private System.Windows.Forms.Panel pnlSelectionFilters;
        private System.Windows.Forms.FlowLayoutPanel pnlActions;

        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbFirmalarSecimi;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbKisilerSecimi;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbIzinlerSecimi;

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpIzinBaslangicTarihi;
        private System.Windows.Forms.DateTimePicker dtpIzinBaslangicSaati;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpIzinBitisTarihi;
        private System.Windows.Forms.DateTimePicker dtpIzinBitisSaati;

        private System.Windows.Forms.Button btnIzinleriGoster;
        private System.Windows.Forms.Button btnIzinGuncelle;
        private System.Windows.Forms.Button btnIzinSil;
        private System.Windows.Forms.Button btnIzinEkle;

        private System.Windows.Forms.Panel pnlGridContainer;
        private System.Windows.Forms.DataGridView dgIzinlerTablosu;

        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.CheckBox chkSaatlikIzinMi;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtAciklama;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Button btnVazgec;
    }
}
