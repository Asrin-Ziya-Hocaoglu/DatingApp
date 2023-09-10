using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace API.Data
{
    public class CachedUserRepository : IUserRepository
    {
        private readonly UserRepository _decorated;
        private readonly IMemoryCache _memoryCache;

        public CachedUserRepository(UserRepository decorated, IMemoryCache memoryCache)
        {
            _decorated = decorated;
            _memoryCache = memoryCache;
        }

        public Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser)
        {
            string key = $"member-{username}";
            return _memoryCache.GetOrCreateAsync(
                key, 
                entry => 
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

                    return _decorated.GetMemberAsync(username, isCurrentUser);
                });
        }

        public Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            string key = $"members";
            return _memoryCache.GetOrCreateAsync(key, entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

                return _decorated.GetMembersAsync(userParams);
            });
        }

        public Task<IEnumerable<AppUser>> GetUserAsync()
        {
            string key = $"member";
            return _memoryCache.GetOrCreateAsync(
                key,
                entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

                    return _decorated.GetUserAsync();
                });
        }

        public Task<AppUser> GetUserByIdAsync(int id)
        {
            string key = $"member-{id}";
            return _memoryCache.GetOrCreateAsync(
                key,
                entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

                    return _decorated.GetUserByIdAsync(id);
                });
        }

        public Task<AppUser> GetUserByPhotoId(int photoId)
        {
            return _decorated.GetUserByPhotoId(photoId);
        }

        public Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return _decorated.GetUserByUsernameAsync(username);
        }

        public Task<string> GetUserGender(string username)
        {
            return _decorated.GetUserGender(username);
        }

        public void Update(AppUser user)
        {
            _decorated.Update(user);
        }
    }
}
