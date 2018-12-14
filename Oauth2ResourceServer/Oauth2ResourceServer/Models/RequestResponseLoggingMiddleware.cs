using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Oauth2ResourceServer.Models
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<Startup> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly string OAUTH2_SERVER = "https://oauth2serverapt.azurewebsites.net";
        private readonly string OAUTH2_AUTHORIZATION_URI = "/Oauth2/CheckToken";
        private readonly string SCOPES = "http://basicscope.com,http://songresourcescope.com";

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<Startup> logger, IMemoryCache memoryCache)
        {
            _next = next;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task Invoke(HttpContext context)
        {            
            context.Request.Headers.TryGetValue("Authorization", out StringValues authorizationToken);
            var basicToken = authorizationToken + "";
            // lấy token từ header.
            var accessToken = basicToken.Replace("Basic ", "");
            var cachedCredential = _memoryCache.Get<Credential>(accessToken);
            if (cachedCredential != null && cachedCredential.IsValid(SCOPES))
            {
                _logger.LogInformation("Get token from cache.");
                await _next(context);
                return;
            }
            _logger.LogInformation("Check token from oauth2 api.");
            // Trường hợp ko có trong cache hoặc ko hợp lệ.
            // Kiểm tra từ oauth2 server.
            var client = new HttpClient();
            var result = client.GetAsync(OAUTH2_SERVER + OAUTH2_AUTHORIZATION_URI + "?tokenKey=" + accessToken + "&scopes=" + SCOPES)
                .Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Valid token, save to cache.");
                var newCredential = JsonConvert.DeserializeObject<Credential>(result.Content.ReadAsStringAsync().Result) ;
                _logger.LogInformation("Member access token: " + newCredential.AccessToken);
                _memoryCache.Set(newCredential.AccessToken, newCredential,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(300)));
                await _next(context);
            }
            else
            {
                _logger.LogError("Invalid token, return 403.");
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

    }
}
