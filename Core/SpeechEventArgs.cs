using System;

namespace YMH219.SpeechApp.Core
{
    public sealed class SpeechStatusChangedEventArgs : EventArgs
    {
        public string StatusText { get; }

        public SpeechStatusChangedEventArgs(string statusText)
        {
            StatusText = statusText;
        }
    }

    public sealed class SpeechRecognizedTextEventArgs : EventArgs
    {
        public string RecognizedText { get; }
        public float Confidence { get; }
        public DateTime Timestamp { get; }

        public SpeechRecognizedTextEventArgs(string recognizedText, float confidence, DateTime? timestamp = null)
        {
            RecognizedText = recognizedText;
            Confidence = confidence;
            Timestamp = timestamp ?? DateTime.Now;
        }

        public string ToFormattedLine()
        {
            return $"[{Timestamp:HH:mm:ss}] {RecognizedText} (Güven: %{Confidence * 100f:F0})";
        }
    }

    public sealed class SpeechErrorEventArgs : EventArgs
    {
        public string Title { get; }
        public string Message { get; }

        public SpeechErrorEventArgs(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}
