using Application.DTOs;
using API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);
        Task<List<PostSummaryDTO>> GetPostsSummaryByUserIdAsync(int userId);
        Task<List<PostDetailDTO>> GetPostsByUserIdAsync(int userId);
        Task<Post?> GetEntityByIdAsync(int postId);
        Task UpdateAsync(Post post);
        Task<PostDetailDTO?> GetPostsByPostIdAsync(int PostId, int userId);
        Task<List<CommentDTO>> GetCommentsByPostIdAsync(int postId);
        Task<bool> ToggleLikeAsync(int postId, int userId);
        Task<CommentDTO> AddCommentAsync(int postId, int userId, string content);
        Task<List<FeedPostDTO>> GetFeedPostsAsync(int currentUserId);
        Task<List<PostSummaryDTO>> GetTrendingPostsAsync(int limit = 10);
        Task<List<PostSummaryDTO>> SearchPostsByCaptionAsync(string keyword);
    }
}
