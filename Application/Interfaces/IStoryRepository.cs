using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStoryRepository
    {
        Task<StoryDTO> AddStoryAsync(int userId, string mediaUrl);
        Task<List<UserStoryDTO>> GetActiveStoriesAsync(int guestId,int currentUserId);
        Task CreateStoryView(int storyId, int viewerId);
    }
}
