namespace CeyPASS.WFA.UserControls.Raporlar
{
    partial class ucRaporlar
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
            this.dgRaporlar = new System.Windows.Forms.DataGridView();
            this.pnlFilters = new System.Windows.Forms.Panel();
            this.tlpFilters = new System.Windows.Forms.TableLayoutPanel();
            this.pnlDateFilters = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpGirisTarihi = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpCikisTarihi = new System.Windows.Forms.DateTimePicker();
            this.pnlSearchFilters = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbRaporTurleri = new System.Windows.Forms.ComboBox();
            this.lblFirma = new System.Windows.Forms.Label();
            this.cmbFirma = new System.Windows.Forms.ComboBox();
            this.txtFiltrele = new System.Windows.Forms.TextBox();
            this.btnFiltrele = new System.Windows.Forms.Button();
            this.pnlActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPdfDonustur = new System.Windows.Forms.Button();
            this.btnExceleDonustur = new System.Windows.Forms.Button();
            this.btnRaporGetir = new System.Windows.Forms.Button();
            this.pnlIsyeriFilters = new System.Windows.Forms.Panel();
            this.lblIsyerleri = new System.Windows.Forms.Label();
            this.chkRaporIsyerleri = new System.Windows.Forms.CheckedListBox();
            this.pnlMain.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlGridContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgRaporlar)).BeginInit();
            this.pnlFilters.SuspendLayout();
            this.tlpFilters.SuspendLayout();
            this.pnlDateFilters.SuspendLayout();
            this.pnlSearchFilters.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.pnlIsyeriFilters.SuspendLayout();
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
            this.pnlMain.Size = new System.Drawing.Size(1378, 841);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.pnlGridContainer);
            this.pnlContent.Controls.Add(this.pnlFilters);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(10, 10);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(1358, 821);
            this.pnlContent.TabIndex = 1;
            // 
            // pnlGridContainer
            // 
            this.pnlGridContainer.BackColor = System.Drawing.Color.White;
            this.pnlGridContainer.Controls.Add(this.dgRaporlar);
            this.pnlGridContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridContainer.Location = new System.Drawing.Point(0, 250);
            this.pnlGridContainer.Name = "pnlGridContainer";
            this.pnlGridContainer.Padding = new System.Windows.Forms.Padding(10, 6, 10, 10);
            this.pnlGridContainer.Size = new System.Drawing.Size(1358, 573);
            this.pnlGridContainer.TabIndex = 2;
            // 
            // dgRaporlar
            // 
            this.dgRaporlar.BackgroundColor = System.Drawing.Color.White;
            this.dgRaporlar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgRaporlar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgRaporlar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgRaporlar.Location = new System.Drawing.Point(10, 10);
            this.dgRaporlar.Name = "dgRaporlar";
            this.dgRaporlar.RowHeadersWidth = 51;
            this.dgRaporlar.Size = new System.Drawing.Size(1338, 587);
            this.dgRaporlar.TabIndex = 0;
            // 
            // pnlFilters
            // 
            this.pnlFilters.BackColor = System.Drawing.Color.White;
            this.pnlFilters.Controls.Add(this.tlpFilters);
            this.pnlFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilters.Location = new System.Drawing.Point(0, 0);
            this.pnlFilters.Name = "pnlFilters";
            this.pnlFilters.Size = new System.Drawing.Size(1358, 250);
            this.pnlFilters.TabIndex = 0;
            // 
            // tlpFilters
            // 
            this.tlpFilters.ColumnCount = 3;
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.28424F));
            this.tlpFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36.74521F));
            this.tlpFilters.Controls.Add(this.pnlDateFilters, 0, 0);
            this.tlpFilters.Controls.Add(this.pnlSearchFilters, 1, 0);
            this.tlpFilters.Controls.Add(this.pnlActions, 2, 0);
            this.tlpFilters.Controls.Add(this.pnlIsyeriFilters, 0, 1);
            this.tlpFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFilters.Location = new System.Drawing.Point(0, 0);
            this.tlpFilters.Name = "tlpFilters";
            this.tlpFilters.RowCount = 2;
            this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 158F));
            this.tlpFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tlpFilters.SetColumnSpan(this.pnlIsyeriFilters, 3);
            this.tlpFilters.Size = new System.Drawing.Size(1358, 250);
            this.tlpFilters.TabIndex = 0;
            // 
            // pnlDateFilters
            // 
            this.pnlDateFilters.Controls.Add(this.label2);
            this.pnlDateFilters.Controls.Add(this.dtpGirisTarihi);
            this.pnlDateFilters.Controls.Add(this.label3);
            this.pnlDateFilters.Controls.Add(this.dtpCikisTarihi);
            this.pnlDateFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDateFilters.Location = new System.Drawing.Point(3, 3);
            this.pnlDateFilters.Name = "pnlDateFilters";
            this.pnlDateFilters.Padding = new System.Windows.Forms.Padding(10);
            this.pnlDateFilters.Size = new System.Drawing.Size(401, 161);
            this.pnlDateFilters.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(10, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "Giriş Tarihi";
            // 
            // dtpGirisTarihi
            // 
            this.dtpGirisTarihi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpGirisTarihi.Location = new System.Drawing.Point(10, 33);
            this.dtpGirisTarihi.Name = "dtpGirisTarihi";
            this.dtpGirisTarihi.Size = new System.Drawing.Size(250, 30);
            this.dtpGirisTarihi.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.Gray;
            this.label3.Location = new System.Drawing.Point(10, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(200, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Çıkış Tarihi";
            // 
            // dtpCikisTarihi
            // 
            this.dtpCikisTarihi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpCikisTarihi.Location = new System.Drawing.Point(10, 93);
            this.dtpCikisTarihi.Name = "dtpCikisTarihi";
            this.dtpCikisTarihi.Size = new System.Drawing.Size(250, 30);
            this.dtpCikisTarihi.TabIndex = 3;
            // 
            // pnlSearchFilters
            // 
            this.pnlSearchFilters.Controls.Add(this.label4);
            this.pnlSearchFilters.Controls.Add(this.cmbRaporTurleri);
            this.pnlSearchFilters.Controls.Add(this.lblFirma);
            this.pnlSearchFilters.Controls.Add(this.cmbFirma);
            this.pnlSearchFilters.Controls.Add(this.txtFiltrele);
            this.pnlSearchFilters.Controls.Add(this.btnFiltrele);
            this.pnlSearchFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSearchFilters.Location = new System.Drawing.Point(410, 3);
            this.pnlSearchFilters.Name = "pnlSearchFilters";
            this.pnlSearchFilters.Padding = new System.Windows.Forms.Padding(10);
            this.pnlSearchFilters.Size = new System.Drawing.Size(445, 152);
            this.pnlSearchFilters.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.Gray;
            this.label4.Location = new System.Drawing.Point(10, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(200, 18);
            this.label4.TabIndex = 2;
            this.label4.Text = "Rapor Türü";
            // 
            // cmbRaporTurleri
            // 
            this.cmbRaporTurleri.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRaporTurleri.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbRaporTurleri.Location = new System.Drawing.Point(10, 76);
            this.cmbRaporTurleri.Name = "cmbRaporTurleri";
            this.cmbRaporTurleri.Size = new System.Drawing.Size(400, 31);
            this.cmbRaporTurleri.TabIndex = 3;
            // 
            // lblFirma
            // 
            this.lblFirma.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFirma.ForeColor = System.Drawing.Color.Gray;
            this.lblFirma.Location = new System.Drawing.Point(10, 6);
            this.lblFirma.Name = "lblFirma";
            this.lblFirma.Size = new System.Drawing.Size(200, 18);
            this.lblFirma.TabIndex = 0;
            this.lblFirma.Text = "Firma";
            // 
            // cmbFirma
            // 
            this.cmbFirma.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFirma.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbFirma.Location = new System.Drawing.Point(10, 24);
            this.cmbFirma.Name = "cmbFirma";
            this.cmbFirma.Size = new System.Drawing.Size(400, 31);
            this.cmbFirma.TabIndex = 1;
            // 
            // txtFiltrele
            // 
            this.txtFiltrele.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
            this.txtFiltrele.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtFiltrele.Location = new System.Drawing.Point(10, 114);
            this.txtFiltrele.Name = "txtFiltrele";
            this.txtFiltrele.Size = new System.Drawing.Size(312, 30);
            this.txtFiltrele.TabIndex = 2;
            this.txtFiltrele.TextChanged += new System.EventHandler(this.txtFiltrele_TextChanged);
            // 
            // btnFiltrele
            // 
            this.btnFiltrele.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
            this.btnFiltrele.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnFiltrele.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFiltrele.Image = global::CeyPASS.WFA.Properties.Resources.icons8_search_251;
            this.btnFiltrele.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnFiltrele.Location = new System.Drawing.Point(328, 114);
            this.btnFiltrele.Name = "btnFiltrele";
            this.btnFiltrele.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.btnFiltrele.Size = new System.Drawing.Size(82, 30);
            this.btnFiltrele.TabIndex = 3;
            this.btnFiltrele.Text = "Ara";
            this.btnFiltrele.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnFiltrele.UseVisualStyleBackColor = false;
            // 
            // pnlActions
            // 
            this.pnlActions.Controls.Add(this.btnPdfDonustur);
            this.pnlActions.Controls.Add(this.btnExceleDonustur);
            this.pnlActions.Controls.Add(this.btnRaporGetir);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlActions.Location = new System.Drawing.Point(861, 3);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.pnlActions.Size = new System.Drawing.Size(494, 161);
            this.pnlActions.TabIndex = 2;
            // 
            // btnPdfDonustur
            // 
            this.btnPdfDonustur.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnPdfDonustur.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPdfDonustur.FlatAppearance.BorderSize = 0;
            this.btnPdfDonustur.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPdfDonustur.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnPdfDonustur.ForeColor = System.Drawing.Color.White;
            this.btnPdfDonustur.Image = global::CeyPASS.WFA.Properties.Resources.icons_pdf_50;
            this.btnPdfDonustur.Location = new System.Drawing.Point(327, 13);
            this.btnPdfDonustur.Margin = new System.Windows.Forms.Padding(5, 8, 5, 5);
            this.btnPdfDonustur.Name = "btnPdfDonustur";
            this.btnPdfDonustur.Size = new System.Drawing.Size(142, 82);
            this.btnPdfDonustur.TabIndex = 0;
            this.btnPdfDonustur.Text = "PDF";
            this.btnPdfDonustur.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPdfDonustur.UseVisualStyleBackColor = false;
            this.btnPdfDonustur.Click += new System.EventHandler(this.btnPdfDonustur_Click);
            // 
            // btnExceleDonustur
            // 
            this.btnExceleDonustur.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnExceleDonustur.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExceleDonustur.FlatAppearance.BorderSize = 0;
            this.btnExceleDonustur.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExceleDonustur.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExceleDonustur.ForeColor = System.Drawing.Color.White;
            this.btnExceleDonustur.Image = global::CeyPASS.WFA.Properties.Resources.icons8_excel_50;
            this.btnExceleDonustur.Location = new System.Drawing.Point(176, 13);
            this.btnExceleDonustur.Margin = new System.Windows.Forms.Padding(5, 8, 5, 5);
            this.btnExceleDonustur.Name = "btnExceleDonustur";
            this.btnExceleDonustur.Size = new System.Drawing.Size(141, 82);
            this.btnExceleDonustur.TabIndex = 1;
            this.btnExceleDonustur.Text = "Excel";
            this.btnExceleDonustur.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnExceleDonustur.UseVisualStyleBackColor = false;
            this.btnExceleDonustur.Click += new System.EventHandler(this.btnExceleDonustur_Click);
            // 
            // btnRaporGetir
            // 
            this.btnRaporGetir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnRaporGetir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRaporGetir.FlatAppearance.BorderSize = 0;
            this.btnRaporGetir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRaporGetir.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRaporGetir.ForeColor = System.Drawing.Color.White;
            this.btnRaporGetir.Image = global::CeyPASS.WFA.Properties.Resources.icons8_search_50;
            this.btnRaporGetir.Location = new System.Drawing.Point(27, 13);
            this.btnRaporGetir.Margin = new System.Windows.Forms.Padding(5, 8, 5, 5);
            this.btnRaporGetir.Name = "btnRaporGetir";
            this.btnRaporGetir.Size = new System.Drawing.Size(139, 82);
            this.btnRaporGetir.TabIndex = 2;
            this.btnRaporGetir.Text = "Getir";
            this.btnRaporGetir.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnRaporGetir.UseVisualStyleBackColor = false;
            this.btnRaporGetir.Click += new System.EventHandler(this.btnRaporGetir_Click);
            // 
            // pnlIsyeriFilters
            // 
            this.pnlIsyeriFilters.Controls.Add(this.chkRaporIsyerleri);
            this.pnlIsyeriFilters.Controls.Add(this.lblIsyerleri);
            this.pnlIsyeriFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlIsyeriFilters.Location = new System.Drawing.Point(3, 129);
            this.pnlIsyeriFilters.Name = "pnlIsyeriFilters";
            this.pnlIsyeriFilters.Padding = new System.Windows.Forms.Padding(10, 0, 10, 2);
            this.pnlIsyeriFilters.Size = new System.Drawing.Size(1352, 86);
            this.pnlIsyeriFilters.TabIndex = 3;
            // 
            // lblIsyerleri
            // 
            this.lblIsyerleri.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblIsyerleri.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblIsyerleri.ForeColor = System.Drawing.Color.Gray;
            this.lblIsyerleri.Location = new System.Drawing.Point(10, 2);
            this.lblIsyerleri.Name = "lblIsyerleri";
            this.lblIsyerleri.Size = new System.Drawing.Size(1332, 18);
            this.lblIsyerleri.TabIndex = 0;
            this.lblIsyerleri.Text = "İşyerleri (işaretlenmezse yetkili tüm işyerler)";
            // 
            // chkRaporIsyerleri
            // 
            this.chkRaporIsyerleri.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.chkRaporIsyerleri.CheckOnClick = true;
            this.chkRaporIsyerleri.ColumnWidth = 220;
            this.chkRaporIsyerleri.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRaporIsyerleri.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkRaporIsyerleri.FormattingEnabled = true;
            this.chkRaporIsyerleri.IntegralHeight = false;
            this.chkRaporIsyerleri.Location = new System.Drawing.Point(10, 18);
            this.chkRaporIsyerleri.MultiColumn = true;
            this.chkRaporIsyerleri.Name = "chkRaporIsyerleri";
            this.chkRaporIsyerleri.Size = new System.Drawing.Size(1332, 64);
            this.chkRaporIsyerleri.TabIndex = 1;
            // 
            // ucRaporlar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "ucRaporlar";
            this.Size = new System.Drawing.Size(1378, 841);
            this.Load += new System.EventHandler(this.ucRaporlar_Load);
            this.pnlMain.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlGridContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgRaporlar)).EndInit();
            this.pnlFilters.ResumeLayout(false);
            this.tlpFilters.ResumeLayout(false);
            this.pnlDateFilters.ResumeLayout(false);
            this.pnlSearchFilters.ResumeLayout(false);
            this.pnlSearchFilters.PerformLayout();
            this.pnlActions.ResumeLayout(false);
            this.pnlIsyeriFilters.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlContent;

        private System.Windows.Forms.Panel pnlFilters;
        private System.Windows.Forms.TableLayoutPanel tlpFilters;
        private System.Windows.Forms.Panel pnlDateFilters;
        private System.Windows.Forms.Panel pnlSearchFilters;
        private System.Windows.Forms.FlowLayoutPanel pnlActions;

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpGirisTarihi;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtpCikisTarihi;

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbRaporTurleri;
        private System.Windows.Forms.Label lblFirma;
        private System.Windows.Forms.ComboBox cmbFirma;
        private System.Windows.Forms.TextBox txtFiltrele;
        private System.Windows.Forms.Button btnFiltrele;

        private System.Windows.Forms.Button btnPdfDonustur;
        private System.Windows.Forms.Button btnExceleDonustur;
        private System.Windows.Forms.Button btnRaporGetir;

        private System.Windows.Forms.Panel pnlGridContainer;
        private System.Windows.Forms.DataGridView dgRaporlar;

        private System.Windows.Forms.Panel pnlIsyeriFilters;
        private System.Windows.Forms.Label lblIsyerleri;
        private System.Windows.Forms.CheckedListBox chkRaporIsyerleri;
    }
}
