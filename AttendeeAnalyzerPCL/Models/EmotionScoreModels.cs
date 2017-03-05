namespace AttendeeAnalyzer.Models
{

    public enum EmotionEnum
    {
        Anger, Contempt, Disgust, Fear, Happiness, Neutral, Sadness, Surprise
    }
       
    public class EmotionScoreResult
    {
        public EmotionEnum HighEmotion { get; set; }
        public string HighEmotionEmoji { get; set; }
        public float HighEmotionScore { get; set; }
    }
}
