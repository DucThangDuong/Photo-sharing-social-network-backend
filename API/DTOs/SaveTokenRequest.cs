using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class SaveTokenRequest
    {
        [Required(ErrorMessage = "DeviceToken is required")]
        public string DeviceToken { get; set; } = null!;

        public string? DeviceType { get; set; }
    }
}
