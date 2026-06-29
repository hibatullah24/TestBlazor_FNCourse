

namespace TestBlazor_FNCourse.Data.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public List<UserOrderDto> Orders { get; set; }
    }
}
