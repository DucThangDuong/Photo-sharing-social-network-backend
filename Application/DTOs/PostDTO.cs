using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PostDTO
    {
        public int Id { get; set; }
        public string? Caption { get; set; }
        public List<PostMediumDTO> PostMedia { get; set; } = new List<PostMediumDTO>();
    }

    public class PostMediumDTO
    {
        public string MediaUrl { get; set; } = null!;
    }
}
