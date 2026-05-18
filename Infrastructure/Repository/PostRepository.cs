using API.Entities;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Context; 
namespace Infrastructure.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly InstagramContext _context;

        public PostRepository(InstagramContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
        }

        public async Task<List<PostSummaryDTO>> GetPostsSummaryByUserIdAsync(int userId)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId && p.IsDeleted != true)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostSummaryDTO
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    PostMedia = p.PostMedia.Select(pm => new PostSummaryMediumDTO
                    {
                        MediaUrl = pm.MediaUrl
                    }).ToList()
                })
                .ToListAsync();
        }
        public async Task<List<PostDetailDTO>> GetPostsByUserIdAsync(int userId)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId && p.IsDeleted != true)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostDetailDTO
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    CreatedAt = p.CreatedAt,
                    Visibility = p.Visibility,
                    HideLikeCount = p.HideLikeCount,
                    DisableComments = p.DisableComments,
                    LikeCount = p.Likes.Count(),
                    CommentCount = p.Comments.Count(),
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == userId),
                    IsArchived = p.IsArchived,
                    PostMedia = p.PostMedia.Select(pm => new PostSummaryMediumDTO
                    {
                        MediaUrl = pm.MediaUrl
                    }).ToList()
                })
                .ToListAsync();
        }
        public async Task<PostDetailDTO?> GetPostsByPostIdAsync(int PostId, int userId)
        {
            return await _context.Posts
                .Where(p => p.Id == PostId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostDetailDTO
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    CreatedAt = p.CreatedAt,
                    Visibility = p.Visibility,
                    HideLikeCount = p.HideLikeCount,
                    DisableComments = p.DisableComments,
                    LikeCount = p.Likes.Count(),
                    CommentCount = p.Comments.Count(),
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == userId),
                    IsArchived = p.IsArchived,
                    PostMedia = p.PostMedia.Select(pm => new PostSummaryMediumDTO
                    {
                        MediaUrl = pm.MediaUrl
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Post?> GetEntityByIdAsync(int postId)
        {
            return await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        }

        public Task UpdateAsync(Post post)
        {
            _context.Posts.Update(post);
            return Task.CompletedTask;
        }

        public async Task<List<CommentDTO>> GetCommentsByPostIdAsync(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDTO
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Username = c.User.Username,
                    AvatarUrl = c.User.AvatarUrl,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> ToggleLikeAsync(int postId, int userId)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                return false;
            }
            else
            {
                await _context.Likes.AddAsync(new Like
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
                return true; // Đã like
            }
        }

        public async Task<CommentDTO> AddCommentAsync(int postId, int userId, string content)
        {
            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Lấy thông tin user để trả về DTO
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            return new CommentDTO
            {
                Id = comment.Id,
                UserId = userId,
                Username = user!.Username,
                AvatarUrl = user.AvatarUrl,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<List<FeedPostDTO>> GetFeedPostsAsync(int currentUserId)
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted && !p.IsArchived)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new FeedPostDTO
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    AvatarUrl = p.User.AvatarUrl,
                    Caption = p.Caption,
                    CreatedAt = p.CreatedAt,
                    Visibility = p.Visibility,
                    HideLikeCount = p.HideLikeCount,
                    DisableComments = p.DisableComments,
                    IsArchived = p.IsArchived,
                    LikeCount = p.Likes.Count(),
                    CommentCount = p.Comments.Count(),
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == currentUserId),
                    PostMedia = p.PostMedia.Select(pm => new PostSummaryMediumDTO
                    {
                        MediaUrl = pm.MediaUrl
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<PostSummaryDTO>> GetTrendingPostsAsync(int limit = 10)
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted && !p.IsArchived)
                .OrderByDescending(p => p.Likes.Count())
                .ThenByDescending(p => p.CreatedAt)
                .Take(limit)
                .Select(p => new PostSummaryDTO
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    PostMedia = p.PostMedia.Select(pm => new PostSummaryMediumDTO
                    {
                        MediaUrl = pm.MediaUrl
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<PostSummaryDTO>> SearchPostsByCaptionAsync(string keyword)
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted && !p.IsArchived && p.Caption != null && p.Caption.Contains(keyword))
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostSummaryDTO
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    PostMedia = p.PostMedia.Select(pm => new PostSummaryMediumDTO
                    {
                        MediaUrl = pm.MediaUrl
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<Post> UpdateCaptionPost(int postId, string newCaption)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post != null)
            {
                post.Caption = newCaption;
            }
            return post!;
        }

        public async Task<List<PostDetailDTO>> GetPostsByUserIdAsync(int userId, int myId)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId && p.IsDeleted != true)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostDetailDTO
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    CreatedAt = p.CreatedAt,
                    Visibility = p.Visibility,
                    HideLikeCount = p.HideLikeCount,
                    DisableComments = p.DisableComments,
                    LikeCount = p.Likes.Count(),
                    CommentCount = p.Comments.Count(),
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == myId),
                    IsArchived = p.IsArchived,
                    PostMedia = p.PostMedia.Select(pm => new PostSummaryMediumDTO
                    {
                        MediaUrl = pm.MediaUrl
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}
