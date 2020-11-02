using System;
using System.Collections;
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
  public class MessagesRepository : IMessagesRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessagesRepository(DataContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public void AddMessage(Message message)
    {
      _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
      _context.Messages.Remove(message);
    }

    public async Task<Message> GetMessage(int id)
    {
      return await _context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessagesParams messagesParams)
    {
      var query = _context.Messages
        .OrderByDescending(x => x.DateSent)
        .AsQueryable();

      query = messagesParams.Container switch
      {
        "Inbox" => query.Where(x => x.Recipient.UserName == messagesParams.Username && !x.RecipientDeleted),
        "Outbox" => query.Where(x => x.Sender.UserName == messagesParams.Username && !x.SenderDeleted),
        _ => query.Where(x => x.Recipient.UserName == messagesParams.Username && x.DateRead == null && !x.RecipientDeleted)
      };

      var messages = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

      return await PagedList<MessageDTO>.CreateAsync(messages, messagesParams.PageNumber, messagesParams.PageSize);
    }

    public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
    {
      var messages = await _context.Messages
        .Include(x => x.Sender).ThenInclude(x => x.Photos)
        .Include(x => x.Recipient).ThenInclude(x => x.Photos)
        .Where(x => (x.Sender.UserName == currentUsername && x.Recipient.UserName == recipientUsername && !x.SenderDeleted) ||
                    (x.Recipient.UserName == currentUsername && x.Sender.UserName == recipientUsername && !x.RecipientDeleted))
        .OrderBy(x => x.DateSent)
        .ToListAsync();

      var unreadMessages = messages.Where(x => x.Recipient.UserName == currentUsername && x.DateRead == null).ToList();

      if (unreadMessages.Any())
      {
        foreach (var message in unreadMessages)
        {
          message.DateRead = DateTime.Now;
        }

        await _context.SaveChangesAsync();
      }

      return _mapper.Map<IEnumerable<MessageDTO>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
      return await _context.SaveChangesAsync() > 0;
    }
  }
}
