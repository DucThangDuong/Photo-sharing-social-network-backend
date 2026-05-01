using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);
        Task<List<Application.DTOs.PostDTO>> GetPostsByUserIdAsync(int userId);
    }
}
