using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int ReceiverId { get; set; }

    public int SenderId { get; set; }

    public byte Type { get; set; }

    public int? PostId { get; set; }

    public int? CommentId { get; set; }

    public int? StoryId { get; set; }

    public bool IsRead { get; set; }

    public string? PreviewText { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Comment? Comment { get; set; }

    public virtual Post? Post { get; set; }

    public virtual User Receiver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;

    public virtual Story? Story { get; set; }
}
