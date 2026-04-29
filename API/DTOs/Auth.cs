namespace API.DTOs
{
    public class LoginDTO
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class RegisterDTO
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? Username { get; set; }
    }
}
