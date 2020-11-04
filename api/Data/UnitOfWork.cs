using System.Threading.Tasks;
using api.Interfaces;
using AutoMapper;

namespace api.Data
{
  public class UnitOfWork : IUnitOfWork
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UnitOfWork(DataContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public IUserRepository UserRepository => new UserRepository(_context, _mapper);

    public IMessagesRepository MessagesRepository => new MessagesRepository(_context, _mapper);

    public ILikesRepository LikesRepository => new LikesRepository(_context, _mapper);

    public async Task<bool> Complete()
    {
      return await _context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
      return _context.ChangeTracker.HasChanges();
    }
  }
}
