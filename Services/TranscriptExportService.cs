using System;
using System.IO;
using System.Text;

namespace YMH219.SpeechApp.Services
{
    /// <summary>
    /// Tanınan konuşma metinlerini .txt dosyasına kaydeder.
    /// </summary>
    public sealed class TranscriptExportService
    {
        public void Export(string filePath, string content)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Dosya yolu boş olamaz.", nameof(filePath));
            }

            var sb = new StringBuilder();
            sb.AppendLine("YMH 219 - Konuşma Tanıma (STT) Transkripti");
            sb.AppendLine($"Tarih: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine(content);

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
