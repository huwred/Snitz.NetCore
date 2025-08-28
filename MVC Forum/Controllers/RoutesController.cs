using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;


namespace RouteDebugging.Controllers
{
    public class RoutesModel
    {
          private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

          public RoutesModel(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
          {
            this._actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
          }

          public List<RouteInfo> Routes { get; set; } = new();
    }
    public class RouteInfo
  {
    public string Template { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Constraint { get; set; } = string.Empty;
    public string RouteValues { get; set; } = string.Empty;
  }

    //[Route("[controller]/[action]")]
    public class RoutesController : Controller
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public RoutesController(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;

        }

        [HttpGet("/RoutesController")]
        public IActionResult Index()
        {
            var pageRoutes = _actionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<PageActionDescriptor>()
        .Select(x => new RouteInfo
        {
            Action = "",
            Controller = x.DisplayName,
            Name = x.AttributeRouteInfo?.Name,
            Template = x.AttributeRouteInfo?.Template,
            Constraint = x.ActionConstraints == null ? "" : JsonSerializer.Serialize(x.ActionConstraints),
            RouteValues = string.Join(',', x.RouteValues)
        })
    .OrderBy(r => r.Template);

            var viewRoutes = _actionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Select(x => new RouteInfo
                    {
                        Action = x.RouteValues["Action"],
                        Controller = x.RouteValues["Controller"],
                        Name = x.AttributeRouteInfo?.Name,
                        Template = x.AttributeRouteInfo?.Template,
                        Constraint = x.ActionConstraints == null ? "" : JsonSerializer.Serialize(x.ActionConstraints),
                    })
                .OrderBy(r => r.Template);

            var routes = pageRoutes.Concat(viewRoutes).ToList();
            var vm = new RoutesModel(_actionDescriptorCollectionProvider);
            vm.Routes = viewRoutes.ToList();

            return View("Routes",vm);
            //return Json(routes, new JsonSerializerOptions
            //{
            //    WriteIndented = true,
            //});
        }
    }
}
