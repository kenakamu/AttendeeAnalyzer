using AttendeeAnalyzer.Meetup.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AttendeeAnalyzer.Services
{
    public class MeetupService
    {
        private string eventEndpoint = "https://api.meetup.com/{0}/events?key={1}&sign=true&photo-host=public&page=1";
        private string rsvpEndpoint = "https://api.meetup.com/{0}/events/{1}/rsvps?key={2}&sign=true&photo-host=public";
        private string memberEndpoint = "https://api.meetup.com/2/member/{0}?&sign=true&photo-host=public&key={1}";
        private string membersEndpoint = "https://api.meetup.com/2/members?&sign=true&group_urlname={0}&key={1}";
        private string groupByMemberEndpoint = "https://api.meetup.com/2/groups?member_id={0}&format=json&photo-host=public&page=500&fields=&order=id&desc=false&key={1}";
        private string commentEndpoint = "https://api.meetup.com/{0}/events/{1}/comments?key={2}&sign=true";
        private string commentPostEndpoint = "https://api.meetup.com/{0}/events/{1}/comments?comment={2}&key={3}&sign=true";

        public async Task<Event> GetCurrentEventAsync()
        {
            using (HttpClient client = GetClient())
            {
                var result = await client.GetAsync(string.Format(eventEndpoint, Settings.GroupName, Settings.MeetupAPIKey));
                if (result.IsSuccessStatusCode)
                {
                    List<Event> events = JsonConvert.DeserializeObject<List<Event>>(await result.Content.ReadAsStringAsync());
                    return events.FirstOrDefault();
                }
                else
                    return null;
            }
        }

        public async Task<List<RSVP>> GetRSVPsAsync(string eventId)
        {
            using (HttpClient client = GetClient())
            {
                var result = await client.GetAsync(string.Format(rsvpEndpoint, Settings.GroupName, eventId, Settings.MeetupAPIKey));
                if (result.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<List<RSVP>>(await result.Content.ReadAsStringAsync());
                else
                    return null;
            }
        }

        public async Task<Member> GetMemberAsync(string id)
        {
            using (HttpClient client = GetClient())
            {
                var result = await client.GetAsync(string.Format(memberEndpoint, id, Settings.MeetupAPIKey));
                if (result.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<Member>(await result.Content.ReadAsStringAsync());
                else
                    return null;
            }
        }

        public async Task<List<Member>> GetMembersAsync()
        {
            using (HttpClient client = GetClient())
            {
                var members = new List<Member>();
                var url = string.Format(membersEndpoint, Settings.GroupName, Settings.MeetupAPIKey);
                while (!string.IsNullOrEmpty(url))
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = JObject.Parse(await response.Content.ReadAsStringAsync());                        
                        members.AddRange(JsonConvert.DeserializeObject<List<Member>>(result["results"].ToString()));
                        url = result["meta"]["next"].ToString();
                    }
                    else
                        return null;
                }

                return members;
            }
        }

        public async Task<List<Group>> GetGroupsOfMemberAsync(string id)
        {
            using (HttpClient client = GetClient())
            {
                var result = await client.GetAsync(string.Format(groupByMemberEndpoint, id, Settings.MeetupAPIKey));
                if (result.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<List<Group>>(JToken.Parse(await result.Content.ReadAsStringAsync())["results"].ToString());
                else
                    return new List<Group>();
            }
        }

        public async Task<List<Comment>> GetCommentsAsync(string eventId)
        {
            using (HttpClient client = GetClient())
            {
                var result = await client.GetAsync(string.Format(commentEndpoint, Settings.GroupName, eventId, Settings.MeetupAPIKey));
                if (result.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<List<Comment>>(await result.Content.ReadAsStringAsync());
                else
                    return new List<Comment>();
            }
        }

        public async Task<Comment> PostCommentAsync(string comment, string eventId)
        {
            using (HttpClient client = GetClient())
            {
                var result = await client.PostAsync(string.Format(commentPostEndpoint, Settings.GroupName, eventId, comment, Settings.MeetupAPIKey), null);
                if (result.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<Comment>(await result.Content.ReadAsStringAsync());
                else
                    return null;
            }
        }
        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            return client;
        }
    }
}
