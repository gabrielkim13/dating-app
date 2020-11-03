using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace api.Entities
{
  public class Group
  {
    public Group()
    {
    }

    public Group(string name)
    {
      this.Name = name;
    }

    [Key]
    public string Name { get; set; }
    public ICollection<Connection> Connections { get; set; } = new List<Connection>();
  }
}
