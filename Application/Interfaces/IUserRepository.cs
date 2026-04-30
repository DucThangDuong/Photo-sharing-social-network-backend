using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        public Task AddAsync(string username, string email, string password);
        public Task AddAsync(User newUser);
        public Task<User?> GetByEmailAsync(string email);
        public Task<bool> EmailExistsAsync(string email);
        public Task<UserDTO?> GetByIdAsync(int id);
    }
}
