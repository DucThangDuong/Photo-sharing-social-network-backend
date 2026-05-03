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
        public string? UserName { get; set; }
        public string? FullName { get; set; }
    }
    public class CheckEmailDTO
    {
        public string? Email { get; set; }
    }
}
