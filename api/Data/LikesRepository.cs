using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Helpers;
using api.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
  public class LikesRepository : ILikesRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public LikesRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
    }

    public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
    {
      return await _context.Likes.FindAsync(sourceUserId, likedUserId);
    }

    public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
    {
      var usersQuery = _context.Users.OrderBy(x => x.UserName).AsQueryable();
      var likesQuery = _context.Likes.AsQueryable();

      if (likesParams.Predicate == "liked")
      {
        likesQuery = likesQuery.Where(x => x.SourceUserId == likesParams.UserId);
        usersQuery = likesQuery.Select(x => x.LikedUser);
      }

      if (likesParams.Predicate == "likedBy")
      {
        likesQuery = likesQuery.Where(x => x.LikedUserId == likesParams.UserId);
        usersQuery = likesQuery.Select(x => x.SourceUser);
      }

      var likedUsers = usersQuery.Select(x => new LikeDTO
      {
        Id = x.Id,
        Username = x.UserName,
        KnownAs = x.KnownAs,
        Age = x.DateOfBirth.CalculateAge(),
        PhotoUrl = x.Photos.FirstOrDefault(x => x.IsMain).Url,
        City = x.City,
      });

      return await PagedList<LikeDTO>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<AppUser> GetUserWithLikes(int userId)
    {
      return await _context.Users
        .Include(x => x.LikedUsers)
        .FirstOrDefaultAsync(x => x.Id == userId);
    }
  }
}
