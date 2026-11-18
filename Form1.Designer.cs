namespace FaceRecognition
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null!;

        // ─── Controls ─────────────────────────────────────────────────────
        private System.Windows.Forms.PictureBox pictureBox = null!;
        private System.Windows.Forms.Button btnStartCamera = null!;
        private System.Windows.Forms.Button btnStopCamera = null!;
        private System.Windows.Forms.Button btnSelectImage = null!;
        private System.Windows.Forms.Button btnTrainModel = null!;
        private System.Windows.Forms.Label statusLabel = null!;
        private System.Windows.Forms.Panel panelButtons = null!;
        private System.Windows.Forms.Label lblTitle = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.SuspendLayout();

            // ── Form ──────────────────────────────────────────────────────
            this.Text = "Yüz Algılama ve Tanıma — OpenCV";
            this.Size = new System.Drawing.Size(930, 680);
            this.MinimumSize = new System.Drawing.Size(880, 520);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.Font = new System.Drawing.Font("Segoe UI", 9.5f);

            // ── Title ─────────────────────────────────────────────────────
            lblTitle = new System.Windows.Forms.Label
            {
                Text = "🎯  Yüz Algılama ve Tanıma",
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 42,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI Semibold", 14f),
                BackColor = System.Drawing.Color.FromArgb(20, 20, 20),
                ForeColor = System.Drawing.Color.LimeGreen
            };

            // ── PictureBox ────────────────────────────────────────────────
            pictureBox = new System.Windows.Forms.PictureBox
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom,
                BackColor = System.Drawing.Color.FromArgb(10, 10, 10),
                BorderStyle = System.Windows.Forms.BorderStyle.None
            };

            // ── Button panel ──────────────────────────────────────────────
            panelButtons = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Height = 70,
                BackColor = System.Drawing.Color.FromArgb(20, 20, 20),
                Padding = new System.Windows.Forms.Padding(10, 12, 10, 12)
            };

            System.Drawing.Color btnBack  = System.Drawing.Color.FromArgb(0, 140, 0);
            System.Drawing.Color btnStop  = System.Drawing.Color.FromArgb(180, 30, 30);
            System.Drawing.Color btnSel   = System.Drawing.Color.FromArgb(30, 100, 180);
            System.Drawing.Color btnTrain = System.Drawing.Color.FromArgb(110, 50, 170);
            System.Drawing.Font btnFont   = new System.Drawing.Font("Segoe UI Semibold", 9.5f);

            btnStartCamera = MakeButton("▶  Kamera Başlat", btnBack,  btnFont, 0);
            btnStopCamera  = MakeButton("■  Kamera Durdur",  btnStop,  btnFont, 1);
            btnSelectImage = MakeButton("🖼  Görüntü Seç",   btnSel,   btnFont, 2);
            btnTrainModel  = MakeButton("🔄  Modeli Eğit",   btnTrain, btnFont, 3);

            btnStopCamera.Enabled = false;

            btnStartCamera.Click += btnStartCamera_Click;
            btnStopCamera.Click  += btnStopCamera_Click;
            btnSelectImage.Click += btnSelectImage_Click;
            btnTrainModel.Click  += btnTrainModel_Click;

            panelButtons.Controls.Add(btnStartCamera);
            panelButtons.Controls.Add(btnStopCamera);
            panelButtons.Controls.Add(btnSelectImage);
            panelButtons.Controls.Add(btnTrainModel);

            // ── Status bar ────────────────────────────────────────────────
            statusLabel = new System.Windows.Forms.Label
            {
                Text = "Durum: Hazır.",
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Height = 26,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                BackColor = System.Drawing.Color.FromArgb(10, 10, 10),
                ForeColor = System.Drawing.Color.Silver,
                Padding = new System.Windows.Forms.Padding(6, 0, 0, 0),
                Font = new System.Drawing.Font("Segoe UI", 8.5f)
            };

            // ── Add to form ───────────────────────────────────────────────
            this.Controls.Add(pictureBox);
            this.Controls.Add(panelButtons);
            this.Controls.Add(statusLabel);
            this.Controls.Add(lblTitle);

            this.ResumeLayout(false);
        }

        private static System.Windows.Forms.Button MakeButton(
            string text,
            System.Drawing.Color back,
            System.Drawing.Font font,
            int index)
        {
            var b = new System.Windows.Forms.Button
            {
                Text = text,
                BackColor = back,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                Font = font,
                Size = new System.Drawing.Size(210, 44),
                Location = new System.Drawing.Point(10 + index * 220, 13),
                Cursor = System.Windows.Forms.Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
