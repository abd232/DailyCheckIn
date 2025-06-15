using DailyCheckIn.Models.Entities;

namespace DailyCheckIn.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
