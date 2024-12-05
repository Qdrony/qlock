using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ApiKeyAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        var expectedApiKey = configuration?["ApiKeys:LockKey"];

        if (!context.HttpContext.Request.Headers.TryGetValue("QLOCK-API-KEY", out var extractedApiKey) || extractedApiKey != expectedApiKey)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
