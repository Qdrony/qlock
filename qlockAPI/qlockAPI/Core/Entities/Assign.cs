using System;
using System.Collections.Generic;

namespace qlockAPI.Core.Entities;

public partial class Assign
{
    public int LockId { get; set; }

    public int UserId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual ICollection<AssignGroup> AssignGroups { get; set; } = new List<AssignGroup>();

    public virtual Lock Lock { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
