using System;
using System.Collections.Generic;

namespace qlockAPI.Core.Entities;

public partial class AssignGroup
{
    public int UserId { get; set; }

    public int LockId { get; set; }

    public int GroupId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Assign Assign { get; set; } = null!;

    public virtual Group Group { get; set; } = null!;
}
