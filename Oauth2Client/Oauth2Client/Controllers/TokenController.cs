using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Oauth2Client.Controllers
{
    public class TokenController : Controller
    {
        public string Exchange(string code)
        {
            return "Code: " + code;
        }
    }
}