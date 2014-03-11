using System.Text;

namespace PreStorm.Tool
{
    internal class CodeGenerator
    {
        private readonly string _geometryType;
        private readonly bool _hasNamespace;
        private readonly bool _isMapServer;
        private readonly StringBuilder _code = new StringBuilder();

        #region Private

        private void AppendUsingStatements()
        {
            _code.AppendLine("using System;\r\nusing PreStorm;\r\n");
        }

        private void AppendBeginning(string namespaceName, string className)
        {
            if (_hasNamespace)
                _code.AppendFormat("namespace {0}\r\n{{\r\n    ", namespaceName);

            _code.AppendFormat("public class {0} : Feature{1}\r\n{2}{{", className, _geometryType == null ? "" : string.Format("<{0}>", _geometryType), "    ");
        }

        private void AppendProperty(Field field, string className)
        {
            if (field.type == "esriFieldTypeOID")
                return;

            string csType;

            switch (field.type)
            {
                case "esriFieldTypeInteger":
                    csType = "int";
                    break;
                case "esriFieldTypeSmallInteger":
                    csType = "short";
                    break;
                case "esriFieldTypeDouble":
                    csType = "double";
                    break;
                case "esriFieldTypeSingle":
                    csType = "float";
                    break;
                case "esriFieldTypeString":
                case "esriFieldTypeGUID":
                case "esriFieldTypeGlobalID":
                    csType = "string";
                    break;
                case "esriFieldTypeDate":
                    csType = "DateTime";
                    break;
                default:
                    return;
            }

            if ((_isMapServer || field.nullable) && field.type != "esriFieldTypeString" && field.type != "esriFieldTypeGlobalID")
                csType += "?";

            var indentation = _hasNamespace ? "    " : "";
            var propertyName = field.name.ToSafeName(false, null, null, className);

            if (field.domain != null && field.domain.type == "codedValue")
                _code.AppendFormat("\r\n{0}    [Mapped(\"{1}\")]//, \"{2}\")]{3}\r\n{0}    public virtual {4} {5} {{ get; {6}set; }}\r\n",
                    indentation,
                    field.name,
                    field.domain.name,
                    csType != "string" ? string.Format("  //If applying this domain conversion, change the property type to from {0} to string.", csType) : "",
                    csType,
                    propertyName,
                    field.editable ? "" : "private ");
            else
                _code.AppendFormat("\r\n{0}    [Mapped(\"{1}\")]\r\n{0}    public virtual {2} {3} {{ get; {4}set; }}\r\n",
                    indentation,
                    field.name,
                    csType,
                    propertyName,
                    field.editable ? "" : "private ");
        }

        private void AppendEnding()
        {
            _code.AppendLine(_hasNamespace ? "    }\r\n}" : "}");
        }

        #endregion

        public CodeGenerator(string geometryType, Field[] fields, string namespaceName, string className, bool isMapServer)
        {
            _isMapServer = isMapServer;

            switch (geometryType)
            {
                case "esriGeometryPoint":
                    _geometryType = "Point";
                    break;
                case "esriGeometryMultipoint":
                    _geometryType = "Multipoint";
                    break;
                case "esriGeometryPolyline":
                    _geometryType = "Polyline";
                    break;
                case "esriGeometryPolygon":
                    _geometryType = "Polygon";
                    break;
                default:
                    _geometryType = null;
                    break;
            }

            _hasNamespace = !string.IsNullOrEmpty(namespaceName);

            AppendUsingStatements();

            AppendBeginning(namespaceName, className);

            foreach (var field in fields)
                AppendProperty(field, className);

            AppendEnding();
        }

        public string ToCSharp()
        {
            return _code.ToString();
        }
    }
}
