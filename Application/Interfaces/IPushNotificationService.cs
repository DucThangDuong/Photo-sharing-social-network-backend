namespace Application.Interfaces
{
    public interface IPushNotificationService
    {
        /// <summary>
        /// Gửi push notification tới một thiết bị cụ thể.
        /// </summary>
        Task SendPushAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);

        /// <summary>
        /// Gửi push notification tới nhiều thiết bị cùng lúc.
        /// </summary>
        Task SendPushToMultipleAsync(List<string> deviceTokens, string title, string body, Dictionary<string, string>? data = null);

        /// <summary>
        /// Gửi push notification tới tất cả thiết bị của những người follow một user.
        /// </summary>
        Task SendPushToFollowersAsync(int senderId, string title, string body, Dictionary<string, string>? data = null);
    }
}
