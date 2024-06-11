﻿using Microsoft.AspNetCore.Components.Authorization;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Claims;
using MauiHybridAuth.Shared.Models;
using MauiHybridAuth.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MauiHybridAuth.Web.Services
{
    public class BlazorAuthenticationStateProvider : CustomAuthenticationStateProvider, ICustomAuthenticationStateProvider
    {
        private readonly SignInManager<IdentityUser> _context;

        public BlazorAuthenticationStateProvider(SignInManager<IdentityUser> context)
        {
            _context = context;
        }

        protected override async Task<ClaimsPrincipal> LoginWithProviderAsync(LoginModel loginModel)
        {
            ClaimsPrincipal authenticatedUser;
            LoginStatus = LoginStatus.None;

            try
            {
                
                //Check database for user. Getting an error here... 
                var result = await _context.PasswordSignInAsync(loginModel.Email, loginModel.Password, false, false);
                LoginStatus = result.Succeeded ? LoginStatus.Success : LoginStatus.Failed;

                if (LoginStatus == LoginStatus.Success)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, loginModel.Email) };
                    var identity = new ClaimsIdentity(claims, "Custom authentication");
                    authenticatedUser = new ClaimsPrincipal(identity);
                }
                else
                    authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error logging in: {ex.ToString()}");
                authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return authenticatedUser;
        }        
    }
}

