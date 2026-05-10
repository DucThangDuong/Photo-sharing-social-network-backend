using System;
using System.Collections.Generic;

namespace API.Entities;

public partial class StoryView
{
    public int StoryId { get; set; }

    public int ViewerId { get; set; }

    public bool IsLiked { get; set; }

    public DateTime ViewedAt { get; set; }

    public virtual Story Story { get; set; } = null!;

    public virtual User Viewer { get; set; } = null!;
}
