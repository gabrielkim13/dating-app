namespace api.Helpers
{
  public class MessagesParams : PaginationParams
  {
    public string Username { get; set; }
    public string Container { get; set; } = "Unread";
  }
}
