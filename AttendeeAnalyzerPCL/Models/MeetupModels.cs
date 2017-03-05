using Newtonsoft.Json;
using System.Collections.Generic;

namespace AttendeeAnalyzer.Meetup.Models
{
    public class RSVP
    {
        [JsonProperty("created")]
        public long Created { get; set; }
        [JsonProperty("updated")]
        public long Updated { get; set; }
        [JsonProperty("response")]
        public string Response { get; set; }
        [JsonProperty("guests")]
        public int Guests { get; set; }
        [JsonProperty("_event")]
        public Event Event { get; set; }
        [JsonProperty("group")]
        public Group Group { get; set; }
        [JsonProperty("member")]
        public Member Member { get; set; }
        [JsonProperty("venue")]
        public Venue Venue { get; set; }
    }

    public class Event
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("yes_rsvp_count")]
        public int Yes_RSVP_Count { get; set; }
        [JsonProperty("time")]
        public long Time { get; set; }
        [JsonProperty("utc_offset")]
        public int Utc_Offset { get; set; }
    }

    public class Group
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("urlname")]
        public string UrlName { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("who")]
        public string Who { get; set; }
        [JsonProperty("members")]
        public int Members { get; set; }
        [JsonProperty("join_mode")]
        public string Join_Mode { get; set; }
        [JsonProperty("topics")]
        public List<Topic> Topics { get; set; }
        [JsonProperty("category")]
        public Category Category { get; set; }
    }

    public class Member
    {
        [JsonProperty("event_context")]
        public Event_Context Event_Context { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("topics")]
        public Topic[] Topics { get; set; }
        [JsonProperty("joined")]
        public long Joined { get; set; }
        [JsonProperty("link")]
        public string Link { get; set; }
        [JsonProperty("photo")]
        public Photo Photo { get; set; }
        [JsonProperty("lon")]
        public float Lon { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("visited")]
        public long Visited { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("lat")]
        public float Lat { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("bio")]
        public string Bio { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }
    }

    public class Photo
    {
        [JsonProperty("highres_link")]
        public string Highres_Link { get; set; }
        [JsonProperty("photo_id")]
        public int Id { get; set; }
        [JsonProperty("base_url")]
        public string Base_Url { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("photo_link")]
        public string Photo_Link { get; set; }
        [JsonProperty("thumb_link")]
        public string Thumb_Link { get; set; }
    }    

    public class Topic
    {
        [JsonProperty("urlkey")]
        public string UrlKey { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
    }    

    public class Event_Context
    {
        [JsonProperty("host")]
        public bool Host { get; set; }
    }

    public class Venue
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("lat")]
        public float Lat { get; set; }
        [JsonProperty("lon")]
        public float Lon { get; set; }
        [JsonProperty("repinned")]
        public bool Repinned { get; set; }
        [JsonProperty("address_1")]
        public string Address { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("localized_country_name")]
        public string Localized_Country_Name { get; set; }
    }

    public class Category
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Comment
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("comment")]
        public string CommentDetail { get; set; }
        [JsonProperty("link")]
        public string Link { get; set; }
        [JsonProperty("like_count")]
        public int Like_Count { get; set; }
        [JsonProperty("member")]
        public Member Member { get; set; }
    }
}
