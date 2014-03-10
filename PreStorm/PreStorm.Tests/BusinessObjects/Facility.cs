using System;
using PreStorm;

namespace PreStorm.Tests
{
    public class Facility : Feature<Point>
    {
        [Mapped("facility", "dFacilityTypes")]  //If applying this domain conversion, change the property type to string.
        public virtual string facility { get; set; }

        [Mapped("description")]
        public virtual string description { get; set; }

        [Mapped("quality", "dQuality")]  //If applying this domain conversion, change the property type to string.
        public virtual string quality { get; set; }

        [Mapped("observed")]
        public virtual DateTime? observed { get; set; }

        [Mapped("globalid")]
        public virtual string globalid { get; private set; }
    }
}
