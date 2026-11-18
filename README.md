# Yüz Algılama ve Tanıma — OpenCV ile C#

**Ödev 7** — OpenCV tabanlı yüz algılama ve temel yüz tanıma uygulaması (.NET 10 Windows Forms).

## Özellikler

| Gereksinim | Durum |
|---|---|
| Kamera görüntüsü PictureBox'ta gösterilir | ✅ |
| Yüz bölgesi dikdörtgen ile işaretlenir (Yeşil: Tanınan, Turuncu: Bilinmeyen) | ✅ |
| En az 2 kişilik yerel eğitim seti + temel tanıma | ✅ (`mustafa`, `Kisi2`) |
| Tahmin edilen etiket ve güven skoru gösterilir (`mustafa 85.2%`) | ✅ |
| Kamera Başlat / Durdur düğmeleri | ✅ |
| Görüntü Seç düğmesi (dosyadan resim yükleme ve test etme) | ✅ |
| Modeli Yeniden Eğit düğmesi (tek tıkla anında yeniden eğitim) | ✅ |
| Kamera veya model dosyası bulunamadığında hata bildirimi | ✅ |

## Kullanılan Teknolojiler

- **C# / .NET 10.0 Windows Forms**
- **OpenCvSharp4.Windows** — OpenCV 4.9 sarmalayıcısı
- **OpenCvSharp4.Extensions** — Mat ↔ Bitmap dönüşümü
- **LBPH (Local Binary Patterns Histograms)** — yüz tanıma modeli
- **Haar Cascade** (`haarcascade_frontalface_default.xml`) — yüz algılama

## Kurulum ve Çalıştırma

```bash
# 1. Projeyi derleyin:
dotnet build -c Release

# 2. Çalıştırın:
dotnet run -c Release
# veya doğrudan exe:
# bin/Release/net10.0-windows/FaceRecognition.exe
```

## Eğitim Verisi Eklemek

Tanıma için `TrainingData/` klasörü altına kişi adıyla klasör açıp içine yüz fotoğrafları ekleyin:

```
TrainingData/
  mustafa/
    foto1.jpg   ← farklı açılardan en az 5-10 fotoğraf
    foto2.jpg
    ...
  Kisi2/
    foto1.jpg
    ...
```

Uygulama başlatıldığında veya **"🔄 Modeli Eğit"** düğmesine tıklandığında:
1. Her fotoğraftaki yüz Haar Cascade ile otomatik kırpılır.
2. Histogram eşitleme (EqualizeHist) uygulanarak ışık dengelenir.
3. Yatay aynalama (flip) ile veri çeşitliliği artırılır.
4. LBPH modeli eğitilir ve kullanıma hazır hale gelir.

## Proje Yapısı

```
FaceRecognition/
├── FaceRecognition.csproj
├── Program.cs
├── Form1.cs                      ← Ana mantık (kamera döngüsü, algılama, tanıma)
├── Form1.Designer.cs             ← Koyu temalı modern arayüz
├── haarcascade_frontalface_default.xml   ← Haar cascade yüz modeli
└── TrainingData/
    ├── README.md
    ├── mustafa/                  ← Mustafa için yüz örnekleri
    └── Kisi2/                    ← 2. kişi için yüz örnekleri
```
