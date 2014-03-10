using System;
using PreStorm;

namespace PreStorm.Tests
{
    public class WildfireResponseLine : Feature<Polyline>
    {
        [Mapped("symbolid")]
        public virtual short? symbolid { get; set; }

        [Mapped("timestamp")]
        public virtual DateTime? timestamp { get; set; }

        [Mapped("description")]
        public virtual string description { get; set; }

        [Mapped("created_user")]
        public virtual string created_user { get; private set; }

        [Mapped("created_date")]
        public virtual DateTime? created_date { get; private set; }

        [Mapped("last_edited_user")]
        public virtual string last_edited_user { get; private set; }

        [Mapped("last_edited_date")]
        public virtual DateTime? last_edited_date { get; private set; }
    }
}
