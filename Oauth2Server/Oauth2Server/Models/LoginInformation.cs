﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Oauth2Server.Models
{
    [NotMapped]
    public class LoginInformation
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string RedirectUrl { get; set; }
    }
}
