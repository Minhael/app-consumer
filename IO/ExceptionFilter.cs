using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace App.WebApi.Application.Http;

//  https://andrewlock.net/using-cancellationtokens-in-asp-net-core-mvc-controllers/
public class ExceptionFilter : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var routeData = context.RouteData;
        var controllerName = routeData.Values["controller"];
        var actionName = routeData.Values["action"];
        var ex = context.Exception;

        var payload = new
        {
            Error = ex.Message,
            StackTrace = ex.ToString()
        };

        switch (ex)
        {
            case TaskCanceledException tce:
                // context.Result = new StatusCodeResult(499);
                context.ExceptionHandled = true;
                break;
            case OperationCanceledException oce:
                // context.Result = new StatusCodeResult(499);
                context.ExceptionHandled = true;
                break;
            case ArgumentException ae:
                context.Result = new ObjectResult(payload)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                context.ExceptionHandled = true;
                break;
            default:
                context.Result = new ObjectResult(payload)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
                context.ExceptionHandled = true;
                break;
        }
    }
}