using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Website.Infrustructure.DataSource;

namespace Website.Infrustructure.Models
{
    public class Asset : Poco
    {
        [Required]
        public int UserAuthId { get; set; }
        [Required]
        [StringLength(240)]
        public string Title { get; set; }
        [Required]
        [StringLength(500)]
        public string FileUrl { get; set; }
        public string Tags { get; set; }

        public User User()
        {
            return Repository().Find<User>(u => u.UserAuthId == UserAuthId).First();
        }

        public List<string> TagList()
        {
            List<string> tagList = new List<string>();
            if (!string.IsNullOrEmpty(Tags))
            {
                foreach (string tag in Tags.Split(','))
                {
                    tagList.Add(tag.Trim().Replace(" ", "-").ToLower());
                }
            }
            return tagList;
        }
    }
}