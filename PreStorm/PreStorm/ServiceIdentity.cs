using System.Net;

namespace PreStorm
{
    internal class ServiceIdentity
    {
        public string Url { get; private set; }
        public ICredentials Credentials { get; private set; }
        public Token Token { get; private set; }
        public string GdbVersion { get; private set; }

        public ServiceIdentity(string url, ICredentials credentials, Token token, string gdbVersion)
        {
            Url = url;
            Credentials = credentials;
            Token = token;
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
