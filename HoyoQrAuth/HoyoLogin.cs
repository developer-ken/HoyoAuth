using System.Collections;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace HoyoQrAuth
{
    public partial class HoyoLogin
    {
        public User HoyoUser { private set; get; }

        public HoyoLogin(string CookiesStr)
        {
            HoyoUser = new(CookiesStr);
        }

        public QRScanHandler GetQRScanHandler() => new QRScanHandler(HoyoUser);
    }
}