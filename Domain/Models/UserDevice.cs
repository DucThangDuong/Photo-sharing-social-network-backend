using System;
using System.Collections.Generic;

namespace API.Models;

public partial class UserDevice
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string DeviceToken { get; set; } = null!;

    public string? DeviceType { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
