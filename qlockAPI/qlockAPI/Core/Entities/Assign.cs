using System;
using System.Collections.Generic;

namespace qlockAPI.Core.Entities;

public partial class Assign
{
    public int GroupId { get; set; }

    public int UserId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Group Group { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
