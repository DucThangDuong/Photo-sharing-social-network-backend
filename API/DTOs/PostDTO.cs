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
}
