using System.Net;

namespace PreStorm
{
    internal class ServiceArgs
    {
        public string Url { get; }
        public ICredentials Credentials { get; }
        public Token Token { get; }
        public string GdbVersion { get; }

        public ServiceArgs(string url, ICredentials credentials, Token token, string gdbVersion)
        {
            if (token != null)
                token.Url = url;

            Url = url;
            Credentials = credentials;
            Token = token;
            GdbVersion = gdbVersion;
        }

        public override bool Equals(object obj)
        {
            var args = obj as ServiceArgs;

            if (args == null)
                return false;

            return args.Url == Url && args.Credentials == Credentials && Equals(args.Token, Token) && args.GdbVersion == GdbVersion;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
