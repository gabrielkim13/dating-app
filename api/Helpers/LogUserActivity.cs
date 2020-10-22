using System;
using System.Threading.Tasks;
using api.Extensions;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace api.Helpers
{
  public class LogUserActivity : IAsyncActionFilter
  {
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      var resultContext = await next();

      if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

      var userId = resultContext.HttpContext.User.GetUserId();
      var repo = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();

      var user = await repo.GetUserByIdAsync(userId);
      user.LastActive = DateTime.Now;

      await repo.SaveAllAsync();
    }
  }
}
