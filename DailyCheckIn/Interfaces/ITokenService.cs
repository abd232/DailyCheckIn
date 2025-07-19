using DailyCheckIn.Models.Entities;

namespace DailyCheckIn.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
