using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace TestBlazor_FNCourse.Services
{
    public class AppAuthState : AuthenticationStateProvider
    {
        private ClaimsPrincipal _current = new ClaimsPrincipal(new ClaimsIdentity());

        public int UserId { get; private set; }
        public string Role { get; private set; } = "";
        public string UserName { get; private set; } = "";
        public bool IsLoggedIn => UserId > 0;

        public void Login(int userId, string role, string name)
        {
            UserId = userId;
            Role = role;
            UserName = name;

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, name)
            }, "app_auth");

            _current = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void Logout()
        {
            UserId = 0;
            Role = "";
            UserName = "";
            _current = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_current));
    }
}