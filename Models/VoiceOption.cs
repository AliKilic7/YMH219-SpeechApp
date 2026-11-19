namespace YMH219.SpeechApp.Models
{
    /// <summary>
    /// Sistem üzerinde kurulu TTS ses profili bilgisini tutar.
    /// </summary>
    public sealed class VoiceOption
    {
        public string Name { get; }
        public string Culture { get; }
        public string Gender { get; }

        public VoiceOption(string name, string culture, string gender)
        {
            Name = name;
            Culture = culture;
            Gender = gender;
        }

        public override string ToString()
        {
            return $"{Name} ({Culture} - {Gender})";
        }
    }
}
