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

    public void AddGroup(Group group)
    {
      _context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
      _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
      _context.Messages.Remove(message);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
      return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
      return await _context.Groups
        .Include(group => group.Connections)
        .FirstOrDefaultAsync(group => group.Connections.Any(connection => connection.ConnectionId == connectionId));
    }

    public async Task<Message> GetMessage(int id)
    {
      return await _context.Messages.FindAsync(id);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
      return await _context.Groups
        .Include(group => group.Connections)
        .FirstOrDefaultAsync(group => group.Name == groupName);
    }

    public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessagesParams messagesParams)
    {
      var query = _context.Messages
        .OrderByDescending(x => x.DateSent)
        .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider)
        .AsQueryable();

      query = messagesParams.Container switch
      {
        "Inbox" => query.Where(x => x.RecipientUsername == messagesParams.Username && !x.RecipientDeleted),
        "Outbox" => query.Where(x => x.SenderUsername == messagesParams.Username && !x.SenderDeleted),
        _ => query.Where(x => x.RecipientUsername == messagesParams.Username && x.DateRead == null && !x.RecipientDeleted)
      };

      return await PagedList<MessageDTO>.CreateAsync(query, messagesParams.PageNumber, messagesParams.PageSize);
    }

    public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
    {
      var messages = await _context.Messages
        .Where(x => (x.Sender.UserName == currentUsername && x.Recipient.UserName == recipientUsername && !x.SenderDeleted) ||
                    (x.Recipient.UserName == currentUsername && x.Sender.UserName == recipientUsername && !x.RecipientDeleted))
        .OrderBy(x => x.DateSent)
        .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider)
        .ToListAsync();

      var unreadMessages = messages.Where(x => x.RecipientUsername == currentUsername && x.DateRead == null).ToList();

      if (unreadMessages.Any())
      {
        foreach (var message in unreadMessages)
        {
          message.DateRead = DateTime.UtcNow;
        }
      }

      return messages;
    }

    public void RemoveConnection(Connection connection)
    {
      _context.Connections.Remove(connection);
    }
  }
}
