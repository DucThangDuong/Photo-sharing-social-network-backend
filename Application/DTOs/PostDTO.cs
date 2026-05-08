using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PostSummaryDTO
    {
        public int Id { get; set; }
        public string? Caption { get; set; }
        public List<PostSummaryMediumDTO> PostMedia { get; set; } = new List<PostSummaryMediumDTO>();
    }

    public class PostSummaryMediumDTO
    {
        public string MediaUrl { get; set; } = null!;
    }
    public class PostDetailDTO: PostSummaryDTO
    {
        public DateTime CreatedAt { get; set; }
        public byte Visibility { get; set; }
        public bool HideLikeCount { get; set; }
        public bool DisableComments { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool IsArchived { get; set; }
    }

    public class FeedPostDTO : PostDetailDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }

    public class CommentDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
