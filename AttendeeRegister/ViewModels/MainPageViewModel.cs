using AttendeeAnalyzer;
using AttendeeAnalyzer.Meetup.Models;
using AttendeeAnalyzer.Services;
using AttendeeAnalyzerUWPShared.Services;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace AttendeeRegister.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private List<RSVP> rsvps;
        public List<RSVP> RSVPs
        {
            get { return rsvps; }
            set { rsvps = value; NotifyPropertyChanged(); }
        }

        private RSVP selectedRSVP;
        public RSVP SelectedRSVP
        {
            get { return selectedRSVP; }
            set
            {
                if (selectedRSVP == value)
                    return;
                selectedRSVP = value;
                NotifyPropertyChanged();               
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; NotifyPropertyChanged(); }
        }

        public string Title = "Welcome to " + Settings.GroupName;

        public CaptureElement CaptureElement;

        private MediaCapture mediaCapture = new MediaCapture();
        private MeetupService meetupService = new MeetupService();

        private FaceServiceClient faceClient = new FaceServiceClient(Settings.FaceKey); // Azure Cognitive Face Service Client
        private List<Person> registeredPersons; // Store all persons who registered to Face API.
        private FaceDetector faceDetector; // local camera face detector
        private IList<DetectedFace> detectedFaces; // Store faces detected locally.

        private Event currentEvent;
        private List<Comment> RSVPComments; // Store RSVP status as meetup event comment.

        public MainPageViewModel()
        {
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            Message = "Initializing..";
            faceDetector = await FaceDetector.CreateAsync();
            await mediaCapture.InitializeAsync();
            CaptureElement.Source = mediaCapture;
            await LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            Message = "Loading Data..";

            // Check if face group alredy exists, otherwise create it.
            try
            {
                await faceClient.GetPersonGroupAsync(Settings.PersonGroupId);
            }
            catch (FaceAPIException ex)
            {
                if (ex.ErrorCode == "PersonGroupNotFound")
                    await faceClient.CreatePersonGroupAsync(Settings.PersonGroupId, Settings.PersonGroupId);
                else
                    throw;
            }

            currentEvent = await meetupService.GetCurrentEventAsync();
            if (currentEvent == null)
            {
                Message = "No event scheduled.";
                return;
            }

            registeredPersons = (await faceClient.GetPersonsAsync(Settings.PersonGroupId)).ToList();
            RSVPs = await meetupService.GetRSVPsAsync(currentEvent.Id);
            // Get comments start with 'Welcome' to track who comes to the event.
            RSVPComments = (await meetupService.GetCommentsAsync(currentEvent.Id)).Where(x=>x.CommentDetail.StartsWith("Welcome ")).ToList();

            // Check if RSVPed meetup member is registered to Face API.
            foreach (RSVP rsvp in RSVPs)
            {
                var registeredPerson = registeredPersons.FirstOrDefault(x => x.Name == rsvp.Member.Name);
                if (registeredPerson == null)
                {
                    var userData = new JObject();
                    userData["memberId"] = rsvp.Member.Id;
                    var createdPersonResult = await faceClient.CreatePersonAsync(Settings.PersonGroupId, rsvp.Member.Name, userData.ToString());
                    registeredPersons.Add(await faceClient.GetPersonAsync(Settings.PersonGroupId, createdPersonResult.PersonId));
                }
            }
            
            await mediaCapture.StartPreviewAsync();
            Identify();
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
                detectedFaces = await faceDetector.DetectFacesAsync(image);

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
                        continue;
                    try
                    {
                        Person person;

                        var identifyResults = await faceClient.IdentifyAsync(Settings.PersonGroupId, faces.Select(x => x.FaceId).ToArray());
                        if (identifyResults.FirstOrDefault()?.Candidates?.Count() > 0)
                            person = await faceClient.GetPersonAsync(Settings.PersonGroupId, identifyResults.First().Candidates.First().PersonId);
                        else
                            person = await RegisterAsync(stream);

                        // If welcome comment not posted yet, then post it.
                        if (RSVPComments.FirstOrDefault(x => x.CommentDetail.Contains(person.Name)) == null)
                            RSVPComments.Add(await meetupService.PostCommentAsync("Welcome " + person.Name, currentEvent.Id));

                        Message = $"Hi {person.Name}!";
                        SelectedRSVP = null;
                        await Task.Delay(2000);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Register face to Cognitive Face API
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private async Task<Person> RegisterAsync(InMemoryRandomAccessStream stream)
        {
            if (SelectedRSVP == null)
                Message = "Select your RSVP.";

            while (SelectedRSVP == null)
            {
                await Task.Delay(1000);
            }            

            // All the members should be registered when initialized.
            var registeredPerson = registeredPersons.First(x => x.Name == SelectedRSVP.Member.Name);

            // Register face information and discard image.
            var addPersistedFaceResult = await faceClient.AddPersonFaceAsync(
                Settings.PersonGroupId, registeredPerson.PersonId, ImageConverter.ConvertImage(stream));
            stream.Dispose();

            await faceClient.TrainPersonGroupAsync(Settings.PersonGroupId);
            return await faceClient.GetPersonAsync(Settings.PersonGroupId, registeredPerson.PersonId);
        }        
    }
}
