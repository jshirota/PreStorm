using System;

namespace PreStorm
{
    internal class Token
    {
        private readonly string _url;
        private readonly string _userName;
        private readonly string _password;
        private readonly bool _isArcGISOnline;
        private DateTime? _expiry;
        private string _token;

        public Token(string url, string userName, string password, bool isArcGISOnline)
        {
            _url = url;
            _userName = userName;
            _password = password;
            _isArcGISOnline = isArcGISOnline;
        }

        public override string ToString()
        {
            if (_expiry == null || _expiry.Value.Subtract(DateTime.UtcNow).TotalMinutes < 1)
            {
                if (_isArcGISOnline)
                {
                    var t = Esri.GetArcGISOnlineTokenInfo(_userName, _password);
                    _expiry = DateTime.UtcNow.AddMinutes(t.expires_in);
                    _token = t.access_token;
                }
                else
                {
                    var t = Esri.GetTokenInfo(_url, _userName, _password);
                    _expiry = Config.BaseTime.AddMilliseconds(t.expires);
                    _token = t.token;
                }
            }

            return _token;
        }

        public override bool Equals(object obj)
        {
            var token = obj as Token;

            if (token == null)
                return false;

            return token._url == _url && token._userName == _userName && token._password == _password;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
