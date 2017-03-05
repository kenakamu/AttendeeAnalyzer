using AttendeeAnalyzer;
using AttendeeAnalyzer.Meetup.Models;
using AttendeeAnalyzer.Models;
using AttendeeAnalyzer.Services;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;

namespace SpeakerPortal.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; NotifyPropertyChanged(); }
        }

        private string questionText;
        public string QuestionText
        {
            get { return questionText; }
            set { questionText = value; NotifyPropertyChanged(); }
        }

        private string questionHypo;
        public string QuestionHypo
        {
            get { return questionHypo; }
            set { questionHypo = value; NotifyPropertyChanged(); }
        }

        private string answerText;
        public string AnswerText
        {
            get { return answerText; }
            set { answerText = value; NotifyPropertyChanged(); }
        }

        private string answerHypo;
        public string AnswerHypo
        {
            get { return answerHypo; }
            set { answerHypo = value; NotifyPropertyChanged(); }
        }
                
        private Member member;
        public Member Member
        {
            get { return member; }
            set { member = value; NotifyPropertyChanged(); }
        }

        private string topics;
        public string Topics
        {
            get { return topics; }
            set { topics = value; NotifyPropertyChanged(); }
        }

        private string groups;
        public string Groups
        {
            get { return groups; }
            set { groups = value; NotifyPropertyChanged(); }
        }

        private EmotionScoreResult emotionScoreResult;
        public EmotionScoreResult EmotionScoreResult
        {
            get { return emotionScoreResult; }
            set { emotionScoreResult = value; NotifyPropertyChanged(); }
        }

        private HubConnection hubConnection;
        private IHubProxy hubProxy;

        private MeetupService meetupService = new MeetupService();
        private SpeechRecognizer speechRecognizer;
        private StringBuilder dictatedTextBuilder = new StringBuilder();

        private bool keepListen = false;

        private Event currentEvent;

        public MainPageViewModel()
        {
            InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            currentEvent = await meetupService.GetCurrentEventAsync();
            if (currentEvent == null)
            {
                Message = "No event scheduled.";
                return;
            }

            // Setup SignalR client and subscribe events.
            hubConnection = new HubConnection(Settings.HubUrl);
            hubProxy = hubConnection.CreateHubProxy(Settings.HubName);
            hubProxy.On<string>("BroadcastId", async (id) =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, async () => { await SetMemberAsync(id); });                
            });
            hubProxy.On<string>("BroadcastQuestionText", async text =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal, () => { QuestionText = text; });
            });
            hubProxy.On<string>("BroadcastQuestionHypo", async hypo =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal, () => { QuestionHypo = hypo; });
            });
            hubProxy.On<EmotionScoreResult>("BroadcastEmotionScoreResult", async result =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal, () => { EmotionScoreResult = result; });
            });
            await hubConnection.Start();

            // Initialize Speech recognizer and subscribe event.
            speechRecognizer = new SpeechRecognizer(new Windows.Globalization.Language(Settings.SpeechLanguage));
            speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(25);
            speechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.FromSeconds(50);
            speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromMilliseconds(50);

            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
            speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            await speechRecognizer.CompileConstraintsAsync();
        }

        /// <summary>
        /// Called when speech is fully recognized.
        /// </summary>
        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                dictatedTextBuilder.Append(args.Result.Text + ". ");
                AnswerText = dictatedTextBuilder.ToString();
            });
        }

        /// <summary>
        /// Called while speech is partically recognized
        /// </summary>
        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AnswerHypo = args.Hypothesis.Text;
            });
        }

        /// <summary>
        /// Called when speech session considered to be completed.
        /// </summary>
        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (keepListen)
                    await StartAnswerAsync();
            });
        }
        
        /// <summary>
        /// Get member profile from Meetup and display
        /// </summary>
        /// <param name="id">Meetup member id</param>
        private async Task SetMemberAsync(string id)
        {
            Member = await meetupService.GetMemberAsync(id);
            Topics = string.Join(",", Member.Topics.ToList());

            var groups = await meetupService.GetGroupsOfMemberAsync(id);
            Groups = $@"Tech: {string.Join(",", groups.Where(x => x.Category.Name == "tech"))}
Other: {string.Join(",", groups.Where(x => x.Category.Name != "tech"))}
";           
        }

        /// <summary>
        /// Start speech recognition for speaker
        /// </summary>
        public async Task StartAnswerAsync()
        {
            Message = "Starting Microphone...";
            keepListen = true;
            while (speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                try { await speechRecognizer.ContinuousRecognitionSession.StartAsync(SpeechContinuousRecognitionMode.Default); await Task.Delay(100); }
                catch { /* hide error */ }
            }
            Message = "Microphone started...";
        }

        /// <summary>
        /// Stop speech recognition for speaker
        /// </summary>
        public async void StopAnswerAsync()
        {
            Message = "Stopping Microphone...";
            keepListen = false;
            while (speechRecognizer.State != SpeechRecognizerState.Idle)
            {
                try { await speechRecognizer.ContinuousRecognitionSession.StopAsync(); await Task.Delay(100); }
                catch { /* hide error */ }
            }
            Message = "Microphone stopped...";
        }

        /// <summary>
        /// Send start question siganl to SignalR server.
        /// </summary>
        public async void StartQuestion()
        {
            await hubProxy.Invoke("StartQuestion");
        }

        /// <summary>
        /// Send stop question siganl to SignalR server.
        /// </summary>
        public async void StopQuestion()
        {
            await hubProxy.Invoke("StopQuestion");
        }
        
        /// <summary>
        /// Save the QA to meetup event comment
        /// </summary>
        public async void SaveQA()
        {
            await meetupService.PostCommentAsync($"{Member.Name} asked. Question: {QuestionText} Answer:{AnswerText}", currentEvent.Id);
            Message = "QA Saved!";
        }

        public async void ClearInput()
        {
            QuestionText = QuestionHypo = AnswerText = AnswerHypo = Topics = Groups = Message = "";
            Member = null;
            await hubProxy.Invoke("ClearInput");
        }        
    }
}
