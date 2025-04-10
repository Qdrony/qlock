using System;
using System.Collections.Generic;

namespace qlockAPI.Core.Entities;

public partial class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int LockId { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
}
