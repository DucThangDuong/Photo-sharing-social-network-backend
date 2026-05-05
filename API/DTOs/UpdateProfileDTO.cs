using Microsoft.AspNetCore.Http;

namespace API.DTOs
{
    public class UpdateProfileDTO
    {
        public IFormFile? Avatar { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public int? Gender { get; set; }
    }
}
