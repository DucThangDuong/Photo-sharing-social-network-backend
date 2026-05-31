using API.Models;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Context;
namespace Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly InstagramContext _context;
        public IUserRepository UserRepository { get; }
        public IPostRepository PostRepository { get; }
        public IStoryRepository StoryRepository { get; }
        public INotificationRepository NotificationRepository { get; }

        public UnitOfWork(InstagramContext context, IUserRepository userRepository)
        {
            _context = context;
            UserRepository = userRepository;
            PostRepository = new PostRepository(_context);
            StoryRepository = new StoryRepository(_context);
            NotificationRepository = new NotificationRepository(_context);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
