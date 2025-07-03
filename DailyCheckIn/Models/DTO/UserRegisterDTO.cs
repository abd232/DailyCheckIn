namespace DailyCheckIn.Models.DTO
{
    public class UserRegisterDTO
    {
        public required string UserName { get; set; }
        public required string ArabicName { get; set; }
        public required string? Name { get; set; }
        public string? Email { get; set; }
        public required string Password { get; set; }
        public string? PhoneNumber { get; set; }
        public required double HourlyRate { get; set; }
        public required int Bonus { get; set; } = 0;
        public required DateOnly StartDate { get; set; }
    }
}
