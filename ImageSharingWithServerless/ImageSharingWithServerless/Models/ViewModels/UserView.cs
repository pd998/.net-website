﻿using System.ComponentModel.DataAnnotations;

namespace ImageSharingWithServerless.Models
{
    public class UserView
    {
        public string Id { get; set; }

        [Required]
        [RegularExpression(@"[a-zA-Z0-9_]+")]
        public String UserName { get; set; }

        public String Password { get; set; }

        [Required]
        public bool ADA { get; set; }
    }
}