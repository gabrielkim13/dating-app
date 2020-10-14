using api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
  public class BuggyController : BaseApiController
  {
    private readonly DataContext _context;
    public BuggyController(DataContext context)
    {
      _context = context;
    }

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret()
    {
      return "secret text";
    }

    [HttpGet("not-found")]
    public ActionResult<string> GetNotFound()
    {
      return NotFound();
    }

    [HttpGet("server-error")]
    public ActionResult<string> GetServerError()
    {
      var thing = _context.Users.Find(-1);

      var thingString = thing.ToString();

      return Ok(thingString);
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
      return BadRequest("Bad request!");
    }
  }
}