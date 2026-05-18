using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace API.DTOs
{
    public class PostDTO
    {
        public string? Caption { get; set; }
        public List<IFormFile>? Images { get; set; }
        public int SortOrder { get; set; }
    }
    public class PostUpdateDTO
    {
        public string? Caption { get; set; }
        public byte Visibility { get; set; }
        public bool HideLikeCount { get; set; }
        public bool DisableComments { get; set; }
        public bool IsArchived { get; set; }
    }
    public class PostUpdateCaptionDTO
    {
        public string? Caption { get; set; }
    }
    public class CommentRequestDTO
    {
        public string Content { get; set; } = null!;
    }
}
