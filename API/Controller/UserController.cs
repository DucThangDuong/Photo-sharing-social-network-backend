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

        [HttpGet("post/{postId}/comments")]
        [Authorize]
        public async Task<IActionResult> GetPostComments(int postId)
        {
            try
            {
                var comments = await _unitOfWork.PostRepository.GetCommentsByPostIdAsync(postId);

                return Ok(new
                {
                    success = true,
                    message = "Comments retrieved successfully",
                    data = comments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving comments",
                    error = ex.Message
                });
            }
        }

        [HttpPost("post/{postId}/like")]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                bool isLiked = await _unitOfWork.PostRepository.ToggleLikeAsync(postId, userId);
                await _unitOfWork.SaveChanges();

                int likeCount = (await _unitOfWork.PostRepository.GetPostsByPostIdAsync(postId, userId))?.LikeCount ?? 0;

                return Ok(new
                {
                    success = true,
                    message = isLiked ? "Post liked" : "Post unliked",
                    data = new
                    {
                        isLiked,
                        likeCount
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while toggling like",
                    error = ex.Message
                });
            }
        }

        [HttpPost("post/{postId}/comment")]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, [FromBody] CommentRequestDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Content))
                {
                    return BadRequest(new { success = false, message = "Comment content is required" });
                }

                int userId = HttpContext.User.GetUserId();
                var comment = await _unitOfWork.PostRepository.AddCommentAsync(postId, userId, dto.Content);

                return Ok(new
                {
                    success = true,
                    message = "Comment added successfully",
                    data = comment
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while adding comment",
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

        [HttpGet("trending")]
        [Authorize]
        public async Task<IActionResult> GetTrendingPosts()
        {
            try
            {
                var posts = await _unitOfWork.PostRepository.GetTrendingPostsAsync(10);

                return Ok(new
                {
                    success = true,
                    message = "Trending posts retrieved successfully",
                    data = posts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving trending posts",
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

        [HttpGet("search/posts")]
        [Authorize]
        public async Task<IActionResult> SearchPosts([FromQuery] string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest(new { success = false, message = "Keyword is required" });
                }

                var posts = await _unitOfWork.PostRepository.SearchPostsByCaptionAsync(keyword);

                return Ok(new
                {
                    success = true,
                    message = "Posts search completed",
                    data = posts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while searching posts",
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
