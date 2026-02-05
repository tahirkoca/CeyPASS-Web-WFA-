namespace CeyPASS.WFA.Forms
{
    partial class canliIzlemeVeriEkrani
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(canliIzlemeVeriEkrani));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flpSonGecenler = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.dgSonHareketler = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lblSeciliUnvan = new System.Windows.Forms.Label();
            this.lblSeciliDepartman = new System.Windows.Forms.Label();
            this.lblSeciliAdSoyad = new System.Windows.Forms.Label();
            this.pbSeciliFoto = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.atananKartiGuncelle = new System.Windows.Forms.Button();
            this.kisiyeKartiAta = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgSonHareketler)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSeciliFoto)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flpSonGecenler, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 51.14822F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 43.4238F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.427975F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1782, 958);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flpSonGecenler
            // 
            this.flpSonGecenler.AutoScroll = true;
            this.flpSonGecenler.BackColor = System.Drawing.Color.White;
            this.flpSonGecenler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpSonGecenler.Location = new System.Drawing.Point(3, 3);
            this.flpSonGecenler.Name = "flpSonGecenler";
            this.flpSonGecenler.Size = new System.Drawing.Size(1776, 483);
            this.flpSonGecenler.TabIndex = 1;
            this.flpSonGecenler.WrapContents = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.14865F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.85135F));
            this.tableLayoutPanel2.Controls.Add(this.dgSonHareketler, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 492);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 410F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1776, 410);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // dgSonHareketler
            // 
            this.dgSonHareketler.AllowUserToAddRows = false;
            this.dgSonHareketler.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgSonHareketler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgSonHareketler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgSonHareketler.Location = new System.Drawing.Point(3, 3);
            this.dgSonHareketler.Name = "dgSonHareketler";
            this.dgSonHareketler.ReadOnly = true;
            this.dgSonHareketler.RowHeadersWidth = 51;
            this.dgSonHareketler.RowTemplate.Height = 24;
            this.dgSonHareketler.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgSonHareketler.Size = new System.Drawing.Size(1080, 404);
            this.dgSonHareketler.TabIndex = 2;
            this.dgSonHareketler.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgSonHareketler_CellClick);
            this.dgSonHareketler.SelectionChanged += new System.EventHandler(this.dgSonHareketler_SelectionChanged);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Controls.Add(this.lblSeciliUnvan, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.lblSeciliDepartman, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.lblSeciliAdSoyad, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.pbSeciliFoto, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(1089, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60.80402F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.06533F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.31156F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.8191F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(684, 404);
            this.tableLayoutPanel3.TabIndex = 3;
            // 
            // lblSeciliUnvan
            // 
            this.lblSeciliUnvan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSeciliUnvan.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblSeciliUnvan.Location = new System.Drawing.Point(3, 365);
            this.lblSeciliUnvan.Name = "lblSeciliUnvan";
            this.lblSeciliUnvan.Size = new System.Drawing.Size(678, 20);
            this.lblSeciliUnvan.TabIndex = 6;
            this.lblSeciliUnvan.Text = "Unvan";
            this.lblSeciliUnvan.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSeciliDepartman
            // 
            this.lblSeciliDepartman.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSeciliDepartman.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblSeciliDepartman.Location = new System.Drawing.Point(3, 311);
            this.lblSeciliDepartman.Name = "lblSeciliDepartman";
            this.lblSeciliDepartman.Size = new System.Drawing.Size(678, 20);
            this.lblSeciliDepartman.TabIndex = 5;
            this.lblSeciliDepartman.Text = "Departman";
            this.lblSeciliDepartman.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSeciliAdSoyad
            // 
            this.lblSeciliAdSoyad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSeciliAdSoyad.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblSeciliAdSoyad.Location = new System.Drawing.Point(3, 256);
            this.lblSeciliAdSoyad.Name = "lblSeciliAdSoyad";
            this.lblSeciliAdSoyad.Size = new System.Drawing.Size(678, 30);
            this.lblSeciliAdSoyad.TabIndex = 4;
            this.lblSeciliAdSoyad.Text = "Ad Soyad";
            this.lblSeciliAdSoyad.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbSeciliFoto
            // 
            this.pbSeciliFoto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pbSeciliFoto.Location = new System.Drawing.Point(3, 3);
            this.pbSeciliFoto.Name = "pbSeciliFoto";
            this.pbSeciliFoto.Size = new System.Drawing.Size(678, 239);
            this.pbSeciliFoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbSeciliFoto.TabIndex = 3;
            this.pbSeciliFoto.TabStop = false;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 10;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.747318F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.60071F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.324195F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.25626F));
            this.tableLayoutPanel4.Controls.Add(this.atananKartiGuncelle, 7, 0);
            this.tableLayoutPanel4.Controls.Add(this.kisiyeKartiAta, 9, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 908);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(1776, 47);
            this.tableLayoutPanel4.TabIndex = 3;
            // 
            // atananKartiGuncelle
            // 
            this.atananKartiGuncelle.BackColor = System.Drawing.Color.LightGray;
            this.atananKartiGuncelle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.atananKartiGuncelle.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.atananKartiGuncelle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.atananKartiGuncelle.ForeColor = System.Drawing.Color.Black;
            this.atananKartiGuncelle.Location = new System.Drawing.Point(1199, 0);
            this.atananKartiGuncelle.Margin = new System.Windows.Forms.Padding(0);
            this.atananKartiGuncelle.Name = "atananKartiGuncelle";
            this.atananKartiGuncelle.Size = new System.Drawing.Size(259, 47);
            this.atananKartiGuncelle.TabIndex = 48;
            this.atananKartiGuncelle.Text = "Misafire Verilen Kartı Güncelle";
            this.atananKartiGuncelle.UseVisualStyleBackColor = false;
            this.atananKartiGuncelle.Click += new System.EventHandler(this.atananKartiGuncelle_Click);
            // 
            // kisiyeKartiAta
            // 
            this.kisiyeKartiAta.BackColor = System.Drawing.Color.LightGray;
            this.kisiyeKartiAta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kisiyeKartiAta.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.kisiyeKartiAta.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.kisiyeKartiAta.ForeColor = System.Drawing.Color.Black;
            this.kisiyeKartiAta.Location = new System.Drawing.Point(1499, 0);
            this.kisiyeKartiAta.Margin = new System.Windows.Forms.Padding(0);
            this.kisiyeKartiAta.Name = "kisiyeKartiAta";
            this.kisiyeKartiAta.Size = new System.Drawing.Size(277, 47);
            this.kisiyeKartiAta.TabIndex = 40;
            this.kisiyeKartiAta.Text = "Misafire Kart Ver";
            this.kisiyeKartiAta.UseVisualStyleBackColor = false;
            this.kisiyeKartiAta.Click += new System.EventHandler(this.kisiyeKartiAta_Click);
            // 
            // canliIzlemeVeriEkrani
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1782, 958);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "canliIzlemeVeriEkrani";
            this.Text = "Canlı İzleme Ekranı";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.canliIzlemeVeriEkrani_FormClosing);
            this.Load += new System.EventHandler(this.canliIzlemeVeriEkrani_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgSonHareketler)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbSeciliFoto)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flpSonGecenler;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DataGridView dgSonHareketler;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.PictureBox pbSeciliFoto;
        private System.Windows.Forms.Label lblSeciliUnvan;
        private System.Windows.Forms.Label lblSeciliDepartman;
        private System.Windows.Forms.Label lblSeciliAdSoyad;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button kisiyeKartiAta;
        private System.Windows.Forms.Button atananKartiGuncelle;
    }
}