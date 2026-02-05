namespace CeyPASS.WFA.UserControls.VMY
{
    partial class ucCalismaStatuleri
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
            this.txtCalismaStatuId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCalismaStatuAdi = new System.Windows.Forms.TextBox();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnVazgec = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.pnlToolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCalismaStatuGuncelle = new System.Windows.Forms.Button();
            this.btnCalismaStatuSil = new System.Windows.Forms.Button();
            this.btnCalismaStatuEkle = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.chkCalismaStatuleri = new System.Windows.Forms.CheckedListBox();
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
            this.pnlMain.Size = new System.Drawing.Size(1073, 611);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.pnlCard);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(310, 10);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.pnlContent.Size = new System.Drawing.Size(753, 591);
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
            this.pnlCard.Size = new System.Drawing.Size(733, 591);
            this.pnlCard.TabIndex = 0;
            // 
            // tlpForm
            // 
            this.tlpForm.ColumnCount = 1;
            this.tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpForm.Controls.Add(this.label8, 0, 0);
            this.tlpForm.Controls.Add(this.txtCalismaStatuId, 0, 1);
            this.tlpForm.Controls.Add(this.label1, 0, 2);
            this.tlpForm.Controls.Add(this.txtCalismaStatuAdi, 0, 3);
            this.tlpForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpForm.Location = new System.Drawing.Point(0, 205);
            this.tlpForm.Name = "tlpForm";
            this.tlpForm.Padding = new System.Windows.Forms.Padding(40, 20, 40, 0);
            this.tlpForm.RowCount = 5;
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpForm.Size = new System.Drawing.Size(733, 286);
            this.tlpForm.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label8.ForeColor = System.Drawing.Color.Gray;
            this.label8.Location = new System.Drawing.Point(43, 27);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(139, 23);
            this.label8.TabIndex = 0;
            this.label8.Text = "Çalışma Statü ID";
            // 
            // txtCalismaStatuId
            // 
            this.txtCalismaStatuId.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtCalismaStatuId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCalismaStatuId.Enabled = false;
            this.txtCalismaStatuId.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtCalismaStatuId.Location = new System.Drawing.Point(43, 53);
            this.txtCalismaStatuId.Name = "txtCalismaStatuId";
            this.txtCalismaStatuId.Size = new System.Drawing.Size(647, 32);
            this.txtCalismaStatuId.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(43, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(150, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Çalışma Statü Adı";
            // 
            // txtCalismaStatuAdi
            // 
            this.txtCalismaStatuAdi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCalismaStatuAdi.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtCalismaStatuAdi.Location = new System.Drawing.Point(43, 128);
            this.txtCalismaStatuAdi.Name = "txtCalismaStatuAdi";
            this.txtCalismaStatuAdi.Size = new System.Drawing.Size(647, 32);
            this.txtCalismaStatuAdi.TabIndex = 3;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlFooter.Controls.Add(this.btnVazgec);
            this.pnlFooter.Controls.Add(this.btnKaydet);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 491);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(20);
            this.pnlFooter.Size = new System.Drawing.Size(733, 100);
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
            this.btnKaydet.Location = new System.Drawing.Point(583, 20);
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
            this.pnlToolbar.Controls.Add(this.btnCalismaStatuGuncelle);
            this.pnlToolbar.Controls.Add(this.btnCalismaStatuSil);
            this.pnlToolbar.Controls.Add(this.btnCalismaStatuEkle);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.pnlToolbar.Location = new System.Drawing.Point(0, 75);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(10);
            this.pnlToolbar.Size = new System.Drawing.Size(733, 130);
            this.pnlToolbar.TabIndex = 1;
            // 
            // btnCalismaStatuGuncelle
            // 
            this.btnCalismaStatuGuncelle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(162)))), ((int)(((byte)(184)))));
            this.btnCalismaStatuGuncelle.FlatAppearance.BorderSize = 0;
            this.btnCalismaStatuGuncelle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCalismaStatuGuncelle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCalismaStatuGuncelle.ForeColor = System.Drawing.Color.White;
            this.btnCalismaStatuGuncelle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_update_50;
            this.btnCalismaStatuGuncelle.Location = new System.Drawing.Point(481, 10);
            this.btnCalismaStatuGuncelle.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.btnCalismaStatuGuncelle.Name = "btnCalismaStatuGuncelle";
            this.btnCalismaStatuGuncelle.Padding = new System.Windows.Forms.Padding(5, 0, 15, 0);
            this.btnCalismaStatuGuncelle.Size = new System.Drawing.Size(227, 92);
            this.btnCalismaStatuGuncelle.TabIndex = 0;
            this.btnCalismaStatuGuncelle.Text = "Güncelle";
            this.btnCalismaStatuGuncelle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCalismaStatuGuncelle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCalismaStatuGuncelle.UseVisualStyleBackColor = false;
            // 
            // btnCalismaStatuSil
            // 
            this.btnCalismaStatuSil.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnCalismaStatuSil.FlatAppearance.BorderSize = 0;
            this.btnCalismaStatuSil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCalismaStatuSil.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCalismaStatuSil.ForeColor = System.Drawing.Color.White;
            this.btnCalismaStatuSil.Image = global::CeyPASS.WFA.Properties.Resources.icons8_minus_50;
            this.btnCalismaStatuSil.Location = new System.Drawing.Point(305, 10);
            this.btnCalismaStatuSil.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.btnCalismaStatuSil.Name = "btnCalismaStatuSil";
            this.btnCalismaStatuSil.Padding = new System.Windows.Forms.Padding(5, 0, 20, 0);
            this.btnCalismaStatuSil.Size = new System.Drawing.Size(166, 92);
            this.btnCalismaStatuSil.TabIndex = 1;
            this.btnCalismaStatuSil.Text = "Sil";
            this.btnCalismaStatuSil.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCalismaStatuSil.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCalismaStatuSil.UseVisualStyleBackColor = false;
            // 
            // btnCalismaStatuEkle
            // 
            this.btnCalismaStatuEkle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnCalismaStatuEkle.FlatAppearance.BorderSize = 0;
            this.btnCalismaStatuEkle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCalismaStatuEkle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCalismaStatuEkle.ForeColor = System.Drawing.Color.White;
            this.btnCalismaStatuEkle.Image = global::CeyPASS.WFA.Properties.Resources.icons8_add_50;
            this.btnCalismaStatuEkle.Location = new System.Drawing.Point(123, 10);
            this.btnCalismaStatuEkle.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.btnCalismaStatuEkle.Name = "btnCalismaStatuEkle";
            this.btnCalismaStatuEkle.Padding = new System.Windows.Forms.Padding(5, 0, 20, 0);
            this.btnCalismaStatuEkle.Size = new System.Drawing.Size(172, 92);
            this.btnCalismaStatuEkle.TabIndex = 2;
            this.btnCalismaStatuEkle.Text = "Ekle";
            this.btnCalismaStatuEkle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCalismaStatuEkle.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCalismaStatuEkle.UseVisualStyleBackColor = false;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblHeader.Size = new System.Drawing.Size(733, 75);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Çalışma Statüsü Bilgileri";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Controls.Add(this.chkCalismaStatuleri);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(10, 10);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(1);
            this.pnlLeft.Size = new System.Drawing.Size(300, 591);
            this.pnlLeft.TabIndex = 2;
            // 
            // chkCalismaStatuleri
            // 
            this.chkCalismaStatuleri.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkCalismaStatuleri.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkCalismaStatuleri.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.chkCalismaStatuleri.FormattingEnabled = true;
            this.chkCalismaStatuleri.HorizontalScrollbar = true;
            this.chkCalismaStatuleri.Location = new System.Drawing.Point(1, 51);
            this.chkCalismaStatuleri.Name = "chkCalismaStatuleri";
            this.chkCalismaStatuleri.Size = new System.Drawing.Size(298, 539);
            this.chkCalismaStatuleri.TabIndex = 1;
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
            this.lblListHeader.Text = "Statü Listesi";
            this.lblListHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ucCalismaStatuleri
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "ucCalismaStatuleri";
            this.Size = new System.Drawing.Size(1073, 611);
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
        private System.Windows.Forms.CheckedListBox chkCalismaStatuleri;

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.FlowLayoutPanel pnlToolbar;
        private System.Windows.Forms.TableLayoutPanel tlpForm;
        private System.Windows.Forms.Panel pnlFooter;

        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtCalismaStatuId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCalismaStatuAdi;

        private System.Windows.Forms.Button btnCalismaStatuEkle;
        private System.Windows.Forms.Button btnCalismaStatuSil;
        private System.Windows.Forms.Button btnCalismaStatuGuncelle;

        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Button btnVazgec;
    }
}
