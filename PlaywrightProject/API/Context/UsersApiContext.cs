using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlaywrightProject.API.Models;
using PlaywrightProject.API.ApiClient;
using PlaywrightProject.API.Services;

namespace PlaywrightProject.API.Context
{
    public class UsersApiContext
    {
        public User User { get; set; } = default!;
        public User OtherUser { get; set; } = default!;
        public int UserId { get; set; }
        public int OtherUserId { get; set; }
        public List<int> CreatedUserIds { get; set; } = [];
        public RestResponse Response { get; set; } = default!;
    }
}
