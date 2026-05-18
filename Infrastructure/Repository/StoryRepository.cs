using API.Entities;
using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class StoryRepository : IStoryRepository
    {
        private readonly InstagramContext _context;

        public StoryRepository(InstagramContext context)
        {
            _context = context;
        }

        public async Task<StoryDTO> AddStoryAsync(int userId, string mediaUrl)
        {
            var story = new Story
            {
                UserId = userId,
                MediaUrl = mediaUrl,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24), 
                IsDeleted = false,
                IsArchived = false
            };

            await _context.Stories.AddAsync(story);
            
            return new StoryDTO
            {
                Id = story.Id,
                UserId = story.UserId,
                MediaUrl = story.MediaUrl,
                CreatedAt = story.CreatedAt,
                ExpiresAt = story.ExpiresAt
            };
        }

        public async Task CreateStoryView(int storyId, int viewerId)
        {
            StoryView storyView = new StoryView
            {
                StoryId = storyId,
                ViewerId = viewerId,
                ViewedAt = DateTime.UtcNow,
                IsLiked = false
            };
            await _context.StoryViews.AddAsync(storyView);
        }

        public async Task<List<UserStoryDTO>> GetActiveStoriesAsync(int useuserIdguestrId, int UserId)
        {
            var now = DateTime.UtcNow;

            // 1. Chỉ lấy các Story còn hạn của ĐÚNG người dùng mục tiêu (userId)
            var activeStories = await _context.Stories
                .Where(s => s.UserId == useuserIdguestrId && !s.IsDeleted && s.ExpiresAt > now)
                .OrderBy(s => s.CreatedAt) // Sắp xếp cũ nhất lên trước
                .ToListAsync();

            // Nếu người này không có Story nào, trả về mảng rỗng luôn
            if (!activeStories.Any())
            {
                return new List<UserStoryDTO>();
            }

            // 2. Lấy danh sách ID các Story mà người xem (currentUserId) ĐÃ XEM
            var activeStoryIds = activeStories.Select(s => s.Id).ToList();

            var viewedStoryIds = await _context.StoryViews
                .Where(sv => sv.ViewerId == UserId && activeStoryIds.Contains(sv.StoryId))
                .Select(sv => sv.StoryId)
                .ToListAsync();

            var viewedSet = new HashSet<int>(viewedStoryIds);

            // 3. Lấy thông tin cơ bản của người dùng mục tiêu
            var user = await _context.Users
                .Where(u => u.Id == useuserIdguestrId)
                .Select(u => new { u.Id, u.Username, u.AvatarUrl }) // Chỉ lấy những cột cần thiết cho nhẹ
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new List<UserStoryDTO>();
            }

            // 4. Map dữ liệu sang DTO
            var storiesList = activeStories.Select(s => new StoryDTO
            {
                Id = s.Id,
                UserId = s.UserId,
                MediaUrl = s.MediaUrl,
                CreatedAt = s.CreatedAt,
                ExpiresAt = s.ExpiresAt,
                IsSeen = viewedSet.Contains(s.Id)
            }).ToList();

            var userStory = new UserStoryDTO
            {
                UserId = user.Id,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl,
                HasSeen = storiesList.All(s => s.IsSeen),
                Stories = storiesList
            };

            // Dù chỉ có 1 User, nhưng vì kiểu trả về của bạn là List<UserStoryDTO> nên ta bọc nó trong List
            return new List<UserStoryDTO> { userStory };
        }
    }
}
