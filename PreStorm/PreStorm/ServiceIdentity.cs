using System.Net;
using System.Text.RegularExpressions;

namespace PreStorm
{
    internal class ServiceIdentity
    {
        public string Url { get; private set; }
        public bool IsArcGISOnline { get; private set; }
        public ICredentials Credentials { get; private set; }
        public Token Token { get; private set; }
        public string GdbVersion { get; private set; }

        public ServiceIdentity(string url, ICredentials credentials, string userName, string password, string gdbVersion)
        {
            Url = url;
            IsArcGISOnline = Regex.IsMatch(Url, @"\.arcgis\.com/", RegexOptions.IgnoreCase);
            Credentials = credentials;
            Token = userName == null ? null : new Token(url, userName, password, IsArcGISOnline);
            GdbVersion = gdbVersion;
        }

        public override bool Equals(object obj)
        {
            var serviceIdentity = obj as ServiceIdentity;

            if (serviceIdentity == null)
                return false;

            return serviceIdentity.Url == Url && serviceIdentity.Credentials == Credentials && serviceIdentity.Token == Token && serviceIdentity.GdbVersion == GdbVersion;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
