using System;
using System.Collections.Generic;
using YMH219.SpeechApp.Core;
using YMH219.SpeechApp.Models;

namespace YMH219.SpeechApp.Interfaces
{
    /// <summary>
    /// Metin okuma ve konuşma tanıma servisleri için ortak temel arayüz.
    /// </summary>
    public interface ISpeechService : IDisposable
    {
        string CurrentStatus { get; }
        event EventHandler<SpeechStatusChangedEventArgs>? StatusChanged;
        event EventHandler<SpeechErrorEventArgs>? ErrorOccurred;
        void Stop();
    }

    public interface ITextToSpeechService : ISpeechService
    {
        bool IsSpeaking { get; }
        bool IsPaused { get; }
        int Rate { get; set; }

        IReadOnlyList<VoiceOption> GetAvailableVoices();
        void SelectVoice(string voiceName);
        void SpeakAsync(string text);
        void Pause();
        void Resume();
    }

    public interface ISpeechToTextService : ISpeechService
    {
        bool IsListening { get; }
        string ActiveCultureDisplay { get; }
        event EventHandler<SpeechRecognizedTextEventArgs>? SpeechRecognized;

        void StartListening();
    }
}
