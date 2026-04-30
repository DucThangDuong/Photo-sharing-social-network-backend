using API.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtServices _jwtTokenService;
        public AuthController(IUnitOfWork unitOfWork, IJwtServices jwtServices)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtServices;
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
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = new[] { new { field = "Email", message = "User not found" } }
                    });
                }
                string access_token = _jwtTokenService.GenerateAccessToken(user.Id, "User");
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
                await _unitOfWork.UserRepository.AddAsync(userRegister.Email, userRegister.Email, userRegister.Password);
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
    }
}
