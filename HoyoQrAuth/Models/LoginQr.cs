using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HoyoQrAuth.Models
{
    public class LoginQr
    {
        public readonly string QRRawUri;
        public readonly string AppName;
        public readonly string BizKey;
        public readonly string Ticket;
        public readonly int AppId;
        public readonly int Expire;
        public readonly bool Bbs;

        public LoginQr(string qruri)
        {
            QRRawUri = qruri;
            AppId = RegGetInt(qruri, "app_id=([0-9]+)");
            Expire = RegGetInt(qruri, "expire=([0-9]+)");
            Bbs = RegGetBool(qruri, "bbs=([truefals]+)");
            AppName = RegGet(qruri, "app_name=([A-Z0-9%]+)").ToString();
            BizKey = RegGet(qruri, "biz_key=([0-9a-zA-Z_]+)").ToString();
            Ticket = RegGet(qruri, "ticket=([0-9a-zA-Z]+)").ToString();
        }

        private string? RegGet(string text, string pattern)
        {
            var match = Regex.Match(text, pattern);
            if (match.Success && match.Groups.Count >= 2)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        private int RegGetInt(string text, string pattern)
        {
            int result;
            if (int.TryParse(RegGet(text, pattern), out result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }

        private bool RegGetBool(string text, string pattern)
        {
            return RegGet(text, pattern)?.ToLower() == "true";
        }
    }
}
