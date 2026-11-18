using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Face;

namespace FaceRecognition
{
    public partial class Form1 : Form
    {
        // ─── OpenCV objects ────────────────────────────────────────────────
        private VideoCapture? _capture;
        private CascadeClassifier? _faceCascade;
        private LBPHFaceRecognizer? _recognizer;
        private Thread? _cameraThread;
        private volatile bool _cameraRunning;

        // ─── Training data ─────────────────────────────────────────────────
        private readonly Dictionary<int, string> _labelNames = new();
        private bool _recognizerReady;

        // ─── Paths ─────────────────────────────────────────────────────────
        private static readonly string CascadePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_frontalface_default.xml");
        private static readonly string TrainingDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrainingData");
        private static readonly string ModelPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "face_model.yml");

        private DateTime _lastStatusUpdate = DateTime.MinValue;

        public Form1()
        {
            InitializeComponent();
            LoadCascade();
            TrainRecognizer();
        }

        // ══════════════════════════════════════════════════════════════════
        //  Initialisation helpers
        // ══════════════════════════════════════════════════════════════════

        private void LoadCascade()
        {
            if (!File.Exists(CascadePath))
            {
                ShowError($"Haar cascade dosyası bulunamadı:\n{CascadePath}\n\nLütfen 'haarcascade_frontalface_default.xml' dosyasını kontrol edin.");
                return;
            }
            _faceCascade = new CascadeClassifier(CascadePath);
            SetStatus("Yüz algılama modeli yüklendi.");
        }

