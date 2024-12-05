using System;
using System.Collections.Generic;

namespace qlockAPI.Core.Entities;

public partial class Log
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? LockId { get; set; }

    public int? KeyId { get; set; }

    public DateTime? Time { get; set; }

    public string Action { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Text { get; set; }

    public virtual Key? Key { get; set; }

    public virtual Lock? Lock { get; set; }

    public virtual User? User { get; set; }
}
