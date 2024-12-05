using System;
using System.Collections.Generic;

namespace qlockAPI.Core.Entities;

public partial class Group
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AssignGroup> AssignGroups { get; set; } = new List<AssignGroup>();
}
