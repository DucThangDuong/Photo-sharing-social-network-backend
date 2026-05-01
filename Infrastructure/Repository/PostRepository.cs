using API.Entities;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly InstagramContext _context;

        public PostRepository(InstagramContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
        }

        public async Task<List<Application.DTOs.PostDTO>> GetPostsByUserIdAsync(int userId)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new Application.DTOs.PostDTO
                {
                    Id = p.Id,
                    Caption = p.Caption,
                    PostMedia = p.PostMedia.Select(pm => new Application.DTOs.PostMediumDTO 
                    { 
                        MediaUrl = pm.MediaUrl 
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}
