﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Timor.Cms.Api.ViewModels.Account;
using Timor.Cms.Infrastructure.Configuration;

namespace Timor.Cms.Api.Controllers
{
    [Route("/api/v1/accounts")]
    public class AccountController : Controller
    {
        private readonly JwtOption _jwtOption;

        public AccountController(IOptionsMonitor<JwtOption> option)
        {
            _jwtOption = option.CurrentValue;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("token")]
        public async Task<LoginResult> Login([FromBody]LoginRequest request)
        {
            // Todo:判断身份信息

            var claims = new[]
            {
                new Claim (JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, request.UserName)
            };

            TimeSpan tokenExpirationDate = TimeSpan.FromDays(1);

            var jwtToken = GenerateJwtToken(claims, tokenExpirationDate);

            await Task.CompletedTask;

            return new LoginResult
            {
                Token = jwtToken,
                TokenType = JwtBearerDefaults.AuthenticationScheme,
                ExpireInSeconds = tokenExpirationDate.TotalSeconds,
                UserName = request.UserName
            };
        }

        private string GenerateJwtToken(Claim[] claims, TimeSpan tokenExpirationDate)
        {
            var now = DateTime.Now;

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOption.SecurityKey));
            var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var securityToken = new JwtSecurityToken(
                issuer: _jwtOption.Issuer,
                audience: _jwtOption.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(tokenExpirationDate),
                signingCredentials: sign
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(securityToken);
            return jwtToken;
        }

        [Authorize]
        [HttpGet("protected")]
        public string ProtectedApi()
        {
            return "jwt token validate success!";
        }
    }
}
