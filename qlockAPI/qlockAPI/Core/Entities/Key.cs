using System;
using System.Collections.Generic;

namespace qlockAPI.Core.Entities;

public partial class Key
{
    public int Id { get; set; }

    public string SecretKey { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? ExpirationDate { get; set; }

    public int RemainingUses { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int UserId { get; set; }

    public int LockId { get; set; }

    public virtual Lock Lock { get; set; } = null!;

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual User User { get; set; } = null!;
}
