using PreStorm;

namespace TestClient
{
    public class Incident : Feature<Point>
    {
        [Mapped("req_id")]
        public virtual string req_id { get; set; }

        [Mapped("req_type")]
        public virtual string req_type { get; set; }

        [Mapped("req_date")]
        public virtual string req_date { get; set; }

        [Mapped("req_time")]
        public virtual string req_time { get; set; }

        [Mapped("address")]
        public virtual string address { get; set; }

        [Mapped("district")]
        public virtual string district { get; set; }

        [Mapped("status")]//, "StatusCodes")]  //If applying this domain conversion, change the property type to from short? to string.
        public virtual short? status { get; set; }
    }
}
