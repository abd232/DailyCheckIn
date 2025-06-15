namespace DailyCheckIn.Models.Entities.Base
{
    public class AuditableEntity
    {
        public Guid? ModifiedById { get; set; }
        public AppUser? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? CreatedById { get; set; }
        public AppUser? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool Deleted { get; set; } = false;
    }
}
