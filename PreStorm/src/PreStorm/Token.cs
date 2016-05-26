using System;

namespace PreStorm
{
    /// <summary>
    /// Abstracts the ArcGIS Server token and its generation.
    /// </summary>
    public class Token
    {
        internal string Url;

        private string _token;
        private DateTime _expiry;

        private readonly Func<string, Token> _generateToken;

        /// <summary>
        /// Indicates that a new token has been generated.
        /// </summary>
        public event EventHandler TokenGenerated;

        private Token(string token, DateTime expiry)
        {
            _token = token;
            _expiry = expiry;
        }

        /// <summary>
        /// Initializes a new instance of the Token class based on an existing token string.
        /// </summary>
        /// <param name="token">The token string.</param>
        public Token(string token) : this(token, DateTime.MaxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Token class based on a delegate function for generating the token from the service url.
        /// </summary>
        /// <param name="generateToken"></param>
        public Token(Func<string, Token> generateToken)
        {
            _generateToken = generateToken;
        }

        /// <summary>
        /// Initializes a new instance of the Token class based on the credentials.  When the token is generated this way, it is self-renewing and will not expire.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public Token(string userName, string password) : this(url => GenerateToken(url, userName, password))
        {
        }

        /// <summary>
        /// Initializes a new instance of the Token class based on the credentials.  When the token is generated this way, it is self-renewing and will not expire.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public Token(string url, string userName, string password) : this(userName, password)
        {
            Url = url;
        }

        /// <summary>
        /// Generates a token for the service url.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public static Token GenerateToken(string url, string userName, string password, int? expiration = null)
        {
            var token = Esri.GetTokenInfo(url, userName, password, expiration);
            return new Token(token.token, Esri.BaseTime.AddMilliseconds(token.expires));
        }

        /// <summary>
        /// The time remaining before this token expires.
        /// </summary>
        public double MinutesRemaining => _expiry.Subtract(DateTime.UtcNow).TotalMinutes;

        /// <summary>
        /// Returns the token string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Url != null && _generateToken != null && MinutesRemaining < 0.5)
            {
                var token = _generateToken(Url);
                _token = token._token;
                _expiry = token._expiry;

                TokenGenerated?.Invoke(this, EventArgs.Empty);
            }

            return _token;
        }
    }
}
