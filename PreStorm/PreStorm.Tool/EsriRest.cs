namespace PreStorm.Tool
{
    internal abstract class Response
    {
        public Error error { get; set; }
    }

    internal class Error
    {
        public int code { get; set; }
        public string message { get; set; }
        public string[] details { get; set; }
    }

    internal class ServiceInfo : Response
    {
        public double currentVersion { get; set; }
        public Layer[] layers { get; set; }
        public Layer[] tables { get; set; }
        public string capabilities { get; set; }
    }

    internal class Layer
    {
        public int id { get; set; }
        public string name { get; set; }
        public int[] subLayerIds { get; set; }
    }

    internal class LayerInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string geometryType { get; set; }
        public string displayField { get; set; }
        public Field[] fields { get; set; }
        public string capabilities { get; set; }
    }

    internal class Field
    {
        public string name { get; set; }
        public string type { get; set; }
        public string alias { get; set; }
        public int length { get; set; }
        public bool editable { get; set; }
        public bool nullable { get; set; }
        public Domain domain { get; set; }
    }

    public class Domain
    {
        public string type { get; set; }
        public string name { get; set; }
    }
}
