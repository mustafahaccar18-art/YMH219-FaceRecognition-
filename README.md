# Yüz Algılama ve Tanıma — OpenCV ile C#

**Ödev 7** — OpenCV tabanlı yüz algılama ve temel yüz tanıma uygulaması.

## Özellikler

| Gereksinim | Durum |
|---|---|
| Kamera görüntüsü PictureBox'ta gösterilir | ✅ |
| Yüz bölgesi dikdörtgen ile işaretlenir | ✅ |
| En az 2 kişilik yerel eğitim seti + tanıma | ✅ (TrainingData klasörü) |
| Tahmin edilen etiket ve güven skoru gösterilir | ✅ (ekran üstünde label + %) |
| Kamera Başlat / Durdur düğmeleri | ✅ |
| Görüntü Seç düğmesi | ✅ |
| Kamera bulunamazsa hata gösterilir | ✅ |
| Model dosyası bulunamazsa hata gösterilir | ✅ |

## Kullanılan Teknolojiler

- **C# / .NET 8.0 Windows Forms**
- **OpenCvSharp4.Windows** — OpenCV NuGet sarmalayıcısı
- **OpenCvSharp4.Extensions** — Mat ↔ Bitmap dönüşümü
- **LBPH (Local Binary Patterns Histograms)** — yüz tanıma algoritması
- **Haar Cascade** — yüz algılama modeli

## Kurulum ve Çalıştırma

```bash
# 1. Proje klasöründe:
dotnet build -c Release

# 2. Uygulamayı çalıştır:
dotnet run
# veya doğrudan exe:
# bin\Release\net8.0-windows\FaceRecognition.exe
```

## Eğitim Verisi Eklemek

Tanıma özelliğini aktif etmek için `TrainingData/` klasörüne kendi yüz resimlerinizi ekleyin:

```
TrainingData/
  Ahmet/
    foto1.jpg   ← farklı açılardan en az 5 fotoğraf
    foto2.jpg
    foto3.jpg
  Ayse/
    foto1.jpg
    foto2.jpg
```

Uygulama her başlatıldığında bu klasörü tarayarak modeli otomatik eğitir.

## Proje Yapısı

```
FaceRecognition/
├── FaceRecognition.csproj
├── Program.cs
├── Form1.cs                      ← Ana mantık (algılama + tanıma)
├── Form1.Designer.cs             ← UI düzeni
├── haarcascade_frontalface_default.xml   ← Haar cascade modeli
└── TrainingData/
    ├── README.md
    ├── Kisi1/   ← Buraya jpg/png ekleyin
    └── Kisi2/   ← Buraya jpg/png ekleyin
```
