using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork   
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        public UnitOfWork(DataContext context, IMapper mapper, IMemoryCache memoryCache)
        {
            _context = context;
            _mapper = mapper;
            _memoryCache = memoryCache;
        }
        //public IUserRepository UserRepository => new UserRepository(_context , _mapper);

        public IMessageRepository MessageRepository => new MessageRepository(_context , _mapper);

        public ILikesRepository LikesRepository => new LikesRepository(_context);

        public IPhotoRepository PhotoRepository => new PhotoRepository (_context);

        public IUserRepository UserRepository => new CachedUserRepository(new UserRepository(_context, _mapper), _memoryCache);

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool hasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}