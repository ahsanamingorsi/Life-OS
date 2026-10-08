using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace LifeOS.MVC.Filters;
public sealed class ValidateFormFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method) || context.ModelState.IsValid) return;
        if (context.Controller is not Controller controller) return;
        var messages = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Check the entered values." : e.ErrorMessage).Distinct();
        controller.TempData["Error"] = string.Join(" ", messages);
        var fallback = "/" + context.RouteData.Values["controller"];
        var referer = context.HttpContext.Request.Headers.Referer.ToString();
        var target = Uri.TryCreate(referer, UriKind.Absolute, out var uri) && uri.Authority == context.HttpContext.Request.Host.Value && controller.Url.IsLocalUrl(uri.PathAndQuery) ? uri.PathAndQuery : fallback;
        context.Result = new LocalRedirectResult(target);
    }
    public void OnActionExecuted(ActionExecutedContext context) { }
}
