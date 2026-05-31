using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Application.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class InsecureHttpClientFactory : Google.Apis.Http.HttpClientFactory
    {
        protected override HttpClientHandler CreateClientHandler()
        {
            var handler = base.CreateClientHandler();
            // Bỏ qua lỗi chứng chỉ SSL (thường gặp khi dùng phần mềm diệt virus hoặc proxy)
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            return handler;
        }
    }

    public class PushNotificationService : IPushNotificationService
    {
        private readonly InstagramContext _context;

        public PushNotificationService(InstagramContext context)
        {
            _context = context;

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("ltdd-push-notification-firebase-adminsdk-fbsvc-7376edb21d.json"),
                    HttpClientFactory = new InsecureHttpClientFactory()
                });
            }
        }

        /// <summary>
        /// Gửi push notification tới một thiết bị cụ thể.
        /// </summary>
        public async Task SendPushAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null)
        {
            var message = new Message()
            {
                Token = deviceToken,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>()
                {
                    { "click_action", "FLUTTER_NOTIFICATION_CLICK" }
                }
            };

            try
            {
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"[FCM] Push thành công: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FCM] Lỗi push tới token {deviceToken}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gửi push notification tới nhiều thiết bị cùng lúc (tối đa 500 token/lần theo giới hạn FCM).
        /// </summary>
        public async Task SendPushToMultipleAsync(List<string> deviceTokens, string title, string body, Dictionary<string, string>? data = null)
        {
            if (deviceTokens == null || deviceTokens.Count == 0) return;

            var messageData = data ?? new Dictionary<string, string>()
            {
                { "click_action", "FLUTTER_NOTIFICATION_CLICK" }
            };

            // FCM cho phép tối đa 500 token mỗi lần gửi MulticastMessage
            const int batchSize = 500;
            for (int i = 0; i < deviceTokens.Count; i += batchSize)
            {
                var batch = deviceTokens.Skip(i).Take(batchSize).ToList();

                var multicastMessage = new MulticastMessage()
                {
                    Tokens = batch,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = messageData
                };

                try
                {
                    var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicastMessage);
                    Console.WriteLine($"[FCM] Gửi {response.SuccessCount}/{batch.Count} thành công, {response.FailureCount} thất bại.");

                    // Xóa các token không còn hợp lệ khỏi DB
                    for (int j = 0; j < response.Responses.Count; j++)
                    {
                        if (!response.Responses[j].IsSuccess)
                        {
                            var ex = response.Responses[j].Exception;
                            var errorCode = ex?.MessagingErrorCode;
                            Console.WriteLine($"[FCM] Token [{batch[j]}] thất bại - ErrorCode: {errorCode}, Message: {ex?.Message}");

                            if (errorCode == MessagingErrorCode.Unregistered || errorCode == MessagingErrorCode.InvalidArgument)
                            {
                                var invalidToken = batch[j];
                                var deviceToRemove = await _context.UserDevices
                                    .FirstOrDefaultAsync(d => d.DeviceToken == invalidToken);
                                if (deviceToRemove != null)
                                {
                                    _context.UserDevices.Remove(deviceToRemove);
                                    Console.WriteLine($"[FCM] Đã xóa token không hợp lệ: {invalidToken}");
                                }
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FCM] Lỗi gửi batch: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gửi push notification tới tất cả thiết bị của những người follow một user.
        /// </summary>
        public async Task SendPushToFollowersAsync(int senderId, string title, string body, Dictionary<string, string>? data = null)
        {
            // Lấy danh sách FollowerId của những người đang follow senderId
            var followerIds = await _context.Follows
                .Where(f => f.FollowingId == senderId)
                .Select(f => f.FollowerId)
                .ToListAsync();

            if (followerIds.Count == 0) return;

            // Lấy tất cả device token của các followers
            var deviceTokens = await _context.UserDevices
                .Where(d => followerIds.Contains(d.UserId))
                .Select(d => d.DeviceToken)
                .ToListAsync();

            if (deviceTokens.Count == 0) return;

            await SendPushToMultipleAsync(deviceTokens, title, body, data);
        }
    }
}