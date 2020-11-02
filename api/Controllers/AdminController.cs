using System.Linq;
using System.Threading.Tasks;
using api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
  public class AdminController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager)
    {
      _userManager = userManager;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
      var usersWithRoles = await _userManager.Users
        .Include(user => user.UserRoles)
        .ThenInclude(userRole => userRole.Role)
        .OrderBy(user => user.UserName)
        .Select(user => new
        {
          user.Id,
          Username = user.UserName,
          Roles = user.UserRoles.Select(role => role.Role.Name).ToList()
        })
        .ToListAsync();

      return Ok(usersWithRoles);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles([FromRoute] string username, [FromQuery] string roles)
    {
      var selectedRoles = roles.Split(",").ToArray();

      var user = await _userManager.FindByNameAsync(username);

      if (user == null) return NotFound("User not found");

      var userRoles = await _userManager.GetRolesAsync(user);

      var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

      if (!result.Succeeded) return BadRequest(result.Errors);

      result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

      if (!result.Succeeded) return BadRequest(result.Errors);

      return Ok(await _userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "PhotoModerateRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {
      return Ok();
    }
  }
}
