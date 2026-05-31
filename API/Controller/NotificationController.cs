using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Extensions;
using System.Threading.Tasks;

namespace API.Controller
{
    [Route("notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                var notifications = await _unitOfWork.NotificationRepository.GetUserNotificationsAsync(userId);
                
                return Ok(new
                {
                    success = true,
                    message = "Notifications retrieved successfully",
                    data = notifications
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving notifications",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                int userId = HttpContext.User.GetUserId();
                bool isSuccess = await _unitOfWork.NotificationRepository.MarkAsReadAsync(id, userId);

                if (!isSuccess)
                {
                    return NotFound(new { success = false, message = "Notification not found or access denied" });
                }

                await _unitOfWork.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Notification marked as read successfully"
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while marking notification as read",
                    error = ex.Message
                });
            }
        }
    }
}
