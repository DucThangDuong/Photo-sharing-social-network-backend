using API.Models;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Context;

namespace Infrastructure.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly InstagramContext _context;

        public NotificationRepository(InstagramContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationDTO>> GetUserNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.ReceiverId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDTO
                {
                    Id = n.Id,
                    Type = n.Type,
                    SenderId = n.SenderId,
                    SenderUsername = n.Sender.Username,
                    SenderAvatarUrl = n.Sender.AvatarUrl,
                    PreviewText = n.PreviewText,
                    PostId = n.PostId,
                    CommentId = n.CommentId,
                    StoryId = n.StoryId,
                    TargetMediaUrl = n.Post != null && n.Post.PostMedia.Any() 
                        ? n.Post.PostMedia.OrderBy(m => m.SortOrder).FirstOrDefault().MediaUrl 
                        : (n.Story != null ? n.Story.MediaUrl : null),
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task CreateNotificationAsync(int receiverId, int senderId, byte type, string previewText)
        {
            var notification = new Notification
            {
                ReceiverId = receiverId,
                SenderId = senderId,
                Type = type,
                PreviewText = previewText,
                IsRead = false,
                CreatedAt = System.DateTime.UtcNow
            };
            
            await _context.Notifications.AddAsync(notification);
        }

        public async Task CreateNotificationsForFollowersAsync(int senderId, byte type, string previewText, int? postId = null)
        {
            var followerIds = await _context.Follows
                .Where(f => f.FollowingId == senderId)
                .Select(f => f.FollowerId)
                .ToListAsync();

            if (followerIds.Any())
            {
                var notifications = followerIds.Select(followerId => new Notification
                {
                    ReceiverId = followerId,
                    SenderId = senderId,
                    Type = type,
                    PostId = postId,
                    PreviewText = previewText,
                    IsRead = false,
                    CreatedAt = System.DateTime.UtcNow
                });

                await _context.Notifications.AddRangeAsync(notifications);
            }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.ReceiverId == userId);
            
            if (notification == null) return false;

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                _context.Notifications.Update(notification);
            }
            
            return true;
        }
    }
}
