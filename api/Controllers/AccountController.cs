using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using api.Data;
using api.DTOs;
using api.Entities;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
      _mapper = mapper;
      _tokenService = tokenService;
      _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
    {
      if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

      using var hmac = new HMACSHA512();

      var user = _mapper.Map<AppUser>(registerDto);

      user.UserName = registerDto.Username.ToLower();
      user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
      user.PasswordSalt = hmac.Key;

      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      return new UserDTO
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user),
        KnownAs = user.KnownAs
      };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
    {
      var user = await _context.Users
        .Include(user => user.Photos)
        .SingleOrDefaultAsync(user => user.UserName == loginDto.Username);

      if (user == null) return Unauthorized("Invalid username");

      using var hmac = new HMACSHA512(user.PasswordSalt);

      var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

      if (!user.PasswordHash.SequenceEqual(hash)) return Unauthorized("Invalid password");

      return new UserDTO
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user),
        PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain)?.Url,
        KnownAs = user.KnownAs,
      };
    }

    private async Task<bool> UserExists(string username)
    {
      return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
    }
  }
}
