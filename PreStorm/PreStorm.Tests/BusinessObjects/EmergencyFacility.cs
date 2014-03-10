using System;
using PreStorm;

namespace PreStorm.Tests
{
    public class EmergencyFacility : Feature<Point>
    {
        [Mapped("facilityid")]
        public virtual string ID { get; set; }

        [Mapped("facname")]
        public virtual string Name { get; set; }

        [Mapped("factype", "EmergyFacilityType")]
        public virtual string Type { get; set; }

        [Mapped("pocphone")]
        public virtual string Phone { get; set; }

        [Mapped("opendate")]
        public virtual DateTime? OpenDate { get; set; }

        [Mapped("closeddate")]
        public virtual DateTime? ClosedDate { get; set; }

        [Mapped("opsstatus", "OperationalStatus")]
        public virtual string Status { get; set; }
    }
}
