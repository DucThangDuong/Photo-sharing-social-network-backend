using API.Entities;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly InstagramContext _context;
        public UserRepository(InstagramContext context)
        {
            _context = context;
        }

        public async Task AddAsync(string username, string fullName, string email, string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            User user = new User
            {
                AvatarUrl = null,
                Bio = null,
                CreatedAt = DateTime.UtcNow,
                Email = email,
                FullName = fullName,
                Username = username,
                PasswordHash = passwordHash
            };
            await _context.Users.AddAsync(user);
        }

        public async Task AddAsync(User newUser)
        {
            await _context.Users.AddAsync(newUser);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            return await _context.Users.Select(u => new UserDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                Bio = u.Bio,
                AvatarUrl = u.AvatarUrl,
                FollowersNumber = u.FollowFollowings.Count(),
                FollowingsNumber = u.FollowFollowers.Count(),
                PostsNumber = u.Posts.Count(),
                gender=u.Gender
            })
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetEntityByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        public async Task<List<UserSummaryFollowDTO>> GetSuggestedUsersAsync(int currentUserId, int limit = 10)
        {
            return await _context.Users
                .Where(u => u.Id != currentUserId) 
                .OrderBy(u => Guid.NewGuid())      
                .Take(limit)
                .Select(u => new UserSummaryFollowDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    AvatarUrl = u.AvatarUrl,
                    FullName = u.FullName,
                    isFollowing = u.FollowFollowings.Any(f => f.FollowerId == currentUserId)
                })
                .ToListAsync();
        }

        public async Task<List<UserSummaryDTO>> SearchUsersAsync(string keyword)
        {
            return await _context.Users
                .Where(u => u.Username.Contains(keyword) || (u.FullName != null && u.FullName.Contains(keyword)))
                .Take(20)
                .Select(u => new UserSummaryDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    AvatarUrl = u.AvatarUrl,
                    FullName = u.FullName
                })
                .ToListAsync();
        }

        public async Task<bool> FollowUserAsync(int followerId, int followingId)
        {
            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (existingFollow != null)
            {
                _context.Follows.Remove(existingFollow);
                return false; // Unfollowed
            }

            await _context.Follows.AddAsync(new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow
            });
            return true; // Followed
        }

        public async Task<List<UserSummaryDTO>> GetFollowersAsync(int userId)
        {
            return await _context.Follows
                .Where(f => f.FollowingId == userId)
                .Select(f => new UserSummaryDTO
                {
                    Id = f.Follower.Id,
                    Username = f.Follower.Username,
                    AvatarUrl = f.Follower.AvatarUrl,
                    FullName = f.Follower.FullName
                })
                .ToListAsync();
        }

        public async Task<List<UserSummaryFollowDTO>> GetFollowingAsync(int userId)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => new UserSummaryFollowDTO
                {
                    Id = f.Following.Id,
                    Username = f.Following.Username,
                    AvatarUrl = f.Following.AvatarUrl,
                    FullName = f.Following.FullName,
                    isFollowing = f.Following.FollowFollowings.Any(ff => ff.FollowerId == userId)
                })
                .ToListAsync();
        }
    }
}