        /// <summary>
        /// Scans TrainingData\PersonName\ folders.
        /// Crops the face from each training photo and trains the LBPH model.
        /// </summary>
        private void TrainRecognizer()
        {
            if (!Directory.Exists(TrainingDir))
            {
                _recognizerReady = false;
                SetStatus("Eğitim klasörü bulunamadı: " + TrainingDir);
                return;
            }

            var images = new List<Mat>();
            var labels = new List<int>();
            _labelNames.Clear();
            int labelId = 0;
            int totalFacesFound = 0;

            foreach (string personDir in Directory.GetDirectories(TrainingDir))
            {
                string personName = Path.GetFileName(personDir);
                int personFaceCount = 0;

                var allFiles = new List<string>();
                allFiles.AddRange(Directory.GetFiles(personDir, "*.jpg", SearchOption.TopDirectoryOnly));
                allFiles.AddRange(Directory.GetFiles(personDir, "*.jpeg", SearchOption.TopDirectoryOnly));
                allFiles.AddRange(Directory.GetFiles(personDir, "*.png", SearchOption.TopDirectoryOnly));

                foreach (string imgPath in allFiles)
                {
                    if (Path.GetFileName(imgPath).StartsWith("Screenshot", StringComparison.OrdinalIgnoreCase))
                        continue;

                    using Mat color = Cv2.ImRead(imgPath);
                    if (color.Empty()) continue;

                    using Mat gray = new();
                    Cv2.CvtColor(color, gray, ColorConversionCodes.BGR2GRAY);
                    Cv2.EqualizeHist(gray, gray);

                    // Detect face in the training photo
                    Rect[] detectedFaces = _faceCascade != null
                        ? _faceCascade.DetectMultiScale(gray, scaleFactor: 1.1, minNeighbors: 3, flags: HaarDetectionTypes.ScaleImage, minSize: new OpenCvSharp.Size(40, 40))
                        : Array.Empty<Rect>();

                    if (detectedFaces.Length == 0 && _faceCascade != null)
                    {
                        // Try looser threshold if needed
                        detectedFaces = _faceCascade.DetectMultiScale(gray, scaleFactor: 1.05, minNeighbors: 2, flags: HaarDetectionTypes.ScaleImage, minSize: new OpenCvSharp.Size(30, 30));
                    }

                    if (detectedFaces.Length > 0)
                    {
                        // Extract the main face (largest rectangle)
                        Rect bestFace = detectedFaces.OrderByDescending(r => r.Width * r.Height).First();
                        using Mat faceCrop = new(gray, bestFace);
                        using Mat resized = new();
                        Cv2.Resize(faceCrop, resized, new OpenCvSharp.Size(100, 100));
                        Cv2.EqualizeHist(resized, resized);

                        images.Add(resized.Clone());
                        labels.Add(labelId);
                        personFaceCount++;
                        totalFacesFound++;

                        // Add horizontally flipped sample for data augmentation
                        using Mat flipped = new();
                        Cv2.Flip(resized, flipped, FlipMode.Y);
                        images.Add(flipped.Clone());
                        labels.Add(labelId);
                        personFaceCount++;
                        totalFacesFound++;
                    }
                    else if (gray.Width <= 450 && gray.Height <= 450)
                    {
                        // Image is already a cropped face
                        using Mat resized = new();
                        Cv2.Resize(gray, resized, new OpenCvSharp.Size(100, 100));
                        Cv2.EqualizeHist(resized, resized);

                        images.Add(resized.Clone());
                        labels.Add(labelId);
                        personFaceCount++;
                        totalFacesFound++;
                    }
                }

                if (personFaceCount > 0)
                {
                    _labelNames[labelId] = personName;
                    labelId++;
                }
            }

            if (images.Count == 0)
            {
                _recognizerReady = false;
                SetStatus("Eğitim görüntülerinde yüz bulunamadı – tanıma devre dışı.");
                return;
            }

            try
            {
                _recognizer?.Dispose();
                _recognizer = LBPHFaceRecognizer.Create(radius: 1, neighbors: 8, gridX: 8, gridY: 8);
                _recognizer.Train(images, labels);
                _recognizer.Save(ModelPath);
                _recognizerReady = true;

                string names = string.Join(", ", _labelNames.Values);
                SetStatus($"Model eğitildi: {_labelNames.Count} kişi ({names}) — {totalFacesFound} yüz örneği.");
            }
            catch (Exception ex)
            {
                _recognizerReady = false;
                ShowError("Model eğitimi sırasında hata: " + ex.Message);
            }
            finally
            {
                foreach (var m in images) m.Dispose();
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  Camera
        // ══════════════════════════════════════════════════════════════════

        private void btnStartCamera_Click(object sender, EventArgs e)
        {
            if (_cameraRunning) return;

            if (_faceCascade == null)
            {
                ShowError("Cascade modeli yüklenemedi. Kamera başlatılamıyor.");
                return;
            }

            try
            {
                _capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
                if (!_capture.IsOpened())
                {
                    _capture.Dispose();
                    _capture = new VideoCapture(0);
                }
            }
            catch
            {
                _capture = new VideoCapture(0);
            }

            if (!_capture.IsOpened())
            {
                _capture?.Dispose();
                _capture = null;
                ShowError("Kamera açılamadı.\nLütfen bir kameranın bağlı ve başka bir uygulama tarafından kullanılmadığından emin olun.");
                return;
            }

            // Set standard resolution for smooth real-time detection
            _capture.Set(VideoCaptureProperties.FrameWidth, 640);
            _capture.Set(VideoCaptureProperties.FrameHeight, 480);

            _cameraRunning = true;
            btnStartCamera.Enabled = false;
            btnStopCamera.Enabled = true;
            btnSelectImage.Enabled = false;
            btnTrainModel.Enabled = false;
            SetStatus("Kamera aktif…");

            _cameraThread = new Thread(CameraLoop) { IsBackground = true };
            _cameraThread.Start();
        }

        private void btnStopCamera_Click(object sender, EventArgs e)
        {
            StopCamera();
        }

        private void StopCamera()
        {
            _cameraRunning = false;
            _cameraThread?.Join(1500);
            _capture?.Dispose();
            _capture = null;

            if (InvokeRequired)
                Invoke(UpdateButtonsAfterStop);
            else
                UpdateButtonsAfterStop();
        }

        private void UpdateButtonsAfterStop()
        {
            btnStartCamera.Enabled = true;
            btnStopCamera.Enabled = false;
            btnSelectImage.Enabled = true;
            btnTrainModel.Enabled = true;
            SetStatus("Kamera durduruldu.");
        }

        private void CameraLoop()
        {
            using Mat frame = new();
            while (_cameraRunning)
            {
                if (_capture == null || !_capture.Read(frame) || frame.Empty())
                {
                    Thread.Sleep(30);
                    continue;
                }

                Mat display = ProcessFrame(frame);
                Bitmap bmp = BitmapConverter.ToBitmap(display);
                display.Dispose();

                if (!_cameraRunning) { bmp.Dispose(); break; }

                pictureBox.Invoke(() =>
                {
                    var old = pictureBox.Image;
                    pictureBox.Image = bmp;
                    old?.Dispose();
                });
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  Image file selection
        // ══════════════════════════════════════════════════════════════════

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            if (_faceCascade == null)
            {
                ShowError("Cascade modeli yüklenemedi.");
                return;
            }

            using OpenFileDialog dlg = new()
            {
                Title = "Görüntü Seç",
                Filter = "Görüntü Dosyaları|*.jpg;*.jpeg;*.png;*.bmp;*.tiff|Tüm Dosyalar|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            if (!File.Exists(dlg.FileName))
            {
                ShowError("Seçilen dosya bulunamadı:\n" + dlg.FileName);
                return;
            }

            using Mat img = Cv2.ImRead(dlg.FileName);
            if (img.Empty())
            {
                ShowError("Görüntü okunamadı:\n" + dlg.FileName);
                return;
            }

            Mat result = ProcessFrame(img);
            var old = pictureBox.Image;
            pictureBox.Image = BitmapConverter.ToBitmap(result);
            result.Dispose();
            old?.Dispose();
            SetStatus($"Görüntü yüklendi: {Path.GetFileName(dlg.FileName)}");
        }

        private void btnTrainModel_Click(object sender, EventArgs e)
        {
            TrainRecognizer();
            MessageBox.Show(statusLabel.Text, "Model Eğitimi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Core: detect + recognise
        // ══════════════════════════════════════════════════════════════════

        private Mat ProcessFrame(Mat input)
        {
            Mat output = input.Clone();

            using Mat gray = new();
            Cv2.CvtColor(input, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(gray, gray);

            // Detect faces
            Rect[] faces = _faceCascade != null
                ? _faceCascade.DetectMultiScale(
                    gray,
                    scaleFactor: 1.15,
                    minNeighbors: 4,
                    flags: HaarDetectionTypes.ScaleImage,
                    minSize: new OpenCvSharp.Size(40, 40))
                : Array.Empty<Rect>();

            int faceCount = faces.Length;
            string? bestRecognizedPerson = null;

            foreach (Rect face in faces)
            {
                string label = "Bilinmiyor";
                string confidence = "";
                bool isRecognized = false;

                // ── Recognition (if model trained) ───────────────────────
                if (_recognizerReady && _recognizer != null)
                {
                    using Mat faceROI = new(gray, face);
                    using Mat resized = new();
                    Cv2.Resize(faceROI, resized, new OpenCvSharp.Size(100, 100));
                    Cv2.EqualizeHist(resized, resized);

                    _recognizer.Predict(resized, out int predictedLabel, out double dist);

                    // LBPH distance: lower is better. Matching faces typically have dist 40..85.
                    if (dist <= 85.0 && _labelNames.TryGetValue(predictedLabel, out string? name))
                    {
                        label = name;
                        bestRecognizedPerson = name;
                        double confScore = Math.Round(Math.Max(50.0, Math.Min(99.0, (1.0 - (dist / 140.0)) * 100.0)), 1);
                        confidence = $" {confScore:F1}%";
                        isRecognized = true;
                    }
                    else if (dist <= 95.0 && _labelNames.TryGetValue(predictedLabel, out string? closeName))
                    {
                        label = closeName;
                        bestRecognizedPerson = closeName;
                        double confScore = Math.Round(Math.Max(40.0, Math.Min(65.0, (1.0 - (dist / 140.0)) * 100.0)), 1);
                        confidence = $" {confScore:F1}%";
                        isRecognized = true;
                    }
                    else
                    {
                        label = "Bilinmiyor";
                        double unknownScore = Math.Round(Math.Max(5.0, (1.0 - (dist / 140.0)) * 100.0), 1);
                        confidence = $" {unknownScore:F1}%";
                    }
                }

                // Green for recognized, Orange for unknown
                Scalar boxColor = isRecognized ? Scalar.LimeGreen : Scalar.Orange;

                // Draw rectangle
                Cv2.Rectangle(output, face, boxColor, 2);

                // Draw label background + text
                string displayText = $"{label}{confidence}";
                OpenCvSharp.Size textSize = Cv2.GetTextSize(
                    displayText, HersheyFonts.HersheySimplex, 0.65, 1, out _);

                int textY = Math.Max(face.Y - 8, textSize.Height + 6);
                Rect bgRect = new(
                    face.X,
                    textY - textSize.Height - 4,
                    textSize.Width + 8,
                    textSize.Height + 6);

                Cv2.Rectangle(output, bgRect, boxColor, -1);
                Cv2.PutText(output, displayText,
                    new OpenCvSharp.Point(face.X + 4, textY),
                    HersheyFonts.HersheySimplex, 0.65, Scalar.Black, 1);
            }

            // Non-blocking throttled status update
            UpdateStatusThrottled(faceCount, bestRecognizedPerson);

            return output;
        }

        private void UpdateStatusThrottled(int faceCount, string? recognized)
        {
            if ((DateTime.Now - _lastStatusUpdate).TotalMilliseconds < 300) return;
            _lastStatusUpdate = DateTime.Now;

            string msg;
            if (faceCount == 0)
                msg = "Yüz tespit edilemedi.";
            else if (!string.IsNullOrEmpty(recognized))
                msg = $"{faceCount} yüz tespit edildi. (Tanınan: {recognized})";
            else
                msg = $"{faceCount} yüz tespit edildi. (Tanınmayan kişi)";

            if (InvokeRequired)
                BeginInvoke(() => SetStatus(msg));
            else
                SetStatus(msg);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Helpers
        // ══════════════════════════════════════════════════════════════════

        private void SetStatus(string msg)
        {
            if (statusLabel.InvokeRequired)
                statusLabel.Invoke(() => statusLabel.Text = "Durum: " + msg);
            else
                statusLabel.Text = "Durum: " + msg;
        }

        private static void ShowError(string msg)
        {
            MessageBox.Show(msg, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopCamera();
            _faceCascade?.Dispose();
            _recognizer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
