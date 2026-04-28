using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace API.Entities;

public partial class InstagramContext : DbContext
{
    public InstagramContext()
    {
    }

    public InstagramContext(DbContextOptions<InstagramContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Follow> Follows { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostMedium> PostMedia { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comments__3214EC07F9DEDCE9");

            entity.Property(e => e.Content).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__Comments__PostId__60A75C0F");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comments__UserId__619B8048");
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(e => new { e.FollowerId, e.FollowingId }).HasName("PK__Follows__79CB0335C50E0770");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Follower).WithMany(p => p.FollowFollowers)
                .HasForeignKey(d => d.FollowerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Follows__Followe__4F7CD00D");

            entity.HasOne(d => d.Following).WithMany(p => p.FollowFollowings)
                .HasForeignKey(d => d.FollowingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Follows__Followi__5070F446");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => new { e.PostId, e.UserId }).HasName("PK__Likes__7B6AECDC428A395B");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__Likes__PostId__5BE2A6F2");

            entity.HasOne(d => d.User).WithMany(p => p.Likes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Likes__UserId__5CD6CB2B");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Posts__3214EC07282E66AF");

            entity.Property(e => e.Caption).HasMaxLength(2200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Posts__UserId__5441852A");
        });

        modelBuilder.Entity<PostMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PostMedi__3214EC07B2C1DCA0");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.Post).WithMany(p => p.PostMedia)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__PostMedia__PostI__5812160E");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0753743907");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E41542EB7D").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053437A51362").IsUnique();

            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Bio).HasMaxLength(150);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
