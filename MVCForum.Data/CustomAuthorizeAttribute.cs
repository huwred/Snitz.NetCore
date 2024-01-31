using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace SnitzCore.Data
{
    /// <summary>
    /// Specifies that the class or method this attribute is applied to
    /// requires STRPROHIBITNEWMEMBERS or STRREQUIREREG to restrict access
    /// 
    /// </summary>
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public string? RegCheck { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.IsInRole("Admin"))
            {
                return;
            }

            ISnitzConfig? _config = context.HttpContext.RequestServices.GetService(typeof(ISnitzConfig)) as ISnitzConfig;
            if (_config != null)
            {
                if (RegCheck == "STRPROHIBITNEWMEMBERS" && _config.GetIntValue("STRPROHIBITNEWMEMBERS") == 1)
                {
                    if (!context.HttpContext.User.Identity!.IsAuthenticated)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }                
                if (_config.GetIntValue("STRREQUIREREG") == 1)
                {
                    if (!context.HttpContext.User.Identity!.IsAuthenticated)
                    {
                        context.Result = new UnauthorizedResult();
                    }
                }
            }
        }
    }
}
