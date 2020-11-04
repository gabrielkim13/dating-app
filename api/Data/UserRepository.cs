using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;
using api.Entities;
using api.Helpers;
using api.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
  public class UserRepository : IUserRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
    }

    public async Task<MemberDTO> GetMemberAsync(string username)
    {
      return await _context.Users
        .Where(user => user.UserName == username)
        .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
    {
      var query = _context.Users.AsQueryable();

      query = query.Where(u => u.UserName != userParams.CurrentUsername);
      query = query.Where(u => u.Gender == userParams.Gender);

      var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
      var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

      query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

      query = userParams.OrderBy switch
      {
        "created" => query.OrderByDescending(u => u.Created),
        _ => query.OrderByDescending(u => u.LastActive)
      };

      var projectedQuery = query.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider).AsNoTracking();

      return await PagedList<MemberDTO>.CreateAsync(projectedQuery, userParams.PageNumber, userParams.PageSize);
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
      return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
      return await _context.Users.Include(user => user.Photos).SingleOrDefaultAsync(user => user.UserName == username);
    }

    public async Task<string> GetUserGender(string username)
    {
      return await _context.Users
        .Where(user => user.UserName == username)
        .Select(user => user.Gender)
        .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
      return await _context.Users.Include(user => user.Photos).ToListAsync();
    }

    public void Update(AppUser user)
    {
      _context.Entry(user).State = EntityState.Modified;
    }
  }
}
