using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PreStorm.Tool
{
    internal static class Text
    {
        private static readonly string[] Keywords = { "abstract", "add", "addhandler", "addressof", "aggregate", "alias", "and", "andalso", "ansi", "as", "ascending", "assembly", "async", "auto", "await", "base", "binary", "bool", "boolean", "break", "by", "byref", "byte", "byval", "call", "case", "catch", "cbool", "cbyte", "cchar", "cdate", "cdbl", "cdec", "char", "checked", "cint", "class", "clng", "cobj", "compare", "const", "continue", "csbyte", "cshort", "csng", "cstr", "ctype", "cuint", "culng", "cushort", "custom", "date", "decimal", "declare", "default", "delegate", "descending", "dim", "directcast", "distinct", "do", "double", "dynamic", "each", "else", "elseif", "end", "endif", "enum", "equals", "erase", "error", "event", "exit", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "friend", "from", "function", "get", "gettype", "getxmlnamespace", "global", "gosub", "goto", "group", "handles", "if", "implements", "implicit", "imports", "in", "inherits", "int", "integer", "interface", "internal", "into", "is", "isfalse", "isnot", "istrue", "iterator", "join", "key", "let", "lib", "like", "lock", "long", "loop", "me", "mid", "mod", "module", "mustinherit", "mustoverride", "mybase", "myclass", "namespace", "narrowing", "new", "next", "not", "nothing", "notinheritable", "notoverridable", "null", "object", "of", "off", "on", "operator", "option", "optional", "or", "order", "orderby", "orelse", "out", "overloads", "overridable", "override", "overrides", "paramarray", "params", "partial", "preserve", "private", "property", "protected", "public", "raiseevent", "readonly", "redim", "ref", "rem", "remove", "removehandler", "resume", "return", "sbyte", "sealed", "select", "set", "shadows", "shared", "short", "single", "sizeof", "skip", "stackalloc", "static", "step", "stop", "strict", "string", "struct", "structure", "sub", "switch", "synclock", "take", "text", "then", "this", "throw", "to", "true", "try", "trycast", "typeof", "uint", "uinteger", "ulong", "unchecked", "unicode", "unsafe", "until", "ushort", "using", "value", "var", "variant", "virtual", "void", "volatile", "wend", "when", "where", "while", "widening", "with", "withevents", "writeonly", "xor", "yield", "feature", "oid", "graphic", "geometry", "point", "multipoint", "polyline", "polygon" };

        public static string ToSafeName(this string text, bool singular, bool? capital = null, Func<string, bool> notConflicting = null)
        {
            var length = text.Length;

            text = Regex.Replace(text, @"\W", "");
            text = Regex.IsMatch(text, @"^\d") ? "_" + text : text;

            if (singular && length > 2)
            {
                if (Regex.IsMatch(text, @"ies$", RegexOptions.IgnoreCase))
                {
                    text = Regex.Replace(text, @"ies$", "y");
                    text = Regex.Replace(text, @"IES$", "Y");
                }
                else if (Regex.IsMatch(text, @"(ch|sh|ss)es$", RegexOptions.IgnoreCase))
                {
                    text = Regex.Replace(text, @"es$", "", RegexOptions.IgnoreCase);
                }
                else
                {
                    text = Regex.Replace(text, @"s$", "", RegexOptions.IgnoreCase);
                }
            }

            if (capital != null)
            {
                if (capital.Value)
                    text = text.Substring(0, 1).ToUpper() + text.Substring(1);
                else
                    text = text.Substring(0, 1).ToLower() + text.Substring(1);
            }

            text = Keywords.Contains(text.ToLower()) ? text + "_" : text;

            if (notConflicting == null)
                return text;

            return Enumerable.Range(0, 100)
                             .Select(n => text + (n == 0 ? "" : n.ToString()))
                             .First(notConflicting);
        }

        public static string Inject(this string text, params object[] parameters)
        {
            return Regex.Replace(text, @"`\d+`", m =>
            {
                var n = int.Parse(Regex.Replace(m.Value, @"\D", ""));
                return parameters[n].ToString();
            });
        }
    }
}
