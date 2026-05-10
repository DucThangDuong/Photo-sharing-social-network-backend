using Application.Interfaces;
using API.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Extensions;

namespace API.Controller
{
    [Route("story")]
    [ApiController]
    public class StoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public StoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddStory([FromForm] IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No image uploaded." });
                }

                int userId = HttpContext.User.GetUserId();
                
                string baseFolderPath = @"D:\LTDD_images\stories";
                if (!System.IO.Directory.Exists(baseFolderPath))
                {
                    System.IO.Directory.CreateDirectory(baseFolderPath);
                }

                string fileName = $"{userId}_{System.DateTime.UtcNow.Ticks}{System.IO.Path.GetExtension(image.FileName)}";
                string filePath = System.IO.Path.Combine(baseFolderPath, fileName);

                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                string mediaUrl = $"/images/stories/{fileName}";

                var story = await _unitOfWork.StoryRepository.AddStoryAsync(userId, mediaUrl);
                await _unitOfWork.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Story added successfully",
                    data = story
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while adding story",
                    error = ex.Message
                });
            }
        }

        [HttpGet("active")]
        [Authorize]
        public async Task<IActionResult> GetActiveStories()
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var activeStories = await _unitOfWork.StoryRepository.GetActiveStoriesAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Active stories retrieved successfully",
                    data = activeStories
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving stories",
                    error = ex.Message
                });
            }
        }
    }
}