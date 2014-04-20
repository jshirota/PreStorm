using System.Net;

namespace PreStorm
{
    internal class ServiceArgs
    {
        public string Url { get; private set; }
        public ICredentials Credentials { get; private set; }
        public Token Token { get; private set; }
        public string GdbVersion { get; private set; }

        public ServiceArgs(string url, ICredentials credentials, string userName, string password, string gdbVersion)
        {
            Url = url;
            Credentials = credentials;
            Token = userName == null ? null : new Token(url, userName, password);
            GdbVersion = gdbVersion;
        }

        public override bool Equals(object obj)
        {
            var args = obj as ServiceArgs;

            if (args == null)
                return false;

            return args.Url == Url && args.Credentials == Credentials && args.Token == Token && args.GdbVersion == GdbVersion;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
