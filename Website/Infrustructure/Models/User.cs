using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Website.Infrustructure.DataSource;

namespace Website.Infrustructure.Models
{
    public class User : Poco
    {
        [Required]
        public int UserAuthId { get; set; }
        [Required]
        [StringLength(40)]
        public string UserName { get; set; }
        [Required]
        [StringLength(40)]
        public string ApiKey { get; set; }
        public string AvatarUrl { get; set; }
        public string DisplayName { get; set; }
        public string ProfileUrl { get; set; }

        // Required for Twitter/Facebook sign ins:
        public string TwitterUserId { get; set; }
        public string TwitterScreenName { get; set; }
        public string TwitterName { get; set; }
        public string FacebookName { get; set; }
        public string FacebookFirstName { get; set; }
        public string FacebookLastName { get; set; }
        public string FacebookUserId { get; set; }
        public string FacebookUserName { get; set; }
        public string FacebookEmail { get; set; }
    }
}