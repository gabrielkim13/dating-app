using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.SignalR
{
  public class PresenceTracker
  {
    private static readonly Dictionary<string, List<string>> OnlineUsers =
      new Dictionary<string, List<string>>();

    public Task<bool> UserConnected(string username, string connectionId)
    {
      bool isOnline = false;

      lock (OnlineUsers)
      {
        if (OnlineUsers.ContainsKey(username))
        {
          OnlineUsers[username].Add(connectionId);
        }
        else
        {
          isOnline = true;

          OnlineUsers.Add(username, new List<string> { connectionId });
        }
      }

      return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
      bool isOffline = false;

      lock (OnlineUsers)
      {
        if (OnlineUsers.ContainsKey(username)) return Task.FromResult(false);

        OnlineUsers[username].Remove(connectionId);

        if (OnlineUsers[username].Count == 0)
        {
          isOffline = true;

          OnlineUsers.Remove(username);
        }
      }

      return Task.FromResult(isOffline);
    }

    public Task<string[]> GetOnlineUsers()
    {
      var onlineUsers = new string[] { };

      lock (OnlineUsers)
      {
        onlineUsers = OnlineUsers.OrderBy(user => user.Key).Select(user => user.Key).ToArray();
      }

      return Task.FromResult(onlineUsers);
    }

    public Task<List<string>> GetConnectionsForUser(string username)
    {
      var connectionIds = new List<string>();

      lock (OnlineUsers)
      {
        connectionIds = OnlineUsers.GetValueOrDefault(username);
      }

      return Task.FromResult(connectionIds);
    }
  }
}
