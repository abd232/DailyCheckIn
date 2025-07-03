namespace DailyCheckIn.Models.DTO
{
    public class UserLoginDTO
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public bool KeepLogin { get; set; }
    }
}
