using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Comment
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Post Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
