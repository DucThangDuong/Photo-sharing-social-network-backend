using Application.DTOs;
using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        public Task AddAsync(string username,string fullName, string email, string password);
        public Task<bool> IsFollowUser(int followerId, int followingId);
        public Task AddAsync(User newUser);
        public Task<User?> GetByEmailAsync(string email);
        public Task<bool> EmailExistsAsync(string email);
        public Task<UserDTO?> GetByIdAsync(int id);
        public Task<User?> GetEntityByIdAsync(int id);
        public Task UpdateAsync(User user);
        public Task<List<UserSummaryFollowDTO>> GetSuggestedUsersAsync(int currentUserId, int limit = 10);
        public Task<List<UserSummaryDTO>> SearchUsersAsync(string keyword);
        public Task<bool> FollowUserAsync(int followerId, int followingId);
        public Task<List<UserSummaryDTO>> GetFollowersAsync(int userId);
        public Task<List<UserSummaryFollowDTO>> GetFollowingAsync(int userId);
    }
}
