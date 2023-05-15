using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoyoQrAuth.Exceptions
{
    internal class WebApiException : Exception
    {
        readonly string? Rawresponse;
        public WebApiException(string msg) : base(msg) { }

        public WebApiException(string msg, string payload) : base(msg)
        {
            Rawresponse = payload;
        }
    }
}
