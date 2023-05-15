using HoyoQrAuth.Exceptions;
using HoyoQrAuth.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoyoQrAuth
{
    public class QRScanHandler
    {
        private User User;
        private LoginQr Qrcode;

        public QRScanHandler(User user)
        {
            User = user;
        }

        public async Task<QRScanHandler> Scan(LoginQr qrcode)
        {
            JObject raw = new JObject();
            raw.Add("uid", User.Uid);
            raw.Add("token", User.GameToken);
            //JObject pl = new JObject();
            //pl.Add("proto", "Account");
            //pl.Add("raw", raw.ToString());

            JObject jb = new JObject();
            jb.Add("app_id",qrcode.AppId);
            jb.Add("device",User.DeviceId);
            //jb.Add("payload",pl);
            jb.Add("ticket",qrcode.Ticket);

            var result = await User.Post(ApiPath.Scan, jb);

            if (result.Value<int>("retcode") != 0)
            {
                throw new WebApiException("Failed to scan QRCode.",result.ToString());
            }
            Qrcode = qrcode;
            return this;
        }

        public async Task ConfirmLogin()
        {
            var qrcode = Qrcode;
            JObject raw = new JObject();
            raw.Add("uid", User.Uid);
            raw.Add("token", User.GameToken);
            JObject pl = new JObject();
            pl.Add("proto", "Account");
            pl.Add("raw", raw.ToString());

            JObject jb = new JObject();
            jb.Add("app_id", qrcode.AppId);
            jb.Add("device", User.DeviceId);
            jb.Add("payload",pl);
            jb.Add("ticket", qrcode.Ticket);

            var result = await User.Post(ApiPath.Confirm, jb);

            if (result.Value<int>("retcode") != 0)
            {
                throw new WebApiException("Failed to confirm login.", result.ToString());
            }
        }
    }
}
