using System.Security.Claims;

namespace api.Extensions
{
  public static class PrincipalExtensions
  {
    public static string GetUsername(this ClaimsPrincipal user)
    {
      return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
  }
}
