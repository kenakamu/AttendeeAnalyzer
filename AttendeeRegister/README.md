#Attendee Register
This application does following.
- Retreive/Create meetup related information such as event data, RSVP, comments and members data.
- Retrieve/Create face data to Face Cognitive API to identify who is in front of the camera.

See Meetup API detail [here](https://www.meetup.com/meetup_api/)
See Face cognitive API detail [here](https://www.microsoft.com/cognitive-services/en-us/face-api)

## Application Detail
### RSVP data
When launched, it fetches current meetup event and it's RSVP data. Then list the members.

### Face API
After RSVP data obtained, it starts camera and looks at people in front of camera. Once it detects a face, it queries to Face API group data to identify who the attendee is. 
If it cannot detect who the attendee is, it asked to select RSVP data and registers the face to Face API by mapping to the user.

### Event comment
When the application recognize who is the attendee, it writes "welcome" comment in the event page.

## Hardware requirement
At least one camera. If the device has multiple camera.

