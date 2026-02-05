namespace CeyPASS.WFA.UserControls
{
    partial class ucDepartmanTanimlama
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
            this.txtDepartmanId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDepartmanAdi = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDepartmanAciklama = new System.Windows.Forms.TextBox();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnVazgec = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.pnlToolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnDepartmanGuncelle = new System.Windows.Forms.Button();
            this.btnDepartmanSil = new System.Windows.Forms.Button();
            this.btnDepartmanEkle = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.chkDepartmanlar = new System.Windows.Forms.CheckedListBox();
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
            this.pnlMain.Size = new System.Drawing.Size(1200, 716);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.pnlCard);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(310, 10);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.pnlContent.Size = new System.Drawing.Size(880, 696);
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
            this.pnlCard.Size = new System.Drawing.Size(860, 696);
            this.pnlCard.TabIndex = 0;
            // 
            // tlpForm
            // 
            this.tlpForm.ColumnCount = 1;
            this.tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpForm.Controls.Add(this.label8, 0, 0);
            this.tlpForm.Controls.Add(this.txtDepartmanId, 0, 1);
            this.tlpForm.Controls.Add(this.label1, 0, 2);
            this.tlpForm.Controls.Add(this.txtDepartmanAdi, 0, 3);
            this.tlpForm.Controls.Add(this.label2, 0, 4);
            this.tlpForm.Controls.Add(this.txtDepartmanAciklama, 0, 5);
            this.tlpForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpForm.Location = new System.Drawing.Point(0, 197);
            this.tlpForm.Name = "tlpForm";
            this.tlpForm.Padding = new System.Windows.Forms.Padding(40, 10, 40, 0);
            this.tlpForm.RowCount = 7;
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpForm.Size = new System.Drawing.Size(860, 399);
            this.tlpForm.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label8.ForeColor = System.Drawing.Color.Gray;
            this.label8.Location = new System.Drawing.Point(43, 15);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(106, 20);
            this.label8.TabIndex = 0;
            this.label8.Text = "Departman Id";
            // 
            // txtDepartmanId
            // 
            this.txtDepartmanId.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtDepartmanId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDepartmanId.Enabled = false;
            this.txtDepartmanId.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtDepartmanId.Location = new System.Drawing.Point(43, 38);
            this.txtDepartmanId.Name = "txtDepartmanId";
            this.txtDepartmanId.Size = new System.Drawing.Size(774, 32);
            this.txtDepartmanId.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(43, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Departman Adı";
            // 
            // txtDepartmanAdi
            // 
            this.txtDepartmanAdi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDepartmanAdi.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtDepartmanAdi.Location = new System.Drawing.Point(43, 103);
            this.txtDepartmanAdi.Name = "txtDepartmanAdi";
            this.txtDepartmanAdi.Size = new System.Drawing.Size(774, 32);
            this.txtDepartmanAdi.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(43, 145);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Açıklama";
            // 
            // txtDepartmanAciklama
            // 
            this.txtDepartmanAciklama.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDepartmanAciklama.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtDepartmanAciklama.Location = new System.Drawing.Point(43, 168);
            this.txtDepartmanAciklama.Multiline = true;
            this.txtDepartmanAciklama.Name = "txtDepartmanAciklama";
            this.txtDepartmanAciklama.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDepartmanAciklama.Size = new System.Drawing.Size(774, 144);
            this.txtDepartmanAciklama.TabIndex = 5;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlFooter.Controls.Add(this.btnVazgec);
            this.pnlFooter.Controls.Add(this.btnKaydet);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 596);
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
            this.btnVazgec.TabIndex = 1;
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
            this.btnKaydet.TabIndex = 0;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = false;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlToolbar.Controls.Add(this.btnDepartmanGuncelle);
            this.pnlToolbar.Controls.Add(this.btnDepartmanSil);
            this.pnlToolbar.Controls.Add(this.btnDepartmanEkle);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 75);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(10, 15, 10, 15);
            this.pnlToolbar.Size = new System.Drawing.Size(860, 122);
            this.pnlToolbar.TabIndex = 1;
            // 
            // btnDepartmanGuncelle
            // 
            this.btnDepartmanGuncelle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.btnDepartmanGuncelle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDepartmanGuncelle.FlatAppearance.BorderSize = 0;
            this.btnDepartmanGuncelle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDepartmanGuncelle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDepartmanGuncelle.ForeColor = System.Drawing.Color.White;
            this.btnDepartmanGuncelle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_update_50;
            this.btnDepartmanGuncelle.Location = new System.Drawing.Point(663, 20);
            this.btnDepartmanGuncelle.Margin = new System.Windows.Forms.Padding(5);
            this.btnDepartmanGuncelle.Name = "btnDepartmanGuncelle";
            this.btnDepartmanGuncelle.Size = new System.Drawing.Size(172, 85);
            this.btnDepartmanGuncelle.TabIndex = 0;
            this.btnDepartmanGuncelle.Text = "Güncelle";
            this.btnDepartmanGuncelle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDepartmanGuncelle.UseVisualStyleBackColor = false;
            // 
            // btnDepartmanSil
            // 
            this.btnDepartmanSil.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnDepartmanSil.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDepartmanSil.FlatAppearance.BorderSize = 0;
            this.btnDepartmanSil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDepartmanSil.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDepartmanSil.ForeColor = System.Drawing.Color.White;
            this.btnDepartmanSil.Image = global::CeyPASS.WFA.Properties.Resources.icons8_minus_50;
            this.btnDepartmanSil.Location = new System.Drawing.Point(513, 20);
            this.btnDepartmanSil.Margin = new System.Windows.Forms.Padding(5);
            this.btnDepartmanSil.Name = "btnDepartmanSil";
            this.btnDepartmanSil.Size = new System.Drawing.Size(140, 85);
            this.btnDepartmanSil.TabIndex = 1;
            this.btnDepartmanSil.Text = "Sil";
            this.btnDepartmanSil.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDepartmanSil.UseVisualStyleBackColor = false;
            // 
            // btnDepartmanEkle
            // 
            this.btnDepartmanEkle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnDepartmanEkle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDepartmanEkle.FlatAppearance.BorderSize = 0;
            this.btnDepartmanEkle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDepartmanEkle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDepartmanEkle.ForeColor = System.Drawing.Color.White;
            this.btnDepartmanEkle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_add_50;
            this.btnDepartmanEkle.Location = new System.Drawing.Point(334, 20);
            this.btnDepartmanEkle.Margin = new System.Windows.Forms.Padding(5);
            this.btnDepartmanEkle.Name = "btnDepartmanEkle";
            this.btnDepartmanEkle.Size = new System.Drawing.Size(169, 85);
            this.btnDepartmanEkle.TabIndex = 2;
            this.btnDepartmanEkle.Text = "Ekle";
            this.btnDepartmanEkle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDepartmanEkle.UseVisualStyleBackColor = false;
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
            this.lblHeader.Text = "Departman Bilgileri";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Controls.Add(this.chkDepartmanlar);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(10, 10);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(1);
            this.pnlLeft.Size = new System.Drawing.Size(300, 696);
            this.pnlLeft.TabIndex = 2;
            // 
            // chkDepartmanlar
            // 
            this.chkDepartmanlar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkDepartmanlar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDepartmanlar.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.chkDepartmanlar.FormattingEnabled = true;
            this.chkDepartmanlar.HorizontalScrollbar = true;
            this.chkDepartmanlar.Location = new System.Drawing.Point(1, 51);
            this.chkDepartmanlar.Name = "chkDepartmanlar";
            this.chkDepartmanlar.Size = new System.Drawing.Size(298, 644);
            this.chkDepartmanlar.TabIndex = 1;
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
            this.lblListHeader.Text = "Departman Listesi";
            this.lblListHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ucDepartmanTanimlama
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "ucDepartmanTanimlama";
            this.Size = new System.Drawing.Size(1200, 716);
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
        private System.Windows.Forms.CheckedListBox chkDepartmanlar;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.FlowLayoutPanel pnlToolbar;
        private System.Windows.Forms.TableLayoutPanel tlpForm;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtDepartmanId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDepartmanAdi;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDepartmanAciklama;
        private System.Windows.Forms.Button btnDepartmanEkle;
        private System.Windows.Forms.Button btnDepartmanSil;
        private System.Windows.Forms.Button btnDepartmanGuncelle;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Button btnVazgec;
    }
}
