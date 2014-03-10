using System;
using PreStorm;

namespace PreStorm.Tests
{
    public class IncidentArea : Feature<Polygon>
    {
        [Mapped("permanent_identifier")]
        public virtual string permanent_identifier { get; private set; }

        [Mapped("lifecyclestatus", "LifecycleStatus Domain")]  //If applying this domain conversion, change the property type to string.
        public virtual string lifecyclestatus { get; set; }

        [Mapped("incident_number")]
        public virtual int incident_number { get; set; }

        [Mapped("ftype")]
        public virtual int ftype { get; set; }

        [Mapped("fcode")]
        public virtual int fcode { get; set; }

        [Mapped("collection_time")]
        public virtual DateTime collection_time { get; set; }

        [Mapped("description")]
        public virtual string description { get; set; }
    }
}
