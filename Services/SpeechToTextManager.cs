using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using YMH219.SpeechApp.Core;
using YMH219.SpeechApp.Interfaces;

namespace YMH219.SpeechApp.Services
{
    /// <summary>
    /// System.Speech.Recognition.SpeechRecognitionEngine bileşenini sarmalayan STT yöneticisi.
    /// Yüklü Türkçe/İngilizce tanıma motorunu otomatik seçer ve mikrofon hatalarını yönetir.
    /// </summary>
    public sealed class SpeechToTextManager : ISpeechToTextService
    {
        private SpeechRecognitionEngine? _engine;
        private bool _disposed;

        public string CurrentStatus { get; private set; } = "Hazır";
        public bool IsListening { get; private set; }
        public string ActiveCultureDisplay { get; private set; } = "Henüz başlatılmadı";

        public event EventHandler<SpeechStatusChangedEventArgs>? StatusChanged;
        public event EventHandler<SpeechErrorEventArgs>? ErrorOccurred;
        public event EventHandler<SpeechRecognizedTextEventArgs>? SpeechRecognized;

        public void StartListening()
        {
            if (IsListening)
            {
                return;
            }

            try
            {
                EnsureEngineInitialized();
                if (_engine == null)
                {
                    return;
                }

                _engine.SetInputToDefaultAudioDevice();
                _engine.RecognizeAsync(RecognizeMode.Multiple);
                IsListening = true;
                UpdateStatus("Mikrofon Dinleniyor...");
            }
            catch (InvalidOperationException ex)
            {
                CleanupEngine();
                IsListening = false;
                UpdateStatus("Hazır");
                ErrorOccurred?.Invoke(
                    this,
                    new SpeechErrorEventArgs(
                        "Mikrofon veya Ses Aygıtı Bulunamadı",
                        $"Varsayılan mikrofon aygıtı başlatılamadı. Mikrofonunuzun bağlı ve etkin olduğundan emin olun.\n\nDetay: {ex.Message}"));
            }
            catch (Exception ex)
            {
                CleanupEngine();
                IsListening = false;
                UpdateStatus("Hazır");
                ErrorOccurred?.Invoke(
                    this,
                    new SpeechErrorEventArgs(
                        "Konuşma Tanıma Başlatılamadı",
                        $"Konuşma tanıma motoru başlatılırken hata oluştu:\n{ex.Message}"));
            }
        }

        public void Stop()
        {
            if (!IsListening && _engine == null)
            {
                return;
            }

            try
            {
                _engine?.RecognizeAsyncCancel();
            }
            catch
            {
                // İptal sırasında oluşan hatalar yoksayılır.
            }
            finally
            {
                IsListening = false;
                UpdateStatus("Hazır");
            }
        }

        private void EnsureEngineInitialized()
        {
            if (_engine != null)
            {
                return;
            }

            IReadOnlyCollection<RecognizerInfo> recognizers = SpeechRecognitionEngine.InstalledRecognizers();
            if (recognizers == null || recognizers.Count == 0)
            {
                throw new InvalidOperationException(
                    "Sistemde yüklü bir Windows Konuşma Tanıma motoru (Türkçe veya İngilizce dil paketi) bulunamadı.");
            }

            RecognizerInfo selected = recognizers.FirstOrDefault(r => r.Culture.Name.Equals("tr-TR", StringComparison.OrdinalIgnoreCase))
                ?? recognizers.FirstOrDefault(r => r.Culture.TwoLetterISOLanguageName.Equals("tr", StringComparison.OrdinalIgnoreCase))
                ?? recognizers.FirstOrDefault(r => r.Culture.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                ?? recognizers.FirstOrDefault(r => r.Culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
                ?? recognizers.First();

            ActiveCultureDisplay = $"{selected.Culture.DisplayName} ({selected.Culture.Name})";
            _engine = new SpeechRecognitionEngine(selected);
            _engine.LoadGrammar(new DictationGrammar());
            _engine.SpeechRecognized += Engine_SpeechRecognized;
            _engine.RecognizeCompleted += Engine_RecognizeCompleted;
        }

        private void Engine_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result == null || string.IsNullOrWhiteSpace(e.Result.Text))
            {
                return;
            }

            SpeechRecognized?.Invoke(this, new SpeechRecognizedTextEventArgs(e.Result.Text, e.Result.Confidence));
        }

        private void Engine_RecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
        {
            IsListening = false;
            if (e.Error != null && !e.Cancelled)
            {
                ErrorOccurred?.Invoke(
                    this,
                    new SpeechErrorEventArgs("Mikrofon Dinleme Hatası", $"Dinleme sırasında ses akışı kesildi: {e.Error.Message}"));
            }

            UpdateStatus("Hazır");
        }

        private void CleanupEngine()
        {
            if (_engine == null)
            {
                return;
            }

            try
            {
                _engine.SpeechRecognized -= Engine_SpeechRecognized;
                _engine.RecognizeCompleted -= Engine_RecognizeCompleted;
                _engine.Dispose();
            }
            catch
            {
                // Temizleme hatası yoksayılır.
            }
            finally
            {
                _engine = null;
            }
        }

        private void UpdateStatus(string status)
        {
            CurrentStatus = status;
            StatusChanged?.Invoke(this, new SpeechStatusChangedEventArgs(status));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Stop();
            CleanupEngine();
        }
    }
}
