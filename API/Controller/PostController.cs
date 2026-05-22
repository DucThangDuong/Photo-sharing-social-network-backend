using API.DTOs;
using API.Extensions;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [Route("post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        public readonly IUnitOfWork _unitOfWork;
        public PostController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
        [HttpGet("archived")]
        [Authorize]
        public async Task<IActionResult> GetArchivedPosts()
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var posts = await _unitOfWork.PostRepository.GetArchivedPostsByUserIdAsync(userId);
                return Ok(new
                {
                    success = true,
                    message = "Archived posts retrieved successfully",
                    data = posts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving archived posts",
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
        [HttpGet("{postId}/comments")]
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
        [HttpGet("{postId}")]
        [Authorize]
        public async Task<IActionResult> GetPostById(int postId)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var posts = await _unitOfWork.PostRepository.GetPostsByPostIdAsync(postId, userId);
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
        [HttpGet("user/{postId}")]
        [Authorize]
        public async Task<IActionResult> GetPostByIdWithUser(int postId)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var posts = await _unitOfWork.PostRepository.GetPostsByPostIdWithUserAsync(postId, userId);
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
        [HttpGet("user/{postId}/archived")]
        [Authorize]
        public async Task<IActionResult> GetPostByIdArchive(int postId)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var posts = await _unitOfWork.PostRepository.GetPostsByPostIdArchivedAsync(postId, userId);
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
        [HttpPost("newPost")]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromForm] PostDTO postDto)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();

                var post = new API.Entities.Post
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

                            post.PostMedia.Add(new API.Entities.PostMedium
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
        [HttpPost("{postId}/like")]
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
        [HttpPost("{postId}/comment")]
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
                var post = await _unitOfWork.PostRepository.GetEntityByIdAsync(postId);
                if (post.DisableComments==true)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        success = false,
                        message = "An error occurred while adding comment",
                    });
                }
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
        [HttpPut("update/{postId}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] PostUpdateCaptionDTO dto)
        {
            try
            {
                var post = await _unitOfWork.PostRepository.GetEntityByIdAsync(postId);
                if (post == null)
                {
                    return NotFound(new { success = false, message = "Post not found" });
                }
                var newPost = await _unitOfWork.PostRepository.UpdateCaptionPost(postId, dto.Caption);
                await _unitOfWork.SaveChanges();
                return Ok(new
                {
                    success = true,
                    message = "Post updated successfully",
                    data = newPost
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

        [HttpGet("liked")]
        [Authorize]
        public async Task<IActionResult> GetLikedPosts()
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var posts = await _unitOfWork.PostRepository.GetLikedPostsByUserIdAsync(userId);
                return Ok(new
                {
                    success = true,
                    message = "Liked posts retrieved successfully",
                    data = posts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving liked posts",
                    error = ex.Message
                });
            }
        }

    }
}
