using System;
using System.Collections.Generic;

namespace qlockAPI.Core.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? LastLogin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Assign> Assigns { get; set; } = new List<Assign>();

    public virtual ICollection<Key> Keys { get; set; } = new List<Key>();

    public virtual ICollection<Lock> Locks { get; set; } = new List<Lock>();

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<Relationship> RelationshipFriends { get; set; } = new List<Relationship>();

    public virtual ICollection<Relationship> RelationshipUsers { get; set; } = new List<Relationship>();
}
