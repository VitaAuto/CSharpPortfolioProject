using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlaywrightProject.API.Models;

namespace PlaywrightProject.API.Context
{
    public class UsersApiContext
    {
        public User User { get; set; }
        public int UserId { get; set; }
        public User OtherUser { get; set; }
        public int OtherUserId { get; set; }
        public RestResponse Response { get; set; }
        public List<int> CreatedUserIds { get; set; } = new();

        public UsersApiClient ApiClient { get; set; }
    }
}
