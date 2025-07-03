using Microsoft.AspNetCore.Identity;

namespace DailyCheckIn.Models.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string Name { get; set; } = "";
        public string ArabicName { get; set; } = "";
        public double HourlyRate { get; set; }
        public int Bonus { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public ICollection<Attendance> Attendances { get; set; } = [];
        public ICollection<Advance> Advances { get; set; } = [];
        public DateOnly StartDate { get; set; }
        public Guid? ModifiedById { get; set; }
        public AppUser? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? CreatedById { get; set; }
        public AppUser? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool Deleted { get; set; } = false;
    }
}
