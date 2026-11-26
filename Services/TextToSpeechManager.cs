using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using YMH219.SpeechApp.Core;
using YMH219.SpeechApp.Interfaces;
using YMH219.SpeechApp.Models;

namespace YMH219.SpeechApp.Services
{
    /// <summary>
    /// System.Speech.Synthesis.SpeechSynthesizer bileşenini sarmalayan TTS yöneticisi.
    /// </summary>
    public sealed class TextToSpeechManager : ITextToSpeechService
    {
        private readonly SpeechSynthesizer _synthesizer;
        private bool _disposed;

        public string CurrentStatus { get; private set; } = "Hazır";
        public bool IsSpeaking { get; private set; }
        public bool IsPaused { get; private set; }

        public int Rate
        {
            get => _synthesizer.Rate;
            set => _synthesizer.Rate = Math.Clamp(value, -10, 10);
        }

        public event EventHandler<SpeechStatusChangedEventArgs>? StatusChanged;
        public event EventHandler<SpeechErrorEventArgs>? ErrorOccurred;

        public TextToSpeechManager()
        {
            _synthesizer = new SpeechSynthesizer();
            try
            {
                _synthesizer.SetOutputToDefaultAudioDevice();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new SpeechErrorEventArgs("Ses Çıkış Aygıtı Hatası", $"Varsayılan ses çıkış aygıtı ayarlanamadı: {ex.Message}"));
            }

            _synthesizer.SpeakStarted += (_, _) =>
            {
                IsSpeaking = true;
                IsPaused = false;
                UpdateStatus("Metin Okunuyor...");
            };
            _synthesizer.SpeakCompleted += (_, e) =>
            {
                IsSpeaking = false;
                IsPaused = false;
                if (e.Error != null)
                {
                    ErrorOccurred?.Invoke(this, new SpeechErrorEventArgs("Okuma Hatası", e.Error.Message));
                }

                UpdateStatus("Hazır");
            };
        }

        public IReadOnlyList<VoiceOption> GetAvailableVoices()
        {
            var voices = new List<VoiceOption>();
            try
            {
                foreach (InstalledVoice installed in _synthesizer.GetInstalledVoices())
                {
                    if (installed.Enabled)
                    {
                        VoiceInfo info = installed.VoiceInfo;
                        voices.Add(new VoiceOption(
                            info.Name ?? "Varsayılan Ses",
                            info.Culture?.Name ?? "Bilinmeyen",
                            info.Gender.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new SpeechErrorEventArgs("Ses Listesi Hatası", $"Yüklü sesler alınamadı: {ex.Message}"));
            }

            return voices;
        }

        public void SelectVoice(string voiceName)
        {
            if (string.IsNullOrWhiteSpace(voiceName))
            {
                return;
            }

            try
            {
                _synthesizer.SelectVoice(voiceName);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new SpeechErrorEventArgs("Ses Seçim Hatası", $"Seçilen ses yüklenemedi: {ex.Message}"));
            }
        }

        public void SpeakAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Okunacak metin boş olamaz.", nameof(text));
            }

            try
            {
                if (IsPaused)
                {
                    _synthesizer.Resume();
                }

                _synthesizer.SpeakAsyncCancelAll();
                IsPaused = false;
                _synthesizer.SpeakAsync(text);
            }
            catch (Exception ex)
            {
                IsSpeaking = false;
                IsPaused = false;
                UpdateStatus("Hazır");
                ErrorOccurred?.Invoke(this, new SpeechErrorEventArgs("Konuşma Başlatma Hatası", ex.Message));
            }
        }

        public void Pause()
        {
            if (!IsSpeaking || IsPaused)
            {
                return;
            }

            try
            {
                _synthesizer.Pause();
                IsPaused = true;
                UpdateStatus("Duraklatıldı");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new SpeechErrorEventArgs("Duraklatma Hatası", ex.Message));
            }
        }

        public void Resume()
        {
            if (!IsSpeaking || !IsPaused)
            {
                return;
            }

            try
            {
                _synthesizer.Resume();
                IsPaused = false;
                UpdateStatus("Metin Okunuyor...");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new SpeechErrorEventArgs("Devam Ettirme Hatası", ex.Message));
            }
        }

        public void Stop()
        {
            try
            {
                if (IsPaused)
                {
                    _synthesizer.Resume();
                }

                _synthesizer.SpeakAsyncCancelAll();
            }
            catch
            {
                // İptal sırasında hata yoksayılır.
            }
            finally
            {
                IsSpeaking = false;
                IsPaused = false;
                UpdateStatus("Hazır");
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
            _synthesizer.Dispose();
        }
    }
}
