using AuthServer.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace AuthServer.Infrastructure.Attributes
{
    public class ActionValidatorAttribute : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ModelState.IsValid)
                return;

            var result = new ContentResult
            {
                ContentType = "application/json"
            };

            List<string> errors = filterContext.ModelState.GetErrorsList();
            result.Content = JsonConvert.SerializeObject(new { errors });

            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            filterContext.Result = result;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}