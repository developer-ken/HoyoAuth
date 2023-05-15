using HoyoQrAuth;
using HoyoQrAuth.Models;

namespace HoyoAuthCli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string COOKIESSTR = "";                             //https://user.mihoyo.com/ 登录后复制Cookies字符串
            HoyoLogin hoyo = new HoyoLogin(COOKIESSTR);               //初始化HoyoLogin库
            var scan = hoyo.GetQRScanHandler();                       //获取二维码处理装置
            Console.WriteLine("扫描登录码，然后在这里输入扫出的网址：");
            var uri = Console.ReadLine();
            var qrcode = new LoginQr(uri);                            //解析二维码URI
            scan.Scan(qrcode);                                        //使用二维码处理器执行“扫描”操作
            Console.WriteLine("已扫码");
            Console.ReadLine();
            scan.ConfirmLogin();                                      //确认登录游戏
            Console.WriteLine("已登录");
        }
    }
}