using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;

        public AdminController(UserManager<AppUser> userManager, IPhotoService photoService, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _photoService = photoService;
            _unitOfWork = unitOfWork;

        }

        [Authorize(Policy="RequireAdminRole")]
        [HttpGet("users-with-roles")]

        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
            .Include(ur=>ur.UserRoles)
            .ThenInclude(a=>a.Role)
            .OrderBy(u=>u.UserName)
            .Select(u=> new 
            {
                u.Id,
                Username =u.UserName,
                Roles = u.UserRoles.Select(a=>a.Role.Name).ToList(),
            

            })
            .ToListAsync();

            return Ok(users);
            
           
        }

        [Authorize(Policy="ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]

        public async Task<ActionResult> GetPhotosForModeration()
        {
            var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhoto();
            return Ok(photos);
        }

        [Authorize(Policy="ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]

        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if(photo == null) return NotFound("Could not find photo that you want to see ..");
            
            photo.IsApproved = true;

            var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);
            
            if(!user.Photos.Any(x=>x.IsMain))
            {
                photo.IsMain = true;
            }

            await _unitOfWork.Complete();

            return Ok();
        }

        [Authorize(Policy="ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]

        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if(photo == null) return NotFound("Could not find photo that you want to see..");

            if(photo.PublicId !=null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Result =="ok")
                {
                    _unitOfWork.PhotoRepository.RemovePhoto(photo);
                }
            }
            else
            {
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
            }

            await _unitOfWork.Complete();

            return Ok();

        }

        [HttpPost("edit-roles/{username}")]

        public async Task<ActionResult> EditRoles(string username,[FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();
            var user = await _userManager.FindByNameAsync(username);
            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user,selectedRoles.Except(userRoles));
            if(!result.Succeeded)
            {
                return BadRequest("Failed to add to roles");
            }
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if(!result.Succeeded)
            {
                return BadRequest("Failed to remove from roles");
            }

            return Ok(await _userManager.GetRolesAsync(user));

        }

    }
}