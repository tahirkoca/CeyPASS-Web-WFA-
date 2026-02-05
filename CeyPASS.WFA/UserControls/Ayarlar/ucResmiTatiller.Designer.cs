namespace CeyPASS.WFA.UserControls.Ayarlar
{
    partial class ucResmiTatiller
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
            this.pnlCardCustom = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.btnDigerResmiTatilleriAktar = new System.Windows.Forms.Button();
            this.tlpCustomInputs = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.dtpEklenecekTarih = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.txtEklenecekResmiTatil = new System.Windows.Forms.MaskedTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.nudCalismaSaati = new System.Windows.Forms.NumericUpDown();
            this.lblTitleCustom = new System.Windows.Forms.Label();
            this.pnlCardFixed = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSabitResmiTatilleriAktar = new System.Windows.Forms.Button();
            this.tlpFixedInputs = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBaslangicYili = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBitisYili = new System.Windows.Forms.MaskedTextBox();
            this.lblTitleFixed = new System.Windows.Forms.Label();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.lstTatiller = new System.Windows.Forms.ListBox();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.lblListHeader = new System.Windows.Forms.Label();
            this.pnlMain.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlCardCustom.SuspendLayout();
            this.tlpCustomInputs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCalismaSaati)).BeginInit();
            this.pnlCardFixed.SuspendLayout();
            this.tlpFixedInputs.SuspendLayout();
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
            this.pnlMain.Size = new System.Drawing.Size(1219, 700);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlRight
            // 
            this.pnlRight.AutoScroll = true;
            this.pnlRight.Controls.Add(this.pnlCardCustom);
            this.pnlRight.Controls.Add(this.pnlCardFixed);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(490, 10); // Sol panel büyüdüğü için konumu kaydırdık
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.pnlRight.Size = new System.Drawing.Size(719, 680);
            this.pnlRight.TabIndex = 1;
            // 
            // pnlCardCustom
            // 
            this.pnlCardCustom.BackColor = System.Drawing.Color.White;
            this.pnlCardCustom.Controls.Add(this.label7);
            this.pnlCardCustom.Controls.Add(this.btnDigerResmiTatilleriAktar);
            this.pnlCardCustom.Controls.Add(this.tlpCustomInputs);
            this.pnlCardCustom.Controls.Add(this.lblTitleCustom);
            this.pnlCardCustom.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCardCustom.Location = new System.Drawing.Point(20, 320);
            this.pnlCardCustom.Name = "pnlCardCustom";
            this.pnlCardCustom.Padding = new System.Windows.Forms.Padding(20);
            this.pnlCardCustom.Size = new System.Drawing.Size(699, 340);
            this.pnlCardCustom.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Top;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.label7.ForeColor = System.Drawing.Color.Gray;
            this.label7.Location = new System.Drawing.Point(20, 268);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(659, 45);
            this.label7.TabIndex = 3;
            this.label7.Text = "* Dini bayramlar ve özel günler için kullanılır. Çalışma saati girerek yarım gün " +
    "tatilleri tanımlayabilirsiniz.";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnDigerResmiTatilleriAktar
            // 
            this.btnDigerResmiTatilleriAktar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnDigerResmiTatilleriAktar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDigerResmiTatilleriAktar.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnDigerResmiTatilleriAktar.FlatAppearance.BorderSize = 0;
            this.btnDigerResmiTatilleriAktar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDigerResmiTatilleriAktar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnDigerResmiTatilleriAktar.ForeColor = System.Drawing.Color.White;
            this.btnDigerResmiTatilleriAktar.Location = new System.Drawing.Point(20, 218);
            this.btnDigerResmiTatilleriAktar.Name = "btnDigerResmiTatilleriAktar";
            this.btnDigerResmiTatilleriAktar.Size = new System.Drawing.Size(659, 50);
            this.btnDigerResmiTatilleriAktar.TabIndex = 2;
            this.btnDigerResmiTatilleriAktar.Text = "Listeye Ekle";
            this.btnDigerResmiTatilleriAktar.UseVisualStyleBackColor = false;
            this.btnDigerResmiTatilleriAktar.Click += new System.EventHandler(this.btnDigerResmiTatilleriAktar_Click);
            // 
            // tlpCustomInputs
            // 
            this.tlpCustomInputs.ColumnCount = 2;
            this.tlpCustomInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpCustomInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tlpCustomInputs.Controls.Add(this.label6, 0, 0);
            this.tlpCustomInputs.Controls.Add(this.dtpEklenecekTarih, 1, 0);
            this.tlpCustomInputs.Controls.Add(this.label5, 0, 1);
            this.tlpCustomInputs.Controls.Add(this.txtEklenecekResmiTatil, 1, 1);
            this.tlpCustomInputs.Controls.Add(this.label3, 0, 2);
            this.tlpCustomInputs.Controls.Add(this.nudCalismaSaati, 1, 2);
            this.tlpCustomInputs.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpCustomInputs.Location = new System.Drawing.Point(20, 58);
            this.tlpCustomInputs.Name = "tlpCustomInputs";
            this.tlpCustomInputs.RowCount = 3;
            this.tlpCustomInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpCustomInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpCustomInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tlpCustomInputs.Size = new System.Drawing.Size(659, 160);
            this.tlpCustomInputs.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label6.Location = new System.Drawing.Point(3, 13);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 23);
            this.label6.TabIndex = 0;
            this.label6.Text = "Tarih:";
            // 
            // dtpEklenecekTarih
            // 
            this.dtpEklenecekTarih.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpEklenecekTarih.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpEklenecekTarih.Location = new System.Drawing.Point(200, 10);
            this.dtpEklenecekTarih.Name = "dtpEklenecekTarih";
            this.dtpEklenecekTarih.Size = new System.Drawing.Size(456, 30);
            this.dtpEklenecekTarih.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label5.Location = new System.Drawing.Point(3, 63);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 23);
            this.label5.TabIndex = 2;
            this.label5.Text = "Tatil Adı:";
            // 
            // txtEklenecekResmiTatil
            // 
            this.txtEklenecekResmiTatil.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEklenecekResmiTatil.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtEklenecekResmiTatil.Location = new System.Drawing.Point(200, 60);
            this.txtEklenecekResmiTatil.Name = "txtEklenecekResmiTatil";
            this.txtEklenecekResmiTatil.Size = new System.Drawing.Size(456, 30);
            this.txtEklenecekResmiTatil.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label3.Location = new System.Drawing.Point(3, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 23);
            this.label3.TabIndex = 4;
            this.label3.Text = "Çalışma Saati:";
            // 
            // nudCalismaSaati
            // 
            this.nudCalismaSaati.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.nudCalismaSaati.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.nudCalismaSaati.Location = new System.Drawing.Point(200, 115);
            this.nudCalismaSaati.Name = "nudCalismaSaati";
            this.nudCalismaSaati.Size = new System.Drawing.Size(456, 30);
            this.nudCalismaSaati.TabIndex = 5;
            // 
            // lblTitleCustom
            // 
            this.lblTitleCustom.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitleCustom.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitleCustom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblTitleCustom.Location = new System.Drawing.Point(20, 20);
            this.lblTitleCustom.Name = "lblTitleCustom";
            this.lblTitleCustom.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblTitleCustom.Size = new System.Drawing.Size(659, 38);
            this.lblTitleCustom.TabIndex = 0;
            this.lblTitleCustom.Text = "Diğer Resmi Tatil Ekleme";
            // 
            // pnlCardFixed
            // 
            this.pnlCardFixed.BackColor = System.Drawing.Color.White;
            this.pnlCardFixed.Controls.Add(this.label4);
            this.pnlCardFixed.Controls.Add(this.btnSabitResmiTatilleriAktar);
            this.pnlCardFixed.Controls.Add(this.tlpFixedInputs);
            this.pnlCardFixed.Controls.Add(this.lblTitleFixed);
            this.pnlCardFixed.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCardFixed.Location = new System.Drawing.Point(20, 0);
            this.pnlCardFixed.Name = "pnlCardFixed";
            this.pnlCardFixed.Padding = new System.Windows.Forms.Padding(20);
            this.pnlCardFixed.Size = new System.Drawing.Size(699, 300);
            this.pnlCardFixed.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Top;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.label4.ForeColor = System.Drawing.Color.Gray;
            this.label4.Location = new System.Drawing.Point(20, 218);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(659, 45);
            this.label4.TabIndex = 3;
            this.label4.Text = "* Bu kısım her yıl tekrar eden milli bayramların (23 Nisan, 19 Mayıs vb.) verilen" +
    " yıl aralığına göre otomatik eklenmesini sağlar.";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnSabitResmiTatilleriAktar
            // 
            this.btnSabitResmiTatilleriAktar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnSabitResmiTatilleriAktar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSabitResmiTatilleriAktar.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSabitResmiTatilleriAktar.FlatAppearance.BorderSize = 0;
            this.btnSabitResmiTatilleriAktar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSabitResmiTatilleriAktar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnSabitResmiTatilleriAktar.ForeColor = System.Drawing.Color.White;
            this.btnSabitResmiTatilleriAktar.Location = new System.Drawing.Point(20, 168);
            this.btnSabitResmiTatilleriAktar.Name = "btnSabitResmiTatilleriAktar";
            this.btnSabitResmiTatilleriAktar.Size = new System.Drawing.Size(659, 50);
            this.btnSabitResmiTatilleriAktar.TabIndex = 2;
            this.btnSabitResmiTatilleriAktar.Text = "Sabit Tatilleri Oluştur ve Aktar";
            this.btnSabitResmiTatilleriAktar.UseVisualStyleBackColor = false;
            this.btnSabitResmiTatilleriAktar.Click += new System.EventHandler(this.btnSabitResmiTatilleriAktar_Click);
            // 
            // tlpFixedInputs
            // 
            this.tlpFixedInputs.ColumnCount = 2;
            this.tlpFixedInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpFixedInputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tlpFixedInputs.Controls.Add(this.label1, 0, 0);
            this.tlpFixedInputs.Controls.Add(this.txtBaslangicYili, 1, 0);
            this.tlpFixedInputs.Controls.Add(this.label2, 0, 1);
            this.tlpFixedInputs.Controls.Add(this.txtBitisYili, 1, 1);
            this.tlpFixedInputs.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpFixedInputs.Location = new System.Drawing.Point(20, 58);
            this.tlpFixedInputs.Name = "tlpFixedInputs";
            this.tlpFixedInputs.RowCount = 2;
            this.tlpFixedInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpFixedInputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tlpFixedInputs.Size = new System.Drawing.Size(659, 110);
            this.tlpFixedInputs.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Başlangıç Yılı:";
            // 
            // txtBaslangicYili
            // 
            this.txtBaslangicYili.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBaslangicYili.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBaslangicYili.Location = new System.Drawing.Point(200, 12);
            this.txtBaslangicYili.Mask = "0000";
            this.txtBaslangicYili.Name = "txtBaslangicYili";
            this.txtBaslangicYili.Size = new System.Drawing.Size(456, 30);
            this.txtBaslangicYili.TabIndex = 1;
            this.txtBaslangicYili.ValidatingType = typeof(int);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label2.Location = new System.Drawing.Point(3, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Bitiş Yılı:";
            // 
            // txtBitisYili
            // 
            this.txtBitisYili.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBitisYili.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBitisYili.Location = new System.Drawing.Point(200, 67);
            this.txtBitisYili.Mask = "0000";
            this.txtBitisYili.Name = "txtBitisYili";
            this.txtBitisYili.Size = new System.Drawing.Size(456, 30);
            this.txtBitisYili.TabIndex = 3;
            this.txtBitisYili.ValidatingType = typeof(int);
            // 
            // lblTitleFixed
            // 
            this.lblTitleFixed.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitleFixed.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitleFixed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(37)))), ((int)(((byte)(41)))));
            this.lblTitleFixed.Location = new System.Drawing.Point(20, 20);
            this.lblTitleFixed.Name = "lblTitleFixed";
            this.lblTitleFixed.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblTitleFixed.Size = new System.Drawing.Size(659, 38);
            this.lblTitleFixed.TabIndex = 0;
            this.lblTitleFixed.Text = "Yıllık Sabit Resmi Tatil Doldurma";
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.White;
            this.pnlLeft.Controls.Add(this.lstTatiller);
            this.pnlLeft.Controls.Add(this.pnlLeftHeader);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(10, 10);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(1);
            this.pnlLeft.Size = new System.Drawing.Size(480, 680); // BURAYI 480 YAPTIM
            this.pnlLeft.TabIndex = 0;
            // 
            // lstTatiller
            // 
            this.lstTatiller.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstTatiller.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTatiller.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lstTatiller.FormattingEnabled = true;
            this.lstTatiller.HorizontalScrollbar = true; // SCROLL AÇILDI
            this.lstTatiller.ItemHeight = 23;
            this.lstTatiller.Location = new System.Drawing.Point(1, 51);
            this.lstTatiller.Name = "lstTatiller";
            this.lstTatiller.Size = new System.Drawing.Size(478, 628);
            this.lstTatiller.TabIndex = 1;
            // 
            // pnlLeftHeader
            // 
            this.pnlLeftHeader.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlLeftHeader.Controls.Add(this.lblListHeader);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLeftHeader.Location = new System.Drawing.Point(1, 1);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(478, 50);
            this.pnlLeftHeader.TabIndex = 0;
            // 
            // lblListHeader
            // 
            this.lblListHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblListHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblListHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblListHeader.Location = new System.Drawing.Point(0, 0);
            this.lblListHeader.Name = "lblListHeader";
            this.lblListHeader.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblListHeader.Size = new System.Drawing.Size(478, 50);
            this.lblListHeader.TabIndex = 0;
            this.lblListHeader.Text = "Tanımlı Tatiller";
            this.lblListHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ucResmiTatiller
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Name = "ucResmiTatiller";
            this.Size = new System.Drawing.Size(1219, 700);
            this.pnlMain.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.pnlCardCustom.ResumeLayout(false);
            this.tlpCustomInputs.ResumeLayout(false);
            this.tlpCustomInputs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCalismaSaati)).EndInit();
            this.pnlCardFixed.ResumeLayout(false);
            this.tlpFixedInputs.ResumeLayout(false);
            this.tlpFixedInputs.PerformLayout();
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeftHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlLeftHeader;
        private System.Windows.Forms.Label lblListHeader;
        private System.Windows.Forms.ListBox lstTatiller;
        private System.Windows.Forms.Panel pnlCardFixed;
        private System.Windows.Forms.Label lblTitleFixed;
        private System.Windows.Forms.TableLayoutPanel tlpFixedInputs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MaskedTextBox txtBaslangicYili;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox txtBitisYili;
        private System.Windows.Forms.Button btnSabitResmiTatilleriAktar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pnlCardCustom;
        private System.Windows.Forms.Label lblTitleCustom;
        private System.Windows.Forms.TableLayoutPanel tlpCustomInputs;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker dtpEklenecekTarih;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox txtEklenecekResmiTatil;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudCalismaSaati;
        private System.Windows.Forms.Button btnDigerResmiTatilleriAktar;
        private System.Windows.Forms.Label label7;
    }
}
