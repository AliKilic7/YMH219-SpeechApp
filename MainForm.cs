using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YMH219.SpeechApp.Core;
using YMH219.SpeechApp.Interfaces;
using YMH219.SpeechApp.Models;
using YMH219.SpeechApp.Services;

namespace YMH219.SpeechApp
{
    /// <summary>
    /// Metin Okuma (TTS) ve Konuşma Tanıma (STT) panellerini içeren ana form.
    /// </summary>
    public sealed class MainForm : Form
    {
        private readonly ITextToSpeechService _ttsService = new TextToSpeechManager();
        private readonly ISpeechToTextService _sttService = new SpeechToTextManager();
        private readonly TranscriptExportService _exportService = new();

        private readonly Label _lblActiveStatus;
        private readonly Panel _statusBarPanel;

        // TTS Kontrolleri
        private readonly RichTextBox _rtbTtsInput;
        private readonly ComboBox _cmbVoices;
        private readonly TrackBar _trkRate;
        private readonly Label _lblRateValue;
        private readonly Button _btnSpeak;
        private readonly Button _btnPauseResume;
        private readonly Button _btnStopTts;
        private readonly Button _btnClearTts;

        // STT Kontrolleri
        private readonly Button _btnStartListen;
        private readonly Button _btnStopListen;
        private readonly Button _btnSaveTranscript;
        private readonly Button _btnClearStt;
        private readonly Label _lblSttCulture;
        private readonly RichTextBox _rtbTranscript;

        public MainForm()
        {
            Text = "YMH 219 - Sesli Okuma (TTS) ve Konuşma Tanıma (STT) Uygulaması";
            Size = new Size(980, 640);
            MinimumSize = new Size(880, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 247, 250);

            // Alt durum çubuğu
            _statusBarPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(14, 6, 14, 6)
            };

            _lblActiveStatus = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Durum: Hazır",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _statusBarPanel.Controls.Add(_lblActiveStatus);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // SOL: TTS BÖLÜMÜ
            var ttsGroup = new GroupBox
            {
                Text = "Metin Okuma (Text-to-Speech / TTS)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            var ttsPanel = new Panel { Dock = DockStyle.Fill };

            ttsPanel.Controls.Add(CreateLabel("Ses Seçimi:", 4, 8));
            _cmbVoices = new ComboBox
            {
                Location = new Point(4, 28),
                Width = 420,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f)
            };
            ttsPanel.Controls.Add(_cmbVoices);

            _lblRateValue = CreateLabel("Konuşma Hızı: 0", 4, 64);
            ttsPanel.Controls.Add(_lblRateValue);

            _trkRate = new TrackBar
            {
                Location = new Point(4, 84),
                Width = 420,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Minimum = -10,
                Maximum = 10,
                Value = 0,
                TickFrequency = 2
            };
            ttsPanel.Controls.Add(_trkRate);

            ttsPanel.Controls.Add(CreateLabel("Okunacak Metin:", 4, 134));
            _rtbTtsInput = new RichTextBox
            {
                Location = new Point(4, 156),
                Size = new Size(420, 290),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10.5f),
                Text = "Merhaba! YMH 219 Nesne Tabanlı Programlama dersi konuşma sentezleme testi başarılı.",
                BorderStyle = BorderStyle.FixedSingle
            };
            ttsPanel.Controls.Add(_rtbTtsInput);

            _btnSpeak = CreateButton("Okumayı Başlat", 4, 462, 125, Color.FromArgb(22, 163, 74));
            _btnPauseResume = CreateButton("Duraklat", 136, 462, 112, Color.FromArgb(234, 179, 8));
            _btnStopTts = CreateButton("Durdur", 255, 462, 82, Color.FromArgb(220, 38, 38));
            _btnClearTts = CreateButton("Temizle", 344, 462, 80, Color.FromArgb(100, 116, 139));
            _btnPauseResume.Enabled = false;
            _btnStopTts.Enabled = false;

            foreach (Button btn in new[] { _btnSpeak, _btnPauseResume, _btnStopTts, _btnClearTts })
            {
                btn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            }

            ttsPanel.Controls.AddRange(new Control[] { _btnSpeak, _btnPauseResume, _btnStopTts, _btnClearTts });
            ttsGroup.Controls.Add(ttsPanel);

            // SAĞ: STT BÖLÜMÜ
            var sttGroup = new GroupBox
            {
                Text = "Konuşma Tanıma (Speech-to-Text / STT)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            var sttPanel = new Panel { Dock = DockStyle.Fill };

            _btnStartListen = CreateButton("Dinlemeyi Başlat", 4, 12, 145, Color.FromArgb(37, 99, 235));
            _btnStopListen = CreateButton("Dinlemeyi Durdur", 156, 12, 145, Color.FromArgb(220, 38, 38));
            _btnStopListen.Enabled = false;
            sttPanel.Controls.Add(_btnStartListen);
            sttPanel.Controls.Add(_btnStopListen);

            _lblSttCulture = CreateLabel("Tanıma Motoru: Henüz başlatılmadı", 4, 52);
            _lblSttCulture.ForeColor = Color.FromArgb(71, 85, 105);
            sttPanel.Controls.Add(_lblSttCulture);

            sttPanel.Controls.Add(CreateLabel("Tanınan Metin (Zaman Damgalı):", 4, 80));
            _rtbTranscript = new RichTextBox
            {
                Location = new Point(4, 102),
                Size = new Size(420, 344),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                Font = new Font("Consolas", 9.5f),
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle
            };
            sttPanel.Controls.Add(_rtbTranscript);

            _btnSaveTranscript = CreateButton(".txt Olarak Kaydet", 4, 462, 155, Color.FromArgb(15, 118, 110));
            _btnClearStt = CreateButton("Temizle", 166, 462, 92, Color.FromArgb(100, 116, 139));
            _btnSaveTranscript.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnClearStt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            sttPanel.Controls.Add(_btnSaveTranscript);
            sttPanel.Controls.Add(_btnClearStt);

            sttGroup.Controls.Add(sttPanel);

            mainLayout.Controls.Add(ttsGroup, 0, 0);
            mainLayout.Controls.Add(sttGroup, 1, 0);

            Controls.Add(mainLayout);
            Controls.Add(_statusBarPanel);

            LoadVoices();
            WireEvents();
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
        }

