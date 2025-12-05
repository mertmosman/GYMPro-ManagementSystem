using System;

namespace GymApp.Web.Entities
{
    public class GymSchedule : BaseEntity
    {
        public DayOfWeek Day { get; set; } // 0=Pazar, 1=Pazartesi...
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public bool IsClosed { get; set; } // O gün tatil mi?

        public int GymId { get; set; }
        public Gym? Gym { get; set; }
    }
}