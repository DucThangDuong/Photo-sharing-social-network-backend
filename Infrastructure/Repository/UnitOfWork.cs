using API.Entities;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly InstagramContext _context;
        public IUserRepository UserRepository { get; }
        public UnitOfWork(InstagramContext context,IUserRepository userRepository)
        {
            _context = context;
            UserRepository = userRepository;

        }
        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
