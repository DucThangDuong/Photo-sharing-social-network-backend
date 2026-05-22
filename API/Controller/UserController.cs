using API.DTOs;
using API.Extensions;
using Application.DTOs;
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
        [HttpGet("{userIdFind}")]
        [Authorize]
        public async Task<IActionResult> GetProfileOfUser(int userIdFind)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userIdFind);
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
        [HttpGet("isFollow")]
        [Authorize]
        public async Task<IActionResult> UnfollowUser([FromQuery] int followingId)
        {
            try
            {
                int followerId = HttpContext.User.GetUserId();
                if (followerId == followingId)
                {
                    return BadRequest(new { success = false, message = "You cannot unfollow yourself" });
                }
                bool isUnfollowed = await _unitOfWork.UserRepository.IsFollowUser(followerId, followingId);
                return Ok(new
                {
                    success = true,
                    message = isUnfollowed ? "Unfollowed successfully" : "Followed successfully",
                    data = isUnfollowed
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while processing the unfollow request",
                    error = ex.Message
                });
            }
        }
        [HttpGet("postsSummary")]
        [Authorize]
        public async Task<IActionResult> GetMyPostsSummary()
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var posts = await _unitOfWork.PostRepository.GetPostsSummaryByUserIdAsync(userId);

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
        [HttpGet("postsSummary/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetPostsSummary(int userId)
        {
            try
            {
                var posts = await _unitOfWork.PostRepository.GetPostsSummaryByUserIdAsync(userId);

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
        [HttpGet("{userId}/posts")]
        [Authorize]
        public async Task<IActionResult> GetPostsUserId(int userId)
        {
            try
            {
                int myId = HttpContext.User.GetUserId();
                var posts = await _unitOfWork.PostRepository.GetPostsByUserIdAsync(userId, myId);
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
                user.Bio = dto.Bio;
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

        [HttpPut("post/{postId}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] PostUpdateDTO dto)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var post = await _unitOfWork.PostRepository.GetEntityByIdAsync(postId);
                
                if (post == null)
                {
                    return NotFound(new { success = false, message = "Post not found" });
                }

                if (post.UserId != userId)
                {
                    return Forbid();
                }

                bool isUpdated = false;

                if (dto.Caption != null && post.Caption != dto.Caption)
                {
                    post.Caption = dto.Caption;
                    isUpdated = true;
                }

                if (post.Visibility != dto.Visibility)
                {
                    post.Visibility = dto.Visibility;
                    isUpdated = true;
                }

                if (post.HideLikeCount != dto.HideLikeCount)
                {
                    post.HideLikeCount = dto.HideLikeCount;
                    isUpdated = true;
                }

                if (post.DisableComments != dto.DisableComments)
                {
                    post.DisableComments = dto.DisableComments;
                    isUpdated = true;
                }

                if (post.IsArchived != dto.IsArchived)
                {
                    post.IsArchived = dto.IsArchived;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    await _unitOfWork.PostRepository.UpdateAsync(post);
                    await _unitOfWork.SaveChanges();
                }
                var newPostData = await _unitOfWork.PostRepository.GetPostsByPostIdAsync(post.Id,userId);
                return Ok(new
                {
                    success = true,
                    message = isUpdated ? "Post updated successfully" : "No changes were made",
                    data= newPostData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while updating the post",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("post/{postId}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var post = await _unitOfWork.PostRepository.GetEntityByIdAsync(postId);
                
                if (post == null)
                {
                    return NotFound(new { success = false, message = "Post not found" });
                }

                if (post.UserId != userId)
                {
                    return Forbid();
                }

                if (post.IsDeleted)
                {
                    return BadRequest(new { success = false, message = "Post is already deleted" });
                }

                post.IsDeleted = true;
                await _unitOfWork.PostRepository.UpdateAsync(post);
                await _unitOfWork.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Post deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while deleting the post",
                    error = ex.Message
                });
            }
        }

        [HttpGet("feed")]
        [Authorize]
        public async Task<IActionResult> GetFeed()
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var posts = await _unitOfWork.PostRepository.GetFeedPostsAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "Feed retrieved successfully",
                    data = posts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving feed",
                    error = ex.Message
                });
            }
        }

        [HttpGet("suggestions")]
        [Authorize]
        public async Task<IActionResult> GetSuggestedUsers()
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var suggestedUsers = await _unitOfWork.UserRepository.GetSuggestedUsersAsync(userId, 10);

                return Ok(new
                {
                    success = true,
                    message = "Suggested users retrieved successfully",
                    data = suggestedUsers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving suggested users",
                    error = ex.Message
                });
            }
        }

        [HttpGet("search/users")]
        [Authorize]
        public async Task<IActionResult> SearchUsers([FromQuery] string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest(new { success = false, message = "Keyword is required" });
                }

                var users = await _unitOfWork.UserRepository.SearchUsersAsync(keyword);

                return Ok(new
                {
                    success = true,
                    message = "Users search completed",
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while searching users",
                    error = ex.Message
                });
            }
        }

        [HttpPost("follow/{followingId}")]
        [Authorize]
        public async Task<IActionResult> FollowUser(int followingId)
        {
            try
            {
                int followerId = HttpContext.User.GetUserId();

                if (followerId == followingId)
                {
                    return BadRequest(new { success = false, message = "You cannot follow yourself" });
                }

                bool isFollowed = await _unitOfWork.UserRepository.FollowUserAsync(followerId, followingId);
                await _unitOfWork.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = isFollowed ? "Followed successfully" : "Unfollowed successfully",
                    data = new { isFollowed }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while processing the follow request",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{userId}/followers")]
        [Authorize]
        public async Task<IActionResult> GetFollowers(int userId)
        {
            try
            {
                var followers = await _unitOfWork.UserRepository.GetFollowersAsync(userId);
                return Ok(new
                {
                    success = true,
                    message = "Followers retrieved successfully",
                    data = followers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving followers",
                    error = ex.Message
                });
            }
        }

        [HttpGet("{userId}/following")]
        [Authorize]
        public async Task<IActionResult> GetFollowing(int userId)
        {
            try
            {
                var following = await _unitOfWork.UserRepository.GetFollowingAsync(userId);
                return Ok(new
                {
                    success = true,
                    message = "Following list retrieved successfully",
                    data = following
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving following list",
                    error = ex.Message
                });
            }
        }
    }
}
