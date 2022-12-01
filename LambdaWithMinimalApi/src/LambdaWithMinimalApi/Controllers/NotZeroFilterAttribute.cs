using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LambdaWithMinimalApi.Controllers;

[AttributeUsage(AttributeTargets.Method)]
public class NotZeroFilterAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var parsed = int.TryParse(context.ActionArguments["y"]?.ToString(), out var y);
        if (!parsed)
        {
            context.Result = new BadRequestObjectResult("y must be int");
            return;
        }

        if (y == 0)
        {
            context.Result = new BadRequestObjectResult("y cannot be 0");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}