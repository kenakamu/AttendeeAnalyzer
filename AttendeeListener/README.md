#Attendee Listner
This application does following.
- Retrieve/Create face data to Face Cognitive API to identify who is in front of the camera.
- See the emotion of the attendee.
- Voice to Text feature to capture what attendee says.
- Send/Receive data via SignalR server.

See Face cognitive API detail [here](https://www.microsoft.com/cognitive-services/en-us/face-api)
See Emotion cognitive API detail [here](https://www.microsoft.com/cognitive-services/en-us/emotion-api)
See SignalR detail [here](https://docs.microsoft.com/en-us/aspnet/signalr/overview/getting-started/tutorial-getting-started-with-signalr)
See Speech to Text detail [here](https://docs.microsoft.com/en-us/windows/uwp/input-and-devices/speech-recognition)

## Application Detail
### Face API
When launched, it starts camera and looks at people in front of camera. Once it detects a face, it queries to Face API group data to identify who the attendee is. 
Then send the data to SignalR server.

### Emotion API
Once it detects the face, it starts monitoring the emotion of the attendee, and send the data to SignalR server.

### Speech To Text
The application uses UWP Speech Recognizer to capture voice data. Starting/Stopping microphone is controlled by Speaker Portal.

## Hardware requirement
At least one camera and one microphone.
