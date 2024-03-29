﻿using AutoMapper;
using Data.Models;
using Data.Roles;
using Funtest.Security;
using Funtest.Services.Interfaces;
using Funtest.TransferObject.Account.Requests;
using Funtest.TransferObject.Admin.Requests;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Funtest.Services
{
    public class AccountService : Service, IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public AccountService(IServiceProvider serviceProvider, IMapper mapper, IEmailService emailService) : base(serviceProvider)
        {
            _mapper = mapper;
            _emailService = emailService;
        }

        public string RemoveDiacritics(string s)
        {
            string asciiEquivalents = Encoding.ASCII.GetString(
                         Encoding.GetEncoding("Cyrillic").GetBytes(s)
                     );

            return asciiEquivalents;
        }

        private string CreateUserName(User user, Product product)
        {
            var userName = $"{user.FirstName}.{user.LastName}";
            userName = userName.Normalize(NormalizationForm.FormD);
            var chars = userName.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            userName = new string(chars).Normalize(NormalizationForm.FormC);
            userName = RemoveDiacritics(userName);


            var duplicat = Context.Users.Where(x => x.FirstName == user.FirstName && user.LastName == x.LastName && x.ProductId == product.Id).Count();

            if (duplicat > 0)
            {
                userName += duplicat;
            }
            var workspace = Regex.Replace(product.Name, @"\s+", "").ToLower();
            return $"{userName.ToLower()}@{workspace}.com";
        }

        public async Task<string> AddNewUser(AddNewUserRequest request, Product product)
        {
            var user = _mapper.Map<User>(request);
            user.Id = Guid.NewGuid().ToString();
            user.Email = request.Email;
            user.UserName = CreateUserName(user, product);
            user.ProductId = request.ProductId;

            var result = await UserManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return null;

            await UserManager.AddToRoleAsync(user, request.Role);
            return user.UserName;
        }

        public async Task<bool> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = Context.Users.Where(x => x.Email == request.Email && x.UserName == request.UserName).FirstOrDefault();

            if (user == null)
            {
                return false;
            }

            var passwordResetToken = await UserManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(passwordResetToken);
            var emailServiceResponse = await _emailService.SendResetPasswordMail(user, encodedToken, "");

            return emailServiceResponse;
        }

        public async Task<bool> ResetPassword(ResetPasswordRequest request)
        {
            var user = await Context.Users.FindAsync(request.UserId);

            var decodedToken = HttpUtility.UrlDecode(request.PasswordResetToken);

            if (request.Password != request.ConfirmedPassword)
                return false;

            var result = await UserManager.ResetPasswordAsync(user, decodedToken, request.Password);
            if (result.Succeeded)
                return true;
            return false;
        }

        public async Task<string> AddInvitedUser(RegisterInvitatedUserRequest request, Product product)
        {
            var user = _mapper.Map<User>(request);
            user.Id = Guid.NewGuid().ToString();
            user.Email = SecureSensitiveData.Decode(request.EmailEncoded);
            user.UserName = CreateUserName(user, product);
            user.ProductId = Guid.Parse(SecureSensitiveData.Decode(request.ProductIdEncoded));

            var result = await UserManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return null;

            await UserManager.AddToRoleAsync(user, request.Role);
            return user.UserName;
        }

        public async Task<bool> RemoveAccount(RemoveAccountRequest request, string pmId)
        {
            var user = await UserManager.FindByNameAsync(request.UserName);

            if (user == null)
                return false;

            var isTheSameTeam = await IsTheSameTeam(pmId, user.Id);
            if (!isTheSameTeam)
                return false;

            user.IsDeleted = true;
            Context.Users.Update(user);

            if (await Context.SaveChangesAsync() == 0)
                return false;
            return true;
        }

        public async Task<bool> IsTheSameTeam(string user1Id, string user2Id)
        {
            var user1 = await Context.Users.FindAsync(user1Id);
            var user2 = await Context.Users.FindAsync(user2Id);

            return user1.ProductId == user2.ProductId;
        }
    }
}
