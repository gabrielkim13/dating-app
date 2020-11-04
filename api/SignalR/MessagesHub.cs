using System;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace api.SignalR
{
  public class MessagesHub : Hub
  {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHubContext;
    private readonly PresenceTracker _presenceTracker;

    public MessagesHub(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<PresenceHub> presenceHubContext,
                       PresenceTracker presenceTracker)
    {
      _unitOfWork = unitOfWork;
      _mapper = mapper;
      _presenceHubContext = presenceHubContext;
      _presenceTracker = presenceTracker;
    }

    public override async Task OnConnectedAsync()
    {
      var sender = Context.User.GetUsername();
      var connectionId = Context.ConnectionId;
      var httpContext = Context.GetHttpContext();

      var recipient = httpContext.Request.Query["user"].ToString();

      var groupName = GetGroupName(sender, recipient);

      await Groups.AddToGroupAsync(connectionId, groupName);

      var group = await AddToGroup(groupName);
      await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

      var messageThread = await _unitOfWork.MessagesRepository.GetMessageThread(sender, recipient);

      if (_unitOfWork.HasChanges()) await _unitOfWork.Complete();

      await Clients.Caller.SendAsync("ReceiveMessageThread", messageThread);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      var group = await RemoveFromMessageGroup();
      await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

      await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDTO createMessageDTO)
    {
      var username = Context.User.GetUsername();

      if (username == createMessageDTO.RecipientUsername.ToLower())
      {
        throw new HubException("You cannot message yourself");
      }

      var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
      var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername.ToLower());

      if (recipient == null)
      {
        throw new HubException("User not found");
      }

      var message = new Message
      {
        SenderId = sender.Id,
        SenderUsername = sender.UserName,
        RecipientId = recipient.Id,
        RecipientUsername = recipient.UserName,
        Content = createMessageDTO.Content
      };

      var groupName = GetGroupName(sender.UserName, recipient.UserName);
      var group = await _unitOfWork.MessagesRepository.GetMessageGroup(groupName);

      if (group.Connections.Any(connection => connection.Username == recipient.UserName))
      {
        message.DateRead = DateTime.UtcNow;
      }
      else
      {
        var connections = await _presenceTracker.GetConnectionsForUser(recipient.UserName);

        if (connections != null)
        {
          await _presenceHubContext.Clients.Clients(connections).SendAsync("NewMessageReceived", new
          {
            username = sender.UserName,
            knownAs = sender.KnownAs
          });
        }
      }

      _unitOfWork.MessagesRepository.AddMessage(message);

      if (await _unitOfWork.Complete())
      {
        await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDTO>(message));
      }
      else
      {
        throw new HubException("Failed to save message");
      }
    }

    private async Task<Group> AddToGroup(string groupName)
    {
      var username = Context.User.GetUsername();
      var connectionId = Context.ConnectionId;

      var group = await _unitOfWork.MessagesRepository.GetMessageGroup(groupName);

      var connection = new Connection(connectionId, username);

      if (group == null)
      {
        group = new Group(groupName);
        _unitOfWork.MessagesRepository.AddGroup(group);
      }

      group.Connections.Add(connection);

      if (await _unitOfWork.Complete()) return group;

      throw new HubException("Failed to join group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
      var username = Context.User.GetUsername();
      var connectionId = Context.ConnectionId;

      var group = await _unitOfWork.MessagesRepository.GetGroupForConnection(connectionId);
      var connection = group.Connections.FirstOrDefault(connection => connection.ConnectionId == connectionId);
      _unitOfWork.MessagesRepository.RemoveConnection(connection);

      if (await _unitOfWork.Complete()) return group;

      throw new HubException("Failed to remove from group");
    }

    private string GetGroupName(string sender, string recipient)
    {
      var stringCompare = string.Compare(sender, recipient) < 0;

      return stringCompare ? $"{sender}-{recipient}" : $"{recipient}-{sender}";
    }
  }
}
