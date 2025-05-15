using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Entities;

namespace qlockAPI.Core.Database;

public partial class QlockContext : DbContext
{
    public QlockContext()
    {
    }

    public QlockContext(DbContextOptions<QlockContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public virtual DbSet<Key> Keys { get; set; }

    public virtual DbSet<Lock> Locks { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Relationship> Relationships { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Assign> Assigns { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseNpgsql("Host=localhost;Database=qlock;Username=postgres;Password=20010119");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assign>(entity =>
        {
            entity.HasKey(e => new { e.GroupId, e.UserId }).HasName("assigns_pkey");

            entity.ToTable("assigns");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("assigned_at");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("groups_pkey");

            entity.ToTable("groups");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.LockId).HasColumnName("lock_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Key>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("keys_pkey");

            entity.ToTable("keys");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpirationDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expiration_date");
            entity.Property(e => e.LockId).HasColumnName("lock_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.RemainingUses)
                .HasDefaultValueSql("'-1'::integer")
                .HasColumnName("remaining_uses");
            entity.Property(e => e.SecretKey)
                .HasMaxLength(255)
                .HasColumnName("secret_key");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Lock).WithMany(p => p.Keys)
                .HasForeignKey(d => d.LockId)
                .HasConstraintName("keys_lock_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Keys)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("keys_user_id_fkey");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
        });

        modelBuilder.Entity<Lock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("locks_pkey");

            entity.ToTable("locks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Owner).HasColumnName("owner");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.OwnerNavigation).WithMany(p => p.Locks)
                .HasForeignKey(d => d.Owner)
                .HasConstraintName("locks_owner_fkey");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("logs_pkey");

            entity.ToTable("logs");

            entity.HasIndex(e => e.KeyId, "idx_logs_key_id");

            entity.HasIndex(e => e.LockId, "idx_logs_lock_id");

            entity.HasIndex(e => e.UserId, "idx_logs_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasColumnName("action");
            entity.Property(e => e.KeyId).HasColumnName("key_id");
            entity.Property(e => e.LockId).HasColumnName("lock_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Text).HasColumnName("text");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("time");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Key).WithMany(p => p.Logs)
                .HasForeignKey(d => d.KeyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("logs_key_id_fkey");

            entity.HasOne(d => d.Lock).WithMany(p => p.Logs)
                .HasForeignKey(d => d.LockId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("logs_lock_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Logs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("logs_user_id_fkey");
        });

        modelBuilder.Entity<Relationship>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.FriendId }).HasName("relationships_pkey");

            entity.ToTable("relationships");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FriendId).HasColumnName("friend_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");

            entity.HasOne(d => d.Friend).WithMany(p => p.RelationshipFriends)
                .HasForeignKey(d => d.FriendId)
                .HasConstraintName("relationships_friend_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.RelationshipUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("relationships_user_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.LastLogin)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_login");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Pushtoken).HasColumnName("pushtoken");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
