using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Post
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Caption { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<PostMedium> PostMedia { get; set; } = new List<PostMedium>();

    public virtual User User { get; set; } = null!;
}
