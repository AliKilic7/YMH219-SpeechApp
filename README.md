# YMH219 - Sesli Okuma (TTS) ve Konuşma Tanıma (STT) Uygulaması

Nesne Tabanlı Programlama (YMH 219) dersi kapsamında .NET 8.0 Windows Forms ve `System.Speech` NuGet paketi kullanılarak geliştirilmiş, metni sese dönüştüren (TTS) ve mikrofondan konuşmayı metne çeviren (STT) masaüstü uygulamasıdır.

## Öğrenci Bilgileri
- **Ad Soyad:** Selahattin Ali Kılıç
- **Öğrenci No:** 250541036
- **Ders:** YMH 219 - Nesne Tabanlı Programlama

## Kullanılan Teknoloji / NuGet Paketleri ve OOP Mimarisi
- **Platform:** .NET 8.0 Windows Forms (`net8.0-windows`)
- **NuGet Paketi:** `System.Speech` (`SpeechSynthesizer` & `SpeechRecognitionEngine`)
- **OOP Mimarisi:**
  - **Interfaces & Polymorphism:** `ISpeechService`, `ITextToSpeechService` ve `ISpeechToTextService` arayüzleri ortak yaşam döngüsü (`Stop()`, `CurrentStatus`, `StatusChanged`, `ErrorOccurred`) sözleşmesi sunar.
  - **Managers & Encapsulation:** `TextToSpeechManager` ve `SpeechToTextManager` sınıfları sistem konuşma motorlarını sarmalar.
  - **Custom EventArgs:** `SpeechStatusChangedEventArgs`, `SpeechRecognizedTextEventArgs` ve `SpeechErrorEventArgs`.

## Kurulum ve Çalıştırma Adımları
```bash
cd YMH219-SpeechApp
dotnet restore
dotnet build
dotnet run
```

## Uygulamanın Temel Özellikleri
- **Text-to-Speech (TTS):** Yüklü sistem seslerini listeleme (`ComboBox`), konuşma hızını `-10` ile `+10` arasında ayarlama (`TrackBar`), `Okumayı Başlat`, `Duraklat/Devam Et`, `Durdur` ve `Temizle`.
- **Speech-to-Text (STT):** Yüklü Türkçe/İngilizce konuşma tanıma motorunu otomatik algılama, `Dinlemeyi Başlat` / `Dinlemeyi Durdur`, tanınan metni `[HH:mm:ss]` zaman damgasıyla ekleme ve `.txt` olarak dışa aktarma.
- **Durum Çubuğu ve Hata Koruması:** `"Hazır"`, `"Metin Okunuyor..."`, `"Duraklatıldı"`, `"Mikrofon Dinleniyor..."` durumlarını renkli alt çubukta gösterir; mikrofon veya konuşma motoru bulunamadığında uygulamanın çökmesini önleyip kullanıcıya `MessageBox` ile bilgi verir.

## Ekran Görüntüleri
![Ekran 1](screenshots/ekran1.png)
![Ekran 2](screenshots/ekran2.png)

## Bilinen Sınırlamalar / Geliştirme Notları
- `System.Speech` yalnızca Windows masaüstü ortamında çalışır.
- STT özelliği, bilgisayarda etkin bir mikrofon ve Windows Konuşma Tanıma dil paketinin (`tr-TR` veya `en-US`) kurulu olmasını gerektirir.
