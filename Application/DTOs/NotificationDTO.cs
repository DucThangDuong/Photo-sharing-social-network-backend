using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public byte Type { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; } = null!;
        public string? SenderAvatarUrl { get; set; }
        public string? PreviewText { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        public int? StoryId { get; set; }
        public string? TargetMediaUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
