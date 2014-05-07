using System;

namespace PreStorm
{
    /// <summary>
    /// Abstracts the ArcGIS Server token and its generation.
    /// </summary>
    public class Token
    {
        internal string Url;

        private readonly bool _isStatic;
        private readonly string _userName;
        private readonly string _password;
        private string _token;
        private DateTime? _expiry;

        private Token(string userName, string password, string token)
        {
            _userName = userName;
            _password = password;
            _token = token;
        }

        /// <summary>
        /// Initializes a new instance of the Token class based on the credentials.  When the token is generated this way, it is self-renewing and will not expire.
        /// </summary>
        /// <param name="userName">The user name for the ArcGIS Server authentication.</param>
        /// <param name="password">The password for the ArcGIS Server authentication.</param>
        public Token(string userName, string password) : this(userName, password, null) { }

        /// <summary>
        /// Initializes a new instance of the Token class based on an existing token string.
        /// </summary>
        /// <param name="token">The token string.</param>
        public Token(string token)
            : this(null, null, token)
        {
            _isStatic = true;
        }

        /// <summary>
        /// Returns the token string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_isStatic)
                return _token;

            if (_expiry == null || _expiry.Value.Subtract(DateTime.UtcNow).TotalMinutes < 1)
            {
                var t = Esri.GetTokenInfo(Url, _userName, _password);
                _expiry = Esri.BaseTime.AddMilliseconds(t.expires);
                _token = t.token;
            }

            return _token;
        }

        /// <summary>
        /// Overridden to return the value equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var token = obj as Token;

            if (token == null)
                return false;

            return token.Url == Url && token._userName == _userName && token._password == _password;
        }

        /// <summary>
        /// Overridden to always return zero.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
