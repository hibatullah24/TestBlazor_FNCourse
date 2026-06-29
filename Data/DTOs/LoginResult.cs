
namespace TestBlazor_FNCourse.Data.DTOs
{
    public class LoginResult
    {
        internal string role;
        internal int userId;
        internal string message;

        public string Message { get; set; } = "";
        public int UserId { get; set; }
        public string Role { get; set; } = "";
        public string UserName { get; set; } = "";
    }
}