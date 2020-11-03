using System;
using System.Threading.Tasks;
using api.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace api.SignalR
{
  public class PresenceHub : Hub
  {
    private readonly PresenceTracker _presenceTracker;

    public PresenceHub(PresenceTracker presenceTracker)
    {
      _presenceTracker = presenceTracker;
    }

    public override async Task OnConnectedAsync()
    {
      var username = Context.User.GetUsername();
      var connectionId = Context.ConnectionId;

      var isOnline = await _presenceTracker.UserConnected(username, connectionId);

      if (isOnline)
      {
        await Clients.Others.SendAsync("UserIsOnline", username);
      }

      var onlineUsers = await _presenceTracker.GetOnlineUsers();
      await Clients.Caller.SendAsync("GetOnlineUsers", onlineUsers);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      var username = Context.User.GetUsername();
      var connectionId = Context.ConnectionId;

      var isOffline = await _presenceTracker.UserDisconnected(username, connectionId);

      if (isOffline)
      {
        await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
      }

      await base.OnDisconnectedAsync(exception);
    }
  }
}
