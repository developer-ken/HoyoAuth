using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HoyoQrAuth.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

public class User
{
    const string SALT = "PVeGWIZACpxXZ1ibMVJPi9inCY4Nd4y2";
    public string WebCookie { get; private set; }
    public string Uid { get; private set; }
    public string Ticket { get; private set; }
    public string Stoken { get; private set; }
    public string GameToken { get; private set; }
    public string DeviceId { get; private set; }
    public dynamic Role { get; private set; }



    private HttpClientHandler handler = new HttpClientHandler()
    {
        CookieContainer = new CookieContainer(),
        UseCookies = true
    };

    private HttpClient client;

    public User(string webCookie)
    {
        WebCookie = webCookie;
        client = new HttpClient(handler);
        var webCookieDict = ParseCookie(webCookie);
        Uid = webCookieDict["login_uid"];
        Ticket = webCookieDict["login_ticket"];
        Stoken = GetStoken(webCookieDict).Result;
        GameToken = GetGameToken().Result;
        this.DeviceId = Guid.NewGuid().ToString();

        foreach (var citem in webCookieDict)
        {
            handler.CookieContainer.Add(new Cookie(citem.Key, citem.Value, "/", ".mihoyo.com"));
        }
    }

    public User(string webCookie, string DeviceId)
    {
        WebCookie = webCookie;
        client = new HttpClient(handler);
        var webCookieDict = ParseCookie(webCookie);
        Uid = webCookieDict["login_uid"];
        Ticket = webCookieDict["login_ticket"];
        Stoken = GetStoken(webCookieDict).Result;
        GameToken = GetGameToken().Result;
        this.DeviceId = DeviceId;

        foreach (var citem in webCookieDict)
        {
            handler.CookieContainer.Add(new Cookie(citem.Key, citem.Value, "/", ".mihoyo.com"));
        }
    }

    private static Dictionary<string, string> ParseCookie(string webCookie)
    {
        var cookie_ = webCookie.Trim().Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Trim().Split('=', 2))
            .Where(pair => pair.Length == 2)
            .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());
        return cookie_;
    }

    private async Task<string> GetStoken(Dictionary<string, string> webCookieDict)
    {
        var url = "https://api-takumi.mihoyo.com/auth/api/getMultiTokenByLoginTicket";
        var params_ = new Dictionary<string, string>
        {
            {"login_ticket", webCookieDict["login_ticket"]},
            {"token_types", "3"},
            {"uid", webCookieDict["login_uid"]}
        };
        var response = await client.GetAsync(url + "?" + await new FormUrlEncodedContent(params_).ReadAsStringAsync());
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var resstring = await response.Content.ReadAsStringAsync();
            var tokens = JsonConvert.DeserializeObject<dynamic>(resstring);
            if (tokens.message == "OK")
            {
                foreach (var item in tokens.data.list)
                {
                    if (item.name == "stoken")
                    {
                        return item.token;
                    }
                }
            }
            throw new WebApiException("Get STOKEN denied by remote server", resstring);
        }
        throw new WebApiException("HTTP request code != OK");
    }

    private async Task<string> GetGameToken()
    {
        var url = "https://api-takumi.mihoyo.com/auth/api/getGameToken";
        var params_ = new Dictionary<string, string>
        {
            {"stoken", Stoken},
            {"uid", Uid}
        };
        var response = await client.GetAsync(url + "?" + await new FormUrlEncodedContent(params_).ReadAsStringAsync());
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var resstring = await response.Content.ReadAsStringAsync();
            var jsonLoads = JsonConvert.DeserializeObject<dynamic>(resstring);
            if (jsonLoads.message == "OK")
            {
                return jsonLoads.data.game_token;
            }
            throw new WebApiException("Get GameToken denied by remote server", resstring);
        }
        throw new WebApiException("HTTP request code != OK");
    }

    private static string GetDS()
    {
        string time_now = ((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString();
        string rand = new Random().Next(100001, 200000).ToString();
        string m = $"salt={SALT}&t={time_now}&r={rand}";
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(m));
            return $"{time_now},{rand},{BitConverter.ToString(hash).Replace("-", "")}";
        }
    }

    public async Task<JObject> Post(string url, JObject json)
    {
        if (client.DefaultRequestHeaders.Contains("DS"))
        {
            client.DefaultRequestHeaders.Remove("DS");
        }
        client.DefaultRequestHeaders.Add("DS", GetDS());

        var content = new StringContent(json.ToString(Formatting.None), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();
        return JObject.Parse(responseString);
    }

    public override string ToString()
    {
        return $"name：{Role[0].nickname}\t{nameof(Uid)}={Uid}";
    }
}

public static class ApiPath
{
    public const string Scan = "https://api-sdk.mihoyo.com/hk4e_cn/combo/panda/qrcode/scan";
    public const string Confirm = "https://api-sdk.mihoyo.com/hk4e_cn/combo/panda/qrcode/confirm";
}