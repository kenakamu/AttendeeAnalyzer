using AttendeeAnalyzer;
using AttendeeAnalyzer.Models;
using AttendeeAnalyzer.Meetup.Models;
using AttendeeAnalyzerUWPShared.Services;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.Media.SpeechRecognition;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Microsoft.ProjectOxford.Common.Contract;
using Windows.UI.Core;

namespace AttendeeListener.ViewModels
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

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        public CaptureElement CaptureElement;
        private MediaCapture mediaCapture = new MediaCapture();
        private DeviceInformationCollection devInfoCollection;

        private FaceServiceClient faceClient = new FaceServiceClient(Settings.FaceKey);
        private EmotionServiceClient emotionClient = new EmotionServiceClient(Settings.EmotionKey);
        private FaceDetector faceDetector;

        private HubConnection hubConnection;
        private IHubProxy hubProxy;

        private SpeechRecognizer speechRecognizer;
        private StringBuilder dictatedTextBuilder = new StringBuilder();

        private bool keepListen;
        private bool getEmotion;

        public MainPageViewModel()
        {
            InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            Message = "Initializing";

            devInfoCollection = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Setup SignalR client and subscribe events.
            hubConnection = new HubConnection(Settings.HubUrl);
            hubProxy = hubConnection.CreateHubProxy(Settings.HubName);
            hubProxy.On("BroadcastStartQuestion", async () =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                     CoreDispatcherPriority.Normal, async () => { await StartQuestionAsync(); });
            });
            hubProxy.On("BroadcastStopQuestion", async () =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, async () => { await StopQuestionAsync(); });
            });
            hubProxy.On("BroadcastClear", async () =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, async () => { await ClearInputAsync(); });
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

            // Initialize video and start preview.
            await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings() { VideoDeviceId = devInfoCollection.Last().Id });
            CaptureElement.Source = mediaCapture;
            await mediaCapture.StartPreviewAsync();

            faceDetector = await FaceDetector.CreateAsync();

            Identify();
            GetEmotion();
        }

        /// <summary>
        /// Called when speech is fully recognized.
        /// </summary>
        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                dictatedTextBuilder.Append(args.Result.Text + ". ");
                QuestionText = dictatedTextBuilder.ToString();
                hubProxy.Invoke("SendQuestionText", dictatedTextBuilder.ToString());
            });
        }

        /// <summary>
        /// Called while speech is partically recognized
        /// </summary>
        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                QuestionHypo = args.Hypothesis.Text;
                hubProxy.Invoke("SendQuestionHypo", args.Hypothesis.Text);
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
                    await StartQuestionAsync();
            });
        }

        /// <summary>
        /// Detect face by using local function.
        /// </summary>
        private async Task<InMemoryRandomAccessStream> DetectFaceAsync()
        {
            var imgFormat = ImageEncodingProperties.CreateJpeg();

            while (true)
            {
                var stream = new InMemoryRandomAccessStream();
                await mediaCapture.CapturePhotoToStreamAsync(imgFormat, stream);

                var image = await ImageConverter.ConvertToSoftwareBitmapAsync(stream);

                var detectedFaces = await faceDetector.DetectFacesAsync(image);

                if (detectedFaces.Count == 0)
                    continue;
                else if (detectedFaces.Count != 1)
                    Message = "too many faces!";
                else
                    return stream;
            }
        }

        /// <summary>
        /// Identify the person by using Cognitive Face API
        /// </summary>
        private async void Identify()
        {
            while (true)
            {
                Message = "Seeing you...";
                using (var stream = await DetectFaceAsync())
                {
                    var faces = await faceClient.DetectAsync(ImageConverter.ConvertImage(stream));
                    if (!faces.Any())
                    {
                        continue;
                    }
                    else
                    {
                        try
                        {
                            var identifyResults = await faceClient.IdentifyAsync(Settings.PersonGroupId, faces.Select(x => x.FaceId).ToArray());
                            if (identifyResults.FirstOrDefault()?.Candidates?.Any() ?? false)
                            {
                                var personId = identifyResults.First().Candidates.First().PersonId;
                                var person = await faceClient.GetPersonAsync(Settings.PersonGroupId, personId);
                                Message = $"Hi {person.Name}";
                                var userData = JToken.Parse(person.UserData);
                                var id = userData["memberId"].ToString();
                                await hubProxy.Invoke("SendId", JToken.Parse(person.UserData)["memberId"].ToString());
                                getEmotion = true;
                                break;
                            }
                        }
                        catch
                        {
                            Message = "Please register yourself first.";
                            await Task.Delay(2000);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Start speech recognition
        /// </summary>
        private async Task StartQuestionAsync()
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
        /// Stop speech recognition
        /// </summary>
        public async Task StopQuestionAsync()
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

        private async Task ClearInputAsync()
        {
            await StopQuestionAsync();
            getEmotion = false;
            dictatedTextBuilder.Clear();
            Message = QuestionText = QuestionHypo = "";
            Identify();
        }

        /// <summary>
        /// Detect attendee's emotion and send to SignalR server.
        /// </summary>
        private async void GetEmotion()
        {
            while (true)
            {
                if (getEmotion)
                {
                    try
                    {
                        var stream = await DetectFaceAsync();
                        var emotions = await emotionClient.RecognizeAsync(ImageConverter.ConvertImage(stream));
                        var result = GetHighestEmotion(emotions.First().Scores);
                        await hubProxy.Invoke("SendEmotionScoreResult", result);
                    }
                    catch { /* hide error */ }
                }
                await Task.Delay(2000);
            }
        }

        /// <summary>
        /// Calculate the highest emotion from emotion result.
        /// </summary>
        /// <param name="scores">Emotion Result</param>
        /// <returns>EmotionScoreResult which contains score and emoji to express emotion.</returns>
        private EmotionScoreResult GetHighestEmotion(EmotionScores scores)
        {
            var tempHighestEmotion = EmotionEnum.Anger;
            var tempHighestScore = scores.Anger;
            var tempHighestEmotionEmoji = "\U0001F621";

            if (tempHighestScore <= scores.Contempt)
            {
                tempHighestScore = scores.Contempt;
                tempHighestEmotion = EmotionEnum.Contempt;
                tempHighestEmotionEmoji = "\U0001F615";
            }
            if (tempHighestScore <= scores.Disgust)
            {
                tempHighestScore = scores.Disgust;
                tempHighestEmotion = EmotionEnum.Disgust;
                tempHighestEmotionEmoji = "\U0001F62C";
            }
            if (tempHighestScore <= scores.Fear)
            {
                tempHighestScore = scores.Fear;
                tempHighestEmotion = EmotionEnum.Fear;
                tempHighestEmotionEmoji = "\U0001F631";
            }
            if (tempHighestScore <= scores.Happiness)
            {
                tempHighestScore = scores.Happiness;
                tempHighestEmotion = EmotionEnum.Happiness;
                tempHighestEmotionEmoji = "\U0001F60D";
            }
            if (tempHighestScore <= scores.Neutral)
            {
                tempHighestScore = scores.Neutral;
                tempHighestEmotion = EmotionEnum.Neutral;
                tempHighestEmotionEmoji = "\U0001F610";
            }
            if (tempHighestScore <= scores.Sadness)
            {
                tempHighestScore = scores.Sadness;
                tempHighestEmotion = EmotionEnum.Sadness;
                tempHighestEmotionEmoji = "\U0001F622";
            }
            if (tempHighestScore <= scores.Surprise)
            {
                tempHighestScore = scores.Surprise;
                tempHighestEmotion = EmotionEnum.Surprise;
                tempHighestEmotionEmoji = "\U0001F632";
            }

            return new EmotionScoreResult()
            {
                HighEmotion = tempHighestEmotion,
                HighEmotionScore = tempHighestScore,
                HighEmotionEmoji = tempHighestEmotionEmoji
            };
        }

        /// <summary>
        /// Switch the webcam device
        /// </summary>
        /// <returns></returns>
        public async Task SwitchCameraAsync()
        {
            if (devInfoCollection.Count == 1)
                return;

            var video = devInfoCollection.Where(x => x.Id.ToLower() != mediaCapture.MediaCaptureSettings.VideoDeviceId.ToLower());
            var settings = new MediaCaptureInitializationSettings();
            settings.VideoDeviceId = video.First().Id;
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync(settings);
            CaptureElement.Source = mediaCapture;
            await mediaCapture.StartPreviewAsync();
        }      
    }
}
