using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper  _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger _logger;
        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,ITokenService tokenService, IMapper mapper,ILogger<AccountController> logger  )
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _mapper = mapper;                                    
            _signInManager = signInManager;
            _logger =logger;

        }


        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.UserName))
            {
                return BadRequest("Username is Taken by someone");
            }

            var user = _mapper.Map<AppUser>(registerDto);
            
            
                user.UserName = registerDto.UserName.ToLower();
                   
            var result = await _userManager.CreateAsync(user,registerDto.Password);

            if(!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user,"Member");
            if(!roleResult.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = 
            await _userManager.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x=>x.UserName==loginDto.Username.ToLower());

            if(user == null)
            {
                return Unauthorized("Invalid Username");
            }

            var result = await _signInManager
            .CheckPasswordSignInAsync(user,loginDto.Password,false);

            if(!result.Succeeded)
            {
                return Unauthorized();
            }
            
            _logger.LogInformation("giris yapıldı");
            _logger.LogError("Hatalı giris yapıldı");
            
                      
            return new UserDto{
                Username=user.UserName,
                Token = await _tokenService.CreateToken(user),               
                PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };

        }



        private async Task<bool> UserExists(string userName)
        {
            return await _userManager.Users.AnyAsync(x=>x.UserName == userName.ToLower());
        }
    }
}