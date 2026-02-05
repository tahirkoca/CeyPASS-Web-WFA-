namespace CeyPASS.WFA.UserControls.Ayarlar
{
    partial class ucCihazlar
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
            this.pnlRight = new System.Windows.Forms.Panel();
            this.pnlFormContainer = new System.Windows.Forms.Panel();
            this.tlpForm = new System.Windows.Forms.TableLayoutPanel();
            this.label8 = new System.Windows.Forms.Label();
            this.txtCihazId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFirmaId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCihazAdi = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtIpAdres = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtAciklama = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbCihazTipleri = new System.Windows.Forms.ComboBox();
            this.lblHeaderInfo = new System.Windows.Forms.Label();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnVazgec = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.pnlToolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCihazSil = new System.Windows.Forms.PictureBox();
            this.btnCihazGuncelle = new System.Windows.Forms.PictureBox();
            this.btnCihazEkle = new System.Windows.Forms.PictureBox();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.chkCihazlar = new System.Windows.Forms.CheckedListBox();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.labelListHeader = new System.Windows.Forms.Label();
            this.pnlMain.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlFormContainer.SuspendLayout();
            this.tlpForm.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnCihazSil)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCihazGuncelle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCihazEkle)).BeginInit();
            this.pnlLeft.SuspendLayout();
            this.pnlLeftHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.pnlMain.Controls.Add(this.pnlRight);
            this.pnlMain.Controls.Add(this.pnlLeft);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMain.Size = new System.Drawing.Size(1305, 755);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.White;
            this.pnlRight.Controls.Add(this.pnlFormContainer);
            this.pnlRight.Controls.Add(this.pnlFooter);
            this.pnlRight.Controls.Add(this.pnlToolbar);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(410, 10);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(885, 735);
            this.pnlRight.TabIndex = 1;
            // 
            // pnlFormContainer
            // 
            this.pnlFormContainer.AutoScroll = true;
            this.pnlFormContainer.Controls.Add(this.tlpForm);
            this.pnlFormContainer.Controls.Add(this.lblHeaderInfo);
            this.pnlFormContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFormContainer.Location = new System.Drawing.Point(0, 60);
            this.pnlFormContainer.Name = "pnlFormContainer";
            this.pnlFormContainer.Padding = new System.Windows.Forms.Padding(20);
            this.pnlFormContainer.Size = new System.Drawing.Size(885, 595);
            this.pnlFormContainer.TabIndex = 2;
            // 
            // tlpForm
            // 
            this.tlpForm.ColumnCount = 2;
            this.tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tlpForm.Controls.Add(this.label8, 0, 0);
            this.tlpForm.Controls.Add(this.txtCihazId, 1, 0);
            this.tlpForm.Controls.Add(this.label1, 0, 1);
            this.tlpForm.Controls.Add(this.txtFirmaId, 1, 1);
            this.tlpForm.Controls.Add(this.label2, 0, 2);
            this.tlpForm.Controls.Add(this.txtCihazAdi, 1, 2);
            this.tlpForm.Controls.Add(this.label3, 0, 3);
            this.tlpForm.Controls.Add(this.txtIpAdres, 1, 3);
            this.tlpForm.Controls.Add(this.label4, 0, 4);
            this.tlpForm.Controls.Add(this.txtPort, 1, 4);
            this.tlpForm.Controls.Add(this.label5, 0, 5);
            this.tlpForm.Controls.Add(this.txtAciklama, 1, 5);
            this.tlpForm.Controls.Add(this.label6, 0, 6);
            this.tlpForm.Controls.Add(this.cmbCihazTipleri, 1, 6);
            this.tlpForm.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpForm.Location = new System.Drawing.Point(20, 68);
            this.tlpForm.Name = "tlpForm";
            this.tlpForm.RowCount = 7;
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpForm.Size = new System.Drawing.Size(845, 390);
            this.tlpForm.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label8.Location = new System.Drawing.Point(3, 13);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(76, 23);
            this.label8.TabIndex = 0;
            this.label8.Text = "Cihaz Id:";
            // 
            // txtCihazId
            // 
            this.txtCihazId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCihazId.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtCihazId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCihazId.Enabled = false;
            this.txtCihazId.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtCihazId.Location = new System.Drawing.Point(172, 10);
            this.txtCihazId.Name = "txtCihazId";
            this.txtCihazId.Size = new System.Drawing.Size(670, 30);
            this.txtCihazId.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label1.Location = new System.Drawing.Point(3, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Firma Id:";
            // 
            // txtFirmaId
            // 
            this.txtFirmaId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFirmaId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFirmaId.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtFirmaId.Location = new System.Drawing.Point(172, 60);
            this.txtFirmaId.Name = "txtFirmaId";
            this.txtFirmaId.Size = new System.Drawing.Size(670, 30);
            this.txtFirmaId.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label2.Location = new System.Drawing.Point(3, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 23);
            this.label2.TabIndex = 4;
            this.label2.Text = "Cihaz Adı:";
            // 
            // txtCihazAdi
            // 
            this.txtCihazAdi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCihazAdi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCihazAdi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtCihazAdi.Location = new System.Drawing.Point(172, 110);
            this.txtCihazAdi.Name = "txtCihazAdi";
            this.txtCihazAdi.Size = new System.Drawing.Size(670, 30);
            this.txtCihazAdi.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label3.Location = new System.Drawing.Point(3, 163);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "IP Adres:";
            // 
            // txtIpAdres
            // 
            this.txtIpAdres.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIpAdres.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtIpAdres.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtIpAdres.Location = new System.Drawing.Point(172, 160);
            this.txtIpAdres.Name = "txtIpAdres";
            this.txtIpAdres.Size = new System.Drawing.Size(670, 30);
            this.txtIpAdres.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label4.Location = new System.Drawing.Point(3, 213);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 23);
            this.label4.TabIndex = 8;
            this.label4.Text = "Port:";
            // 
            // txtPort
            // 
            this.txtPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPort.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPort.Location = new System.Drawing.Point(172, 210);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(670, 30);
            this.txtPort.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label5.Location = new System.Drawing.Point(3, 278);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 23);
            this.label5.TabIndex = 10;
            this.label5.Text = "Açıklama:";
            // 
            // txtAciklama
            // 
            this.txtAciklama.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAciklama.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAciklama.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtAciklama.Location = new System.Drawing.Point(172, 253);
            this.txtAciklama.Multiline = true;
            this.txtAciklama.Name = "txtAciklama";
            this.txtAciklama.Size = new System.Drawing.Size(670, 74);
            this.txtAciklama.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label6.Location = new System.Drawing.Point(3, 348);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 23);
            this.label6.TabIndex = 12;
            this.label6.Text = "Cihaz Tipi:";
            // 
            // cmbCihazTipleri
            // 
            this.cmbCihazTipleri.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCihazTipleri.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbCihazTipleri.FormattingEnabled = true;
            this.cmbCihazTipleri.Location = new System.Drawing.Point(172, 344);
            this.cmbCihazTipleri.Name = "cmbCihazTipleri";
            this.cmbCihazTipleri.Size = new System.Drawing.Size(670, 31);
            this.cmbCihazTipleri.TabIndex = 13;
            // 
            // lblHeaderInfo
            // 
            this.lblHeaderInfo.AutoSize = true;
            this.lblHeaderInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeaderInfo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblHeaderInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblHeaderInfo.Location = new System.Drawing.Point(20, 20);
            this.lblHeaderInfo.Name = "lblHeaderInfo";
            this.lblHeaderInfo.Padding = new System.Windows.Forms.Padding(0, 0, 0, 20);
            this.lblHeaderInfo.Size = new System.Drawing.Size(156, 48);
            this.lblHeaderInfo.TabIndex = 1;
            this.lblHeaderInfo.Text = "Cihaz Detayları";
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlFooter.Controls.Add(this.btnVazgec);
            this.pnlFooter.Controls.Add(this.btnKaydet);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 655);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(10);
            this.pnlFooter.Size = new System.Drawing.Size(885, 80);
            this.pnlFooter.TabIndex = 1;
            // 
            // btnVazgec
            // 
            this.btnVazgec.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnVazgec.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVazgec.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnVazgec.FlatAppearance.BorderSize = 0;
            this.btnVazgec.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVazgec.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnVazgec.ForeColor = System.Drawing.Color.White;
            this.btnVazgec.Location = new System.Drawing.Point(725, 10);
            this.btnVazgec.Name = "btnVazgec";
            this.btnVazgec.Size = new System.Drawing.Size(150, 60);
            this.btnVazgec.TabIndex = 1;
            this.btnVazgec.Text = "Vazgeç";
            this.btnVazgec.UseVisualStyleBackColor = false;
            // 
            // btnKaydet
            // 
            this.btnKaydet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnKaydet.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnKaydet.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnKaydet.FlatAppearance.BorderSize = 0;
            this.btnKaydet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKaydet.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnKaydet.ForeColor = System.Drawing.Color.White;
            this.btnKaydet.Location = new System.Drawing.Point(10, 10);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(150, 60);
            this.btnKaydet.TabIndex = 0;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = false;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlToolbar.Controls.Add(this.btnCihazSil);
            this.pnlToolbar.Controls.Add(this.btnCihazGuncelle);
            this.pnlToolbar.Controls.Add(this.btnCihazEkle);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 0);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(5);
            this.pnlToolbar.Size = new System.Drawing.Size(885, 60);
            this.pnlToolbar.TabIndex = 0;
            // 
            // btnCihazSil
            // 
            this.btnCihazSil.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCihazSil.Image = global::CeyPASS.WFA.Properties.Resources.icons8_minus_50;
            this.btnCihazSil.Location = new System.Drawing.Point(822, 8);
            this.btnCihazSil.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnCihazSil.Name = "btnCihazSil";
            this.btnCihazSil.Size = new System.Drawing.Size(43, 43);
            this.btnCihazSil.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnCihazSil.TabIndex = 1;
            this.btnCihazSil.TabStop = false;
            // 
            // btnCihazGuncelle
            // 
            this.btnCihazGuncelle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCihazGuncelle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_update_50;
            this.btnCihazGuncelle.Location = new System.Drawing.Point(766, 8);
            this.btnCihazGuncelle.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnCihazGuncelle.Name = "btnCihazGuncelle";
            this.btnCihazGuncelle.Size = new System.Drawing.Size(43, 43);
            this.btnCihazGuncelle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnCihazGuncelle.TabIndex = 2;
            this.btnCihazGuncelle.TabStop = false;
            // 
            // btnCihazEkle
            // 
            this.btnCihazEkle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCihazEkle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_add_50;
            this.btnCihazEkle.Location = new System.Drawing.Point(710, 8);
            this.btnCihazEkle.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnCihazEkle.Name = "btnCihazEkle";
            this.btnCihazEkle.Size = new System.Drawing.Size(43, 43);
            this.btnCihazEkle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.btnCihazEkle.TabIndex = 0;
            this.btnCihazEkle.TabStop = false;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Controls.Add(this.chkCihazlar);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(10, 10);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(1);
            this.pnlLeft.Size = new System.Drawing.Size(400, 735);
            this.pnlLeft.TabIndex = 0;
            // 
            // chkCihazlar
            // 
            this.chkCihazlar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkCihazlar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkCihazlar.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkCihazlar.FormattingEnabled = true;
            this.chkCihazlar.HorizontalScrollbar = true;
            this.chkCihazlar.Location = new System.Drawing.Point(1, 51);
            this.chkCihazlar.Name = "chkCihazlar";
            this.chkCihazlar.Size = new System.Drawing.Size(398, 683);
            this.chkCihazlar.TabIndex = 1;
            // 
            // pnlLeftHeader
            // 
            this.pnlLeftHeader.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlLeftHeader.Controls.Add(this.labelListHeader);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(1, 1);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(398, 50);
            this.pnlLeftHeader.TabIndex = 0;
            // 
            // labelListHeader
            // 
            this.labelListHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelListHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.labelListHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labelListHeader.Location = new System.Drawing.Point(0, 0);
            this.labelListHeader.Name = "labelListHeader";
            this.labelListHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.labelListHeader.Size = new System.Drawing.Size(398, 50);
            this.labelListHeader.TabIndex = 0;
            this.labelListHeader.Text = "Kayıtlı Cihazlar";
            this.labelListHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ucCihazlar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "ucCihazlar";
            this.Size = new System.Drawing.Size(1305, 755);
            this.Load += new System.EventHandler(this.ucCihazlar_Load);
            this.pnlMain.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.pnlFormContainer.ResumeLayout(false);
            this.pnlFormContainer.PerformLayout();
            this.tlpForm.ResumeLayout(false);
            this.tlpForm.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnCihazSil)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCihazGuncelle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCihazEkle)).EndInit();
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeftHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        // Ana Konteynerlar
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlFormContainer;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.FlowLayoutPanel pnlToolbar;
        private System.Windows.Forms.Panel pnlLeftHeader;
        private System.Windows.Forms.Label labelListHeader;

        // Liste
        private System.Windows.Forms.CheckedListBox chkCihazlar;

        // Form Elemanları
        private System.Windows.Forms.Label lblHeaderInfo;
        private System.Windows.Forms.TableLayoutPanel tlpForm;

        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtCihazId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFirmaId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCihazAdi;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtIpAdres;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtAciklama;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbCihazTipleri;

        // Butonlar (Orijinal tipleri korundu)
        private System.Windows.Forms.Button btnVazgec;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.PictureBox btnCihazSil;
        private System.Windows.Forms.PictureBox btnCihazGuncelle;
        private System.Windows.Forms.PictureBox btnCihazEkle;


    }
}
