using AttendeeAnalyzer.Models;
using Microsoft.AspNet.SignalR;

namespace AttendeeConnector
{
    public class AttendeeHub : Hub
    {
        public void SendId(string id)
        {
            Clients.All.BroadcastId(id);
        }

        public void SendQuestionText(string text)
        {
            Clients.All.BroadcastQuestionText(text);
        }

        public void SendQuestionHypo(string hypo)
        {
            Clients.All.BroadcastQuestionHypo(hypo);
        }

        public void SendEmotionScoreResult(EmotionScoreResult result)
        {
            Clients.All.BroadcastEmotionScoreResult(result);
        }

        public void StartQuestion()
        {
            Clients.All.BroadcastStartQuestion();
        }

        public void StopQuestion()
        {
            Clients.All.BroadcastStopQuestion();
        }

        public void ClearInput()
        {
            Clients.All.BroadcastClear();
        }
    }
}