        private static Button CreateButton(string text, int x, int y, int width, Color backColor)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 32),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadVoices()
        {
            IReadOnlyList<VoiceOption> voices = _ttsService.GetAvailableVoices();
            _cmbVoices.Items.Clear();
            foreach (VoiceOption voice in voices)
            {
                _cmbVoices.Items.Add(voice);
            }

            if (_cmbVoices.Items.Count > 0)
            {
                _cmbVoices.SelectedIndex = 0;
            }
        }

        private void WireEvents()
        {
            _cmbVoices.SelectedIndexChanged += (_, _) =>
            {
                if (_cmbVoices.SelectedItem is VoiceOption voice)
                {
                    _ttsService.SelectVoice(voice.Name);
                }
            };

            _trkRate.ValueChanged += (_, _) =>
            {
                _ttsService.Rate = _trkRate.Value;
                _lblRateValue.Text = $"Konuşma Hızı: {_trkRate.Value}";
            };

            _btnSpeak.Click += BtnSpeak_Click;
            _btnPauseResume.Click += BtnPauseResume_Click;
            _btnStopTts.Click += (_, _) => _ttsService.Stop();
            _btnClearTts.Click += (_, _) => _rtbTtsInput.Clear();

            _btnStartListen.Click += BtnStartListen_Click;
            _btnStopListen.Click += (_, _) => _sttService.Stop();
            _btnClearStt.Click += (_, _) => _rtbTranscript.Clear();
            _btnSaveTranscript.Click += BtnSaveTranscript_Click;

            _ttsService.StatusChanged += (_, e) => UpdateStatusBar(e.StatusText);
            _ttsService.ErrorOccurred += (_, e) => ShowErrorSafe(e.Title, e.Message);

            _sttService.StatusChanged += (_, e) => UpdateStatusBar(e.StatusText);
            _sttService.SpeechRecognized += SttService_SpeechRecognized;
            _sttService.ErrorOccurred += (_, e) => ShowErrorSafe(e.Title, e.Message);
        }

        private void RunOnUi(Action action)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private void BtnSpeak_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_rtbTtsInput.Text))
                {
                    MessageBox.Show("Lütfen okunacak bir metin giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _ttsService.SpeakAsync(_rtbTtsInput.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Metin okuma başlatılamadı:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPauseResume_Click(object? sender, EventArgs e)
        {
            if (_ttsService.IsPaused)
            {
                _ttsService.Resume();
            }
            else
            {
                _ttsService.Pause();
            }
        }

        private void BtnStartListen_Click(object? sender, EventArgs e)
        {
            _sttService.StartListening();
            _lblSttCulture.Text = $"Tanıma Motoru: {_sttService.ActiveCultureDisplay}";
        }

        private void SttService_SpeechRecognized(object? sender, SpeechRecognizedTextEventArgs e)
        {
            RunOnUi(() =>
            {
                _rtbTranscript.AppendText(e.ToFormattedLine() + Environment.NewLine);
                _rtbTranscript.ScrollToCaret();
            });
        }

        private void UpdateStatusBar(string status)
        {
            RunOnUi(() =>
            {
                _lblActiveStatus.Text = $"Durum: {status}";
                _statusBarPanel.BackColor = status switch
                {
                    "Metin Okunuyor..." => Color.FromArgb(22, 163, 74),
                    "Duraklatıldı" => Color.FromArgb(202, 138, 4),
                    "Mikrofon Dinleniyor..." => Color.FromArgb(37, 99, 235),
                    _ => Color.FromArgb(30, 41, 59)
                };

                _btnPauseResume.Enabled = _ttsService.IsSpeaking;
                _btnPauseResume.Text = _ttsService.IsPaused ? "Devam Et" : "Duraklat";
                _btnStopTts.Enabled = _ttsService.IsSpeaking;

                _btnStartListen.Enabled = !_sttService.IsListening;
                _btnStopListen.Enabled = _sttService.IsListening;
            });
        }

        private void ShowErrorSafe(string title, string message)
        {
            RunOnUi(() =>
            {
                MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            });
        }

        private void BtnSaveTranscript_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_rtbTranscript.Text))
            {
                MessageBox.Show("Kaydedilecek konuşma metni bulunmuyor.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "Metin Dosyası (*.txt)|*.txt",
                FileName = $"konusma_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                Title = "Konuşma Transkriptini Kaydet"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                _exportService.Export(dialog.FileName, _rtbTranscript.Text);
                MessageBox.Show("Transkript başarıyla kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dosya kaydedilemedi:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _ttsService.Dispose();
            _sttService.Dispose();
            base.OnFormClosing(e);
        }
    }
}
