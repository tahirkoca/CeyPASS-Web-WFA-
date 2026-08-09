namespace CeyPASS.WFA.UserControls.Canlı_İzleme
{
    partial class gecmisZiyaretciPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblBaslik = new System.Windows.Forms.Label();
            this.txtAra = new System.Windows.Forms.TextBox();
            this.lstGecmis = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lblBaslik
            // 
            this.lblBaslik.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBaslik.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblBaslik.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblBaslik.Location = new System.Drawing.Point(8, 8);
            this.lblBaslik.Name = "lblBaslik";
            this.lblBaslik.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblBaslik.Size = new System.Drawing.Size(344, 24);
            this.lblBaslik.TabIndex = 0;
            this.lblBaslik.Text = "Geçmiş Ziyaretçiler";
            // 
            // txtAra
            // 
            this.txtAra.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAra.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtAra.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtAra.Location = new System.Drawing.Point(8, 32);
            this.txtAra.Name = "txtAra";
            this.txtAra.PlaceholderText = "İsim ara...";
            this.txtAra.Size = new System.Drawing.Size(344, 30);
            this.txtAra.TabIndex = 1;
            // 
            // lstGecmis
            // 
            this.lstGecmis.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstGecmis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstGecmis.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lstGecmis.FormattingEnabled = true;
            this.lstGecmis.IntegralHeight = false;
            this.lstGecmis.ItemHeight = 24;
            this.lstGecmis.Location = new System.Drawing.Point(8, 62);
            this.lstGecmis.Name = "lstGecmis";
            this.lstGecmis.Size = new System.Drawing.Size(344, 510);
            this.lstGecmis.TabIndex = 2;
            // 
            // gecmisZiyaretciPanel
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.Controls.Add(this.lstGecmis);
            this.Controls.Add(this.txtAra);
            this.Controls.Add(this.lblBaslik);
            this.Dock = System.Windows.Forms.DockStyle.Left;
            this.Name = "gecmisZiyaretciPanel";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(360, 580);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblBaslik;
        private System.Windows.Forms.TextBox txtAra;
        private System.Windows.Forms.ListBox lstGecmis;
    }
}
