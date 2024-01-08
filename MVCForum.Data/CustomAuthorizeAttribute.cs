using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SnitzCore.Data.Interfaces;

namespace SnitzCore.Data
{
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly ISnitzConfig _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public string? RegCheck { get; set; }
        public CustomAuthorizeAttribute()
        {

        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            ISnitzConfig _config = context.HttpContext.RequestServices.GetService(typeof(ISnitzConfig)) as ISnitzConfig;
            if (context != null)
            {
                if (RegCheck == "STRPROHIBITNEWMEMBERS" && _config.GetIntValue("STRPROHIBITNEWMEMBERS") == 1)
                {
                    if (!context.HttpContext.User.Identity.IsAuthenticated)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }                
                if (_config.GetIntValue("STRREQUIREREG") == 1)
                {
                    if (!context.HttpContext.User.Identity.IsAuthenticated)
                    {
                        context.Result = new UnauthorizedResult();
                        return;
                    }
                }

            }
        }

    }
}
