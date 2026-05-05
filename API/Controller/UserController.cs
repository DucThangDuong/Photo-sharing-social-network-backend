using API.DTOs;
using API.Extensions;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace API.Controller
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found"
                    });
                }
                return Ok(new
                {
                    success = true,
                    message = "User profile retrieved successfully",
                    data = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving the user profile",
                    error = ex.Message
                });
            }
        }
        [HttpPost("newPost")]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromForm] PostDTO postDto)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();

                var post = new Domain.Entities.Post
                {
                    UserId = userId,
                    Caption = postDto.Caption,
                    CreatedAt = DateTime.UtcNow
                };

                if (postDto.Images != null && postDto.Images.Count > 0)
                {
                    string baseFolderPath = @"D:\LTDD_images";
                    if (!Directory.Exists(baseFolderPath))
                    {
                        Directory.CreateDirectory(baseFolderPath);
                    }

                    string folderName = $"{userId}_{DateTime.UtcNow.Ticks}";
                    string folderPath = Path.Combine(baseFolderPath, folderName);
                    
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    int currentSortOrder = 1;
                    foreach (var image in postDto.Images)
                    {
                        if (image.Length > 0)
                        {
                            string filePath = Path.Combine(folderPath, image.FileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            post.PostMedia.Add(new Domain.Entities.PostMedium
                            {
                                MediaUrl = $"/images/{folderName}/{image.FileName}",
                                SortOrder = currentSortOrder++,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                await _unitOfWork.PostRepository.AddAsync(post);
                await _unitOfWork.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Post created successfully",
                    data = post
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while creating the post",
                    error = ex.Message
                });
            }
        }

        [HttpGet("posts")]
        [Authorize]
        public async Task<IActionResult> GetMyPosts()
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var posts = await _unitOfWork.PostRepository.GetPostsByUserIdAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Posts retrieved successfully",
                    data = posts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving posts",
                    error = ex.Message
                });
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDTO dto)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var user = await _unitOfWork.UserRepository.GetEntityByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                if (!string.IsNullOrEmpty(dto.Username)) user.Username = dto.Username;
                if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
                if (!string.IsNullOrEmpty(dto.Bio)) user.Bio = dto.Bio;
                if (dto.Gender.HasValue) user.Gender = dto.Gender.Value;

                if (dto.Avatar != null && dto.Avatar.Length > 0)
                {
                    string baseFolderPath = @"D:\LTDD_images\avatars";
                    if (!Directory.Exists(baseFolderPath))
                    {
                        Directory.CreateDirectory(baseFolderPath);
                    }

                    string fileName = $"{userId}_{DateTime.UtcNow.Ticks}{Path.GetExtension(dto.Avatar.FileName)}";
                    string filePath = Path.Combine(baseFolderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Avatar.CopyToAsync(stream);
                    }

                    user.AvatarUrl = $"/images/avatars/{fileName}";
                }

                await _unitOfWork.UserRepository.UpdateAsync(user);
                await _unitOfWork.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Profile updated successfully",
                    data = new
                    {
                        user.Id,
                        user.Username,
                        user.FullName,
                        user.Bio,
                        user.Gender,
                        user.AvatarUrl
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while updating the profile",
                    error = ex.Message
                });
            }
        }
    }
}
