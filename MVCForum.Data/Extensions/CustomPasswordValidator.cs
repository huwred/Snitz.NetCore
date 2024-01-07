using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace SnitzCore.Data.Extensions
{
    public class CustomPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string? password)
        {
            var username = await manager.GetUserNameAsync(user);
            if (username != null && username.ToLower().Equals(password?.ToLower()))
                return IdentityResult.Failed(new IdentityError { Description = "Username and Password can't be the same.", Code = "SameUserPass" });
            return password != null && password.ToLower().Contains("password") ? IdentityResult.Failed(new IdentityError { Description = "The word password is not allowed for the Password.", Code = "PasswordContainsPassword" }) : IdentityResult.Success;
        }
    }
}
