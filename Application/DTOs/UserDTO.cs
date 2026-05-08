using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? FullName { get; set; }

        public string? Bio { get; set; }

        public string? AvatarUrl { get; set; }
        public int FollowersNumber { get; set; }
        public int FollowingsNumber { get; set; }
        public int PostsNumber { get; set; }
        public int gender { get; set; }
    }

    public class UserSummaryDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string ? FullName { get; set; }
    }
    public class UserSummaryFollowDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? FullName { get; set; }
        public bool isFollowing { get; set; }
    }
}
