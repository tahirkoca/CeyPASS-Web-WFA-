#pragma warning disable 0169 // Designer alanları kodda kullanılmasa da tasarımcı tarafından kullanılıyor
namespace CeyPASS.WFA.UserControls.EO
{
    partial class ucAylikPuantajEkrani
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
            this.pnlGridContainer = new System.Windows.Forms.Panel();
            this.dgPuantaj = new System.Windows.Forms.DataGridView();
            this.pnlTopBar = new System.Windows.Forms.Panel();
            this.tlpFilters = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbAySecimi = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbFirmaSecimi = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbIsyeriSecimi = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbAdSoyad = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtEkKayitGunu = new System.Windows.Forms.TextBox();
            this.btnEkKayitAyarla = new System.Windows.Forms.Button();
            this.btnCokluSicileAktar = new System.Windows.Forms.Button();
            this.btnPuantajYap = new System.Windows.Forms.Button();
            this.pnlMain.SuspendLayout();
            this.pnlGridContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgPuantaj)).BeginInit();
            this.pnlTopBar.SuspendLayout();
            this.tlpFilters.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(245)))));
            this.pnlMain.Controls.Add(this.pnlGridContainer);
            this.pnlMain.Controls.Add(this.pnlTopBar);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMain.Size = new System.Drawing.Size(1446, 818);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlGridContainer
            // 
            this.pnlGridContainer.BackColor = System.Drawing.Color.White;
            this.pnlGridContainer.Controls.Add(this.dgPuantaj);
            this.pnlGridContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridContainer.Location = new System.Drawing.Point(10, 136);
            this.pnlGridContainer.Name = "pnlGridContainer";
            this.pnlGridContainer.Padding = new System.Windows.Forms.Padding(10);
            this.pnlGridContainer.Size = new System.Drawing.Size(1426, 672);
            this.pnlGridContainer.TabIndex = 1;
            // 
            // dgPuantaj
            // 
            this.dgPuantaj.BackgroundColor = System.Drawing.Color.White;
            this.dgPuantaj.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgPuantaj.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgPuantaj.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgPuantaj.Location = new System.Drawing.Point(10, 10);
            this.dgPuantaj.Name = "dgPuantaj";
            this.dgPuantaj.RowHeadersWidth = 51;
            this.dgPuantaj.Size = new System.Drawing.Size(1406, 652);
            this.dgPuantaj.TabIndex = 0;
            // 
            // pnlTopBar
            // 
            this.pnlTopBar.BackColor = System.Drawing.Color.White;
            this.pnlTopBar.Controls.Add(this.tlpFilters);
            this.pnlTopBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopBar.Location = new System.Drawing.Point(10, 10);
            this.pnlTopBar.Name = "pnlTopBar";
            this.pnlTopBar.Size = new System.Drawing.Size(1426, 126);
            this.pnlTopBar.TabIndex = 0;
            // 
            // tlpFilters
            // 
            this.tlpFilters.ColumnCount = 8;
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.02418F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.601707F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.0825F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.14936F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.534851F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13.79801F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.58037F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13F));
            this.tlpFilters.Controls.Add(this.label1, 0, 0);
            this.tlpFilters.Controls.Add(this.cmbAySecimi, 0, 1);
            this.tlpFilters.Controls.Add(this.label2, 1, 0);
            this.tlpFilters.Controls.Add(this.cmbFirmaSecimi, 1, 1);
            this.tlpFilters.Controls.Add(this.label3, 2, 0);
            this.tlpFilters.Controls.Add(this.cmbIsyeriSecimi, 2, 1);
            this.tlpFilters.Controls.Add(this.label4, 3, 0);
            this.tlpFilters.Controls.Add(this.cmbAdSoyad, 3, 1);
            this.tlpFilters.Controls.Add(this.label5, 4, 0);
            this.tlpFilters.Controls.Add(this.txtEkKayitGunu, 4, 1);
            this.tlpFilters.Controls.Add(this.btnEkKayitAyarla, 5, 1);
            this.tlpFilters.Controls.Add(this.btnCokluSicileAktar, 6, 1);
            this.tlpFilters.Controls.Add(this.btnPuantajYap, 7, 1);
            this.tlpFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFilters.Location = new System.Drawing.Point(0, 0);
            this.tlpFilters.Name = "tlpFilters";
            this.tlpFilters.Padding = new System.Windows.Forms.Padding(10);
            this.tlpFilters.RowCount = 2;
            this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlpFilters.Size = new System.Drawing.Size(1426, 126);
            this.tlpFilters.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(13, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ay Seçimi";
            // 
            // cmbAySecimi
            // 
            this.cmbAySecimi.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbAySecimi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAySecimi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbAySecimi.Location = new System.Drawing.Point(13, 55);
            this.cmbAySecimi.Name = "cmbAySecimi";
            this.cmbAySecimi.Size = new System.Drawing.Size(149, 31);
            this.cmbAySecimi.TabIndex = 1;
            this.cmbAySecimi.SelectedIndexChanged += new System.EventHandler(this.cmbAySecimi_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(168, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Firma";
            // 
            // cmbFirmaSecimi
            // 
            this.cmbFirmaSecimi.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbFirmaSecimi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFirmaSecimi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbFirmaSecimi.Location = new System.Drawing.Point(168, 55);
            this.cmbFirmaSecimi.Name = "cmbFirmaSecimi";
            this.cmbFirmaSecimi.Size = new System.Drawing.Size(129, 31);
            this.cmbFirmaSecimi.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.Gray;
            this.label3.Location = new System.Drawing.Point(303, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 4;
            this.label3.Text = "İşyeri";
            // 
            // cmbIsyeriSecimi
            // 
            this.cmbIsyeriSecimi.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbIsyeriSecimi.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIsyeriSecimi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbIsyeriSecimi.Location = new System.Drawing.Point(303, 55);
            this.cmbIsyeriSecimi.Name = "cmbIsyeriSecimi";
            this.cmbIsyeriSecimi.Size = new System.Drawing.Size(192, 31);
            this.cmbIsyeriSecimi.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.Gray;
            this.label4.Location = new System.Drawing.Point(501, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 6;
            this.label4.Text = "Personel";
            // 
            // cmbAdSoyad
            // 
            this.cmbAdSoyad.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbAdSoyad.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAdSoyad.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbAdSoyad.Location = new System.Drawing.Point(501, 55);
            this.cmbAdSoyad.Name = "cmbAdSoyad";
            this.cmbAdSoyad.Size = new System.Drawing.Size(207, 31);
            this.cmbAdSoyad.TabIndex = 7;
            this.cmbAdSoyad.SelectedValueChanged += new System.EventHandler(this.cmbAdSoyad_SelectedValueChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.Gray;
            this.label5.Location = new System.Drawing.Point(714, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 23);
            this.label5.TabIndex = 8;
            this.label5.Text = "Ek Gün";
            // 
            // txtEkKayitGunu
            // 
            this.txtEkKayitGunu.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtEkKayitGunu.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtEkKayitGunu.Location = new System.Drawing.Point(714, 55);
            this.txtEkKayitGunu.Name = "txtEkKayitGunu";
            this.txtEkKayitGunu.Size = new System.Drawing.Size(114, 30);
            this.txtEkKayitGunu.TabIndex = 9;
            this.txtEkKayitGunu.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnEkKayitAyarla
            // 
            this.btnEkKayitAyarla.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(193)))), ((int)(((byte)(7)))));
            this.btnEkKayitAyarla.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnEkKayitAyarla.FlatAppearance.BorderSize = 0;
            this.btnEkKayitAyarla.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEkKayitAyarla.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEkKayitAyarla.ForeColor = System.Drawing.Color.Black;
            this.btnEkKayitAyarla.Image = global::CeyPASS.WFA.Properties.Resources.icons8_change_25;
            this.btnEkKayitAyarla.Location = new System.Drawing.Point(834, 52);
            this.btnEkKayitAyarla.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            this.btnEkKayitAyarla.Name = "btnEkKayitAyarla";
            this.btnEkKayitAyarla.Size = new System.Drawing.Size(188, 59);
            this.btnEkKayitAyarla.TabIndex = 10;
            this.btnEkKayitAyarla.Text = "Ek Kayıt Ayarla";
            this.btnEkKayitAyarla.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnEkKayitAyarla.UseVisualStyleBackColor = false;
            this.btnEkKayitAyarla.Click += new System.EventHandler(this.btnEkKayitAyarla_Click);
            // 
            // btnCokluSicileAktar
            // 
            this.btnCokluSicileAktar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.btnCokluSicileAktar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCokluSicileAktar.FlatAppearance.BorderSize = 0;
            this.btnCokluSicileAktar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCokluSicileAktar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCokluSicileAktar.ForeColor = System.Drawing.Color.White;
            this.btnCokluSicileAktar.Image = global::CeyPASS.WFA.Properties.Resources.icons8_data_transfer_25;
            this.btnCokluSicileAktar.Location = new System.Drawing.Point(1028, 52);
            this.btnCokluSicileAktar.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            this.btnCokluSicileAktar.Name = "btnCokluSicileAktar";
            this.btnCokluSicileAktar.Size = new System.Drawing.Size(199, 59);
            this.btnCokluSicileAktar.TabIndex = 11;
            this.btnCokluSicileAktar.Text = "Çoklu Sicil Aktar";
            this.btnCokluSicileAktar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCokluSicileAktar.UseVisualStyleBackColor = false;
            this.btnCokluSicileAktar.Click += new System.EventHandler(this.btnCokluSicileAktar_Click);
            // 
            // btnPuantajYap
            // 
            this.btnPuantajYap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnPuantajYap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPuantajYap.FlatAppearance.BorderSize = 0;
            this.btnPuantajYap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPuantajYap.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnPuantajYap.ForeColor = System.Drawing.Color.White;
            this.btnPuantajYap.Image = global::CeyPASS.WFA.Properties.Resources.icons8_excel_25;
            this.btnPuantajYap.Location = new System.Drawing.Point(1233, 52);
            this.btnPuantajYap.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
            this.btnPuantajYap.Name = "btnPuantajYap";
            this.btnPuantajYap.Size = new System.Drawing.Size(180, 59);
            this.btnPuantajYap.TabIndex = 12;
            this.btnPuantajYap.Text = "Puantaj Yap";
            this.btnPuantajYap.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPuantajYap.UseVisualStyleBackColor = false;
            this.btnPuantajYap.Click += new System.EventHandler(this.btnPuantajYap_Click);
            // 
            // ucAylikPuantajEkrani
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "ucAylikPuantajEkrani";
            this.Size = new System.Drawing.Size(1446, 818);
            this.Load += new System.EventHandler(this.ucAylikPuantajEkrani_Load);
            this.pnlMain.ResumeLayout(false);
            this.pnlGridContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgPuantaj)).EndInit();
            this.pnlTopBar.ResumeLayout(false);
            this.tlpFilters.ResumeLayout(false);
            this.tlpFilters.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlTopBar;
        private System.Windows.Forms.TableLayoutPanel tlpFilters;
        private System.Windows.Forms.Panel pnlGridContainer;
        private System.Windows.Forms.DataGridView dgPuantaj;

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ComboBox cmbAySecimi;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbFirmaSecimi;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbIsyeriSecimi;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbAdSoyad;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtEkKayitGunu;

        private System.Windows.Forms.Button btnEkKayitAyarla;
        private System.Windows.Forms.Button btnCokluSicileAktar;
        private System.Windows.Forms.Button btnPuantajYap;
        private System.Windows.Forms.Label label6;
    }
}
