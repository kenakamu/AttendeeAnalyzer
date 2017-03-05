namespace AttendeeAnalyzer
{
    public static class Settings
    {
        // SignalR Settings
        public static string HubUrl = "http://localhost:2597/"; //"http://<yourwebsite>.azurewebsites.net";
        public static string HubName = "AttendeeHub";

        // Meetup.com Settings
        public static string GroupName = ""; //"Tokyo-about_PowerShell-Meetup";
        public static string MeetupAPIKey = "";

        // Cognitive Settings
        public static string PersonGroupId = ""; //"powershellmeetup"; //all lower case
        public static string EmotionKey = "";
        public static string FaceKey = "";

        // Speech Settings
        public static string SpeechLanguage = "en-US";
    }
}
