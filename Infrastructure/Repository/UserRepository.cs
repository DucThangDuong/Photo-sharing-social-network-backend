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

        public async Task AddAsync(string username, string email, string password)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            User user = new User
            {
                AvatarUrl = null,
                Bio = null,
                CreatedAt = DateTime.UtcNow,
                Email = email,
                FullName = username,
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
                FollowersNumber = u.FollowFollowers.Count(),
                FollowingsNumber = u.FollowFollowings.Count()
            })
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
