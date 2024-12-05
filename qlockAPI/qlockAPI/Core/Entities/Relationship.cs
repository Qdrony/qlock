using System;
using System.Collections.Generic;

namespace qlockAPI.Core.Entities;

public partial class Relationship
{
    public int UserId { get; set; }

    public int FriendId { get; set; }

    public string Type { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User Friend { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
