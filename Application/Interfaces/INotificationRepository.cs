using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<NotificationDTO>> GetUserNotificationsAsync(int userId);
        Task CreateNotificationAsync(int receiverId, int senderId, byte type, string previewText);
        Task CreateNotificationsForFollowersAsync(int senderId, byte type, string previewText, int? postId = null);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
    }
}
