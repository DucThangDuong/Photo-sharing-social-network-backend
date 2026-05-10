using System;
using System.Collections.Generic;

namespace API.Entities;

public partial class Story
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsArchived { get; set; }

    public virtual ICollection<StoryView> StoryViews { get; set; } = new List<StoryView>();

    public virtual User User { get; set; } = null!;
}
