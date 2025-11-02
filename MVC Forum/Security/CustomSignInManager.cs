using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVCForum.Security
{
    public class CustomSignInManager : SignInManager<ForumUser>
    {
        private readonly ISnitzCookie _cookie;
        private readonly UserManager<ForumUser> _userManager;
        public CustomSignInManager(
            UserManager<ForumUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ForumUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ForumUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ForumUser> confirmation,
            ISnitzCookie cookie)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _cookie = cookie;
            _userManager = userManager;
        }

        // Example: Override PasswordSignInAsync to add custom logic
        public override async Task<SignInResult> PasswordSignInAsync(
            string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            // Custom logic before sign-in
            // e.g., log attempts, check custom conditions, etc.

            var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

            // Custom logic after sign-in
            return result;
        }
        public override async Task SignInAsync(ForumUser user, AuthenticationProperties? authenticationProperties, string? authenticationMethod = null)
        {
            // Custom logic before sign-in
            await base.SignInAsync(user, authenticationProperties, authenticationMethod);
            // Custom logic after sign-in
        }
        public override Task RefreshSignInAsync(ForumUser user)
        {
            return base.RefreshSignInAsync(user);
        }
        public override bool IsSignedIn(ClaimsPrincipal principal)
        {
            var baseresult = base.IsSignedIn(principal);
            if (baseresult) {
                string? userTimeZoneId = _cookie.GetCookieValue("CookieTimeZone");
                if (!string.IsNullOrEmpty(userTimeZoneId))
                {
                    var user = _userManager.GetUserAsync(principal).Result;
                    if (user != null)
                    {
                        var existingClaims = _userManager.GetClaimsAsync(user).Result;
                        var newClaim = new Claim("TimeZone", userTimeZoneId);

                        // Check if the claim already exists
                        if (!existingClaims.Any(c => c.Type == newClaim.Type && c.Value == newClaim.Value))
                        {
                            var result = _userManager.AddClaimAsync(user, newClaim).Result;

                        }
                        else
                        {
                            // Optionally, update the claim if the value has changed
                            var oldClaim = existingClaims.FirstOrDefault(c => c.Type == newClaim.Type);
                            if (oldClaim != null && oldClaim.Value != newClaim.Value)
                            {
                                var result = _userManager.ReplaceClaimAsync(user, oldClaim, newClaim).Result;
                            }
                        }
                    }

                }
            }
            return baseresult;
        }
    }
}