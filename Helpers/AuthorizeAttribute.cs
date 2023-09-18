using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PEAS.Entities.Authentication;

namespace PEAS.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly IList<Role> _roles;

        public AuthorizeAttribute(params Role[] roles)
        {
            _roles = roles ?? Array.Empty<Role>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Items["Account"] is not Account account || (_roles.Any() && !_roles.Contains(account.Role)))
            {
                // not logged in or role not authorized
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}