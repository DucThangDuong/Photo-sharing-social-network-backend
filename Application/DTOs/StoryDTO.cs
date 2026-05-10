using System;

namespace Application.DTOs
{
    public class StoryDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string MediaUrl { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class UserStoryDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public List<StoryDTO> Stories { get; set; } = new List<StoryDTO>();
    }
}
