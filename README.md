#Attendee Analyzer
###This is Microsoft Cognitive Service sample application for Meetup.com events.
日本語のブログ: https://blogs.msdn.microsoft.com/kenakamu/2017/03/05/use-microsoft-cognitive-service-to-optimize-meetup-event/

See the video for demo starting at 46:29 [Tokyo Azure Meetup #11 - Building Intelligent Applications ](https://www.youtube.com/watch?v=UH3oJ-gfze8)
Detail about Microsoft Cognitive Server. [Cognitive Service](https://www.microsoft.com/cognitive-services/en-us/)

**To make it easier to use the sample, I removed Microsoft Dynamics 365 dependency and use Meetup event comment feature to track who came to the event and QA.**

##How to use the sample
To use the sample, you need to get following keys and set it to AttendeeAnalyzerPCL\Settings.cs file.

###Meetup related
Meetup Group Name: Set the meetup group name of your interest by url name, such as Tokyo-about_PowerShell-Meetup
Meetup API Key: You can get Meetup API Key from https://secure.meetup.com/meetup_api/key/ after signin to meetup.com

###Cognitive related
Person Group Id: You can decide whatever name you want. This is used to create a group in Face Cognitive API to store user and their faces. All lowercase required.
Emotion Key and Face Key: If you have Azure Subscription, you can request key from [Azure Porta](https://portal.azure.com), or you can go to [Trial Site](https://www.microsoft.com/cognitive-services/en-US/subscriptions?mode=NewTrials) to apply for trial.

##Solution Structure
This sample application contains following projects. See the readme.md at each projects for more detail.
- AttendeeAnalyzerPCL: Contains shared model and service for other projects.
- AttendeeAnalyzerUWPShared: Contains shared logic for three UWP applications.
- AttendeeConnector: SignalR server, which relays voice-to-text data and command requests between Listner and Spaker Portal applications.
- AttendeeListener: Set this application for attendee who asks question(s). This application detect the attendee, get emotion and what s/he says, then propagate the data to Speaker Portal application.
- AttendeeRegister: Place this application at the gate of the event room. This application get event and RSVP data from meetup.com, and recognize who actually comes to the event as form of comment in the event.
- SpeakerPortal: Set this application in front of meetup speaker. This application controls when speaker or attendee speaks, and generate text from voices. It also get data from Attendee Listner so the speaker can know who is asking the question right now. It also saves question/answer to Meetup.com as comment in the evnet.

##License
MIT License.
