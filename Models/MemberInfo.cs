using System;
using System.Collections.Generic;

namespace FindMyKids.TeamService.Models
{
    public class MemberInfo
    {
        public Guid ID { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Date { get; set; }
        public string Duedate { get; set; }
        public List<plan> plans { get; set; }
        public List<child> children { get; set; }
    }

    public class plan
    {
        public string id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public string create_time { get; set; }
        public string price { get; set; }
        public List<link> links { get; set; }
    }

    public class link
    {
        public string href { get; set; }
        public string rel { get; set; }
        public string method { get; set; }
    }

    public class child
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}