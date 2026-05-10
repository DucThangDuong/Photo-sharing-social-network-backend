using API.Entities;
using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Context;
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
                ExpiresAt = DateTime.UtcNow.AddHours(24), // Story expires after 24 hours
                IsDeleted = false,
                IsArchived = false
            };

            await _context.Stories.AddAsync(story);
            // SaveChanges is called by UnitOfWork
            
            return new StoryDTO
            {
                Id = story.Id,
                UserId = story.UserId,
                MediaUrl = story.MediaUrl,
                CreatedAt = story.CreatedAt,
                ExpiresAt = story.ExpiresAt
            };
        }

        public async Task<System.Collections.Generic.List<UserStoryDTO>> GetActiveStoriesAsync(int currentUserId)
        {
            var followingIds = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                System.Linq.Queryable.Select(
                    System.Linq.Queryable.Where(_context.Follows, f => f.FollowerId == currentUserId),
                    f => f.FollowingId
                )
            );

            var userIds = new System.Collections.Generic.List<int> { currentUserId };
            userIds.AddRange(followingIds);

            var now = DateTime.UtcNow;

            var activeStories = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                System.Linq.Queryable.Where(
                    _context.Stories,
                    s => userIds.Contains(s.UserId) && !s.IsDeleted && s.ExpiresAt > now
                )
            );

            var groupedStories = System.Linq.Enumerable.GroupBy(activeStories, s => s.UserId);

            var result = new System.Collections.Generic.List<UserStoryDTO>();

            foreach (var group in groupedStories)
            {
                var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                    _context.Users, u => u.Id == group.Key
                );

                if (user != null)
                {
                    result.Add(new UserStoryDTO
                    {
                        UserId = user.Id,
                        Username = user.Username,
                        AvatarUrl = user.AvatarUrl,
                        Stories = System.Linq.Enumerable.ToList(
                            System.Linq.Enumerable.Select(
                                System.Linq.Enumerable.OrderBy(group, s => s.CreatedAt),
                                s => new StoryDTO
                                {
                                    Id = s.Id,
                                    UserId = s.UserId,
                                    MediaUrl = s.MediaUrl,
                                    CreatedAt = s.CreatedAt,
                                    ExpiresAt = s.ExpiresAt
                                }
                            )
                        )
                    });
                }
            }

            // Put current user first
            result = System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.OrderByDescending(result, r => r.UserId == currentUserId)
            );

            return result;
        }
    }
}
