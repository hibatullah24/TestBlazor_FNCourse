using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace TestBlazor_FNCourse.Services
{
    public class AppAuthState : AuthenticationStateProvider
    {
        private ClaimsPrincipal _current = new ClaimsPrincipal(new ClaimsIdentity());

        // Static so it survives circuit resets
        private static int _userId;
        private static string _role = "";
        private static string _userName = "";

        public int UserId => _userId;
        public string Role => _role;
        public string UserName => _userName;
        public bool IsLoggedIn => _userId > 0;

        public void Login(int userId, string role, string name)
        {
            _userId = userId;
            _role = role;
            _userName = name;

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
            _userId = 0;
            _role = "";
            _userName = "";
            _current = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_current));
    }
}