using System;
using System.Collections.Generic;

namespace qlockAPI.Core.Entities;

public partial class Lock
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int Owner { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Key> Keys { get; set; } = new List<Key>();

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual User OwnerNavigation { get; set; } = null!;
}
