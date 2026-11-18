# Eğitim Verisi Klasörü

Bu klasör, yüz tanıma için kullanılan eğitim verilerini içerir.

## Klasör Yapısı

```
TrainingData/
  Kisi1/          ← 1. kişinin adı (klasör adı = etiket)
    img1.jpg
    img2.jpg
    img3.jpg      (en az 5-10 görüntü önerilir)
  Kisi2/          ← 2. kişinin adı
    img1.jpg
    img2.jpg
    ...
  Ahmet/          ← Kendi yüzünüzü ekleyebilirsiniz
    foto1.jpg
    ...
```

## Nasıl Kullanılır

1. `Kisi1` ve `Kisi2` klasörlerinin adlarını tanımak istediğiniz kişilerin
   gerçek adlarıyla değiştirin (örn. `Ahmet`, `Ayse`).
2. Her kişi için en az **5 farklı açıdan** yüz fotoğrafı (`.jpg` veya `.png`)
   ekleyin.
3. Uygulamayı yeniden başlatın — model otomatik olarak eğitilecektir.

## İpuçları

- Görüntüler yüzü açıkça göstermeli ve aydınlık olmalıdır.
- Farklı ifadeler, açılar ve aydınlatma koşulları modeli güçlendirir.
- Görüntü boyutu 100×100 piksele otomatik ölçeklenir; büyük resimler de çalışır.
