using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class PostMedium
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
