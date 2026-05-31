using API.DTOs;
using API.Extensions;
using API.Models;
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace API.Controller
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtServices _jwtTokenService;
        private readonly Infrastructure.Context.InstagramContext _context;
        public AuthController(IUnitOfWork unitOfWork, IJwtServices jwtServices, Infrastructure.Context.InstagramContext context)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtServices;
            _context = context;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO userLogin)
        {
            try
            {
                bool ishas = await _unitOfWork.UserRepository.EmailExistsAsync(userLogin.Email!);
                if (!ishas)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = new[] { new { field = "Email", message = "User not found" } }
                    });
                }
                User? user = await _unitOfWork.UserRepository.GetByEmailAsync(userLogin.Email!);
                if (user == null || !BCrypt.Net.BCrypt.Verify(userLogin.Password, user.PasswordHash))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = new[] { new { field = "Email", message = "User not found" } }
                    });
                }
                string access_token = _jwtTokenService.GenerateAccessToken(user.Id, "User");

                await _unitOfWork.NotificationRepository.CreateNotificationAsync(
                    receiverId: user.Id,
                    senderId: user.Id,
                    type: 1,
                    previewText: "Bạn vừa đăng nhập thành công vào hệ thống."
                );
                await _unitOfWork.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    data = new
                    {
                        access_token = access_token
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred during login",
                    errors = new[] { new { field = "Server", message = ex.Message } }
                });
            }
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO userRegister)
        {
            try
            {
                bool ishas = await _unitOfWork.UserRepository.EmailExistsAsync(userRegister.Email!);
                if (ishas)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = new[] { new { field = "Email", message = "Email already exists" } }
                    });
                }
                await _unitOfWork.UserRepository.AddAsync(userRegister.UserName, userRegister.FullName, userRegister.Email, userRegister.Password);
                await _unitOfWork.SaveChanges();
                return StatusCode(StatusCodes.Status201Created, new
                {
                    success = true,
                    message = "User registered successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred during registration",
                    errors = new[] { new { field = "Server", message = ex.Message } }
                });
            }
        }
        [HttpPost("checkEmail")]
        public async Task<IActionResult> CheckEmail([FromBody] CheckEmailDTO checkEmail)
        {
            try
            {
                bool ishas = await _unitOfWork.UserRepository.EmailExistsAsync(checkEmail.Email!);
                return Ok(new
                {
                    success = true,
                    message = "Email check completed",
                    data = new
                    {
                        exists = ishas
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while checking the email",
                    errors = new[] { new { field = "Server", message = ex.Message } }
                });
            }
        }
        [HttpPost("save-device-token")]
        public async Task<IActionResult> SaveDeviceToken([FromBody] SaveTokenRequest request)
        {
            int userId = HttpContext.User.GetUserId();
            var existingDevice = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.DeviceToken == request.DeviceToken);

            if (existingDevice != null)
            {
                existingDevice.UserId = userId;
                existingDevice.DeviceType = request.DeviceType;
                existingDevice.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var newDevice = new UserDevice
                {
                    UserId = userId,
                    DeviceToken = request.DeviceToken,
                    DeviceType = request.DeviceType,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserDevices.Add(newDevice);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Lưu thiết bị nhận thông báo thành công!" });
        }
    }
}
