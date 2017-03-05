#Speaker Portal
This application does following.
- Get attendee detail by using Meetup API
- Voice to Text feature to capture what attendee says.
- Send/Receive data via SignalR server.

See Meetup API detail [here](https://www.meetup.com/meetup_api/)
See SignalR detail [here](https://docs.microsoft.com/en-us/aspnet/signalr/overview/getting-started/tutorial-getting-started-with-signalr)
See Speech to Text detail [here](https://docs.microsoft.com/en-us/windows/uwp/input-and-devices/speech-recognition)

## Application Detail
### Meetup API
When Attendee Listner sends member id via SignalR, the application gets member detail by calling Meetup API

### Speech To Text
The application uses UWP Speech Recognizer to capture voice data. Starting/Stopping microphone is controlled by clicing buttons.

### SignalR
The application uses SignalR technology to send command to Attendee Listner to start/stop microphone as well as send/receive captured voice to text data.

## Hardware requirement
At least one microphone.
