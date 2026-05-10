using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStoryRepository
    {
        Task<StoryDTO> AddStoryAsync(int userId, string mediaUrl);
        Task<System.Collections.Generic.List<UserStoryDTO>> GetActiveStoriesAsync(int currentUserId);
    }
}
