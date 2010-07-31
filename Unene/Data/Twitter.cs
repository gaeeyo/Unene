using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using Photobucket;
using System.Collections.Generic;
using System.Json;
using System.Diagnostics;
using System.Net.Browser;
using System.ComponentModel;
using System.Windows.Threading;
using System.Security;

namespace Unene
{
    public class TwitterAccount
    {
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
        public string ScreenName { get; set; }
        public ulong UserID { get; set; }


        private const string AccessTokenKey = "AccessToken";
        private const string AccessTokenSecretKey = "AccessTokenSecret";
        private const string ScreenNameKey = "ScreenName";
        private const string UserIDKey = "UserID";

        public static bool IsValid(TwitterAccount account)
        {
            return account != null && !string.IsNullOrEmpty(account.AccessToken)
                && !string.IsNullOrEmpty(account.AccessTokenSecret)
                && account.UserID != 0
                && !string.IsNullOrEmpty(account.ScreenName);
        }

        public static TwitterAccount CreateFromApplicationSettings() {
            var s = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
            var a = new TwitterAccount();
            try
            {
                a.AccessToken = (string)s[AccessTokenKey];
                a.AccessTokenSecret = (string)s[AccessTokenSecretKey];
                a.ScreenName = (string)s[ScreenNameKey];
                a.UserID = Convert.ToUInt64(s[UserIDKey]);
                return a;
            }
            catch(Exception) {
                return null;
            }
        }

        public void Save() {
            var s = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
            s[AccessTokenKey] = AccessToken;
            s[AccessTokenSecretKey] = AccessTokenSecret;
            s[ScreenNameKey] = ScreenName;
            s[UserIDKey] = UserID;
            s.Save();
        }
    }

    public class TwitterOAuth : OAuthBase
    {
        const string CONSUMER_KEY = "VbrtykDQoItmyLCkOOGYiw";
        const string CONSUMER_SECRET = "WZinYHIcoKW1f5ijGN5oOZlr9Wa22LmhK3VREksvEo";
        const string REQUEST_TOKEN_URL = "https://twitter.com/oauth/request_token";
        const string ACCESS_TOKEN_URL = "https://twitter.com/oauth/access_token";
        const string AUTHORIZE_URL = "https://twitter.com/oauth/authorize";

        private string oauth_token;
        private string oauth_token_secret;

        public TwitterAccount Account { get; set; }

        public TwitterOAuth()
        {
        }

        private Dictionary<string, string> QueryParamsToDic(string query)
        {
            var dic = new Dictionary<string,string>();
            foreach (string one in query.Split('&')) {
                string[] kv = one.Split('=');
                dic.Add(kv[0], kv[1]);
            }
            return dic;
        }

        /*
        private string GenerateSignature(string url, string token, string token_secret, string method,
            out string normalizedUrl, out string normalizedRequestParameters)
        {
            return GenerateSignature(new Uri(url), CONSUMER_KEY, CONSUMER_SECRET,
                token, token_secret, method, GenerateTimeStamp(), GenerateNonce(),
                out normalizedUrl, out normalizedRequestParameters);
        }
         * */

        public void OAuthRequest(string url, string method, string token, string tokenSecret,
            DownloadStringCompletedEventHandler completed)
        {
            string normalizedUrl, normalizedRequestParameters;
            /*
            string hash = GenerateSignature(new Uri(url), CONSUMER_KEY, CONSUMER_SECRET,
                token, tokenSecret, method,
                GenerateTimeStamp(), GenerateNonce(),
                out normalizedUrl, out normalizedRequestParameters);
             * */
            string hash = GenerateSignature(new Uri(url), CONSUMER_KEY, CONSUMER_SECRET,
                token, tokenSecret, method,
                GenerateTimestamp(), GenerateNonce(), out normalizedUrl, out normalizedRequestParameters);

            MyWebClient client = new MyWebClient();

            string signedUrl = normalizedUrl + "?" + normalizedRequestParameters
                + "&" + OAuthSignatureKey + "=" + UrlEncode(hash);

            client.DownloadStringCompleted += completed;
            client.DownloadStringAsync(new Uri(signedUrl));
        }

        public bool OAuthPost(string url, UploadStringCompletedEventHandler completed,
            string token = null, string tokenSecret = null)
        {
            string method = "POST";
            if (Account == null || string.IsNullOrEmpty(Account.AccessToken) || string.IsNullOrEmpty(Account.AccessTokenSecret))
            {
                return false;
            }
            if (token == null) {
                token = Account.AccessToken;
                tokenSecret = Account.AccessTokenSecret;
            }

            string normalizedUrl, normalizedRequestParameters;
            /*
            string hash = GenerateSignature(new Uri(url), CONSUMER_KEY, CONSUMER_SECRET,
                token, tokenSecret, method,
                GenerateTimestamp(), GenerateNonce(),
                out normalizedUrl, out normalizedRequestParameters);

            string signedUrl = normalizedUrl + "?" + normalizedRequestParameters
                + "&" + OAuthSignatureKey + "=" + UrlEncode(hash);
             * */
            string hash = GenerateSignature(new Uri(url), CONSUMER_KEY, CONSUMER_SECRET,
                token, tokenSecret, method,
                GenerateTimestamp(), GenerateNonce(),
                out normalizedUrl, out normalizedRequestParameters);

            MyWebClient client = new MyWebClient();
            client.UploadStringCompleted += completed;
            client.UploadStringAsync(new Uri(normalizedUrl),
                normalizedRequestParameters
                + "&" + OAuthSignatureKey + "=" + UrlEncode(hash));
            return true;
        }

        public bool OAuthRequest(string url, string method, DownloadStringCompletedEventHandler completed)
        {
            if (Account == null || string.IsNullOrEmpty(Account.AccessToken) || string.IsNullOrEmpty(Account.AccessTokenSecret))
            {
                return false;
            }
            OAuthRequest(url, method, Account.AccessToken, Account.AccessTokenSecret, completed);
            return true;
        }


        /// <summary>
        /// リクエストトークンを取得
        /// </summary>
        public void GetRequestToken(Action<bool,string> completed)
        {
            OAuthRequest(REQUEST_TOKEN_URL, "GET", null, null,  (sender, e) =>
            {
                if (e.Error == null)
                {
                    Dictionary<string, string> query = QueryParamsToDic(e.Result);
                    oauth_token = query["oauth_token"];
                    oauth_token_secret = query["oauth_token_secret"];
                    string authUrl = AUTHORIZE_URL + "?oauth_token=" + oauth_token;
                    completed(true, authUrl);
                }
                else
                {
                    completed(false, e.Error.Message);
                }
            });
            //wc.DownloadStringAsync(new Uri(url));
        }

        /// <summary>
        /// アクセストークンを取得
        /// </summary>
        public void GetAccessToken(string verifier, Action<bool, string> completed)
        {
            OAuthRequest(ACCESS_TOKEN_URL + "?oauth_verifier=" + verifier, 
                "GET", oauth_token, oauth_token_secret, (sender, e) =>
            {
                if (e.Error == null)
                {
                    //oauth_token=5458012-PNBuQ6aI4gbC1nprVuOF4XsyXuIt65GSHAyCZGnKl8&oauth_token_secret=lzpqHYwQD2thDdE33cijSenuAubkqY7TJbXdEvHs4&user_id=5458012&screen_name=gaeeyo
                    Dictionary<string, string> query = QueryParamsToDic(e.Result);

                    try {
                        var newAccount = new TwitterAccount()
                        {
                            AccessToken = query["oauth_token"],
                            AccessTokenSecret = query["oauth_token_secret"],
                            UserID = Convert.ToUInt64(query["user_id"]),
                            ScreenName = query["screen_name"],
                        };
                        Account = newAccount;
                        completed(true, "");
                    }
                    catch (Exception) {
                        completed(false, "サーバからの応答が想定外でした。");
                    }
                }
                else
                {
                    completed(false, e.Error.Message);
                }
            });
        }

        /// <summary>
        /// タイムラインを取得
        /// </summary>
        public void GetTimeline(string query, Action<bool, string> completed)
        {
            OAuthRequest(query,
                "GET", (object sender, DownloadStringCompletedEventArgs e) =>
            {
                if (e.Error == null)
                {
                    completed(true, e.Result);
                }
                else
                {
                    completed(false, e.Cancelled ? "タイムアウト" : e.Error.Message);
                }
            });
        }

        public static string MakeHomeQuery(ulong id)
        {
            string query = "count=200";
            if (id != 0)
            {
                query = "max_id=" + id.ToString();
            }
            return "http://api.twitter.com/1/statuses/home_timeline.json?" + query;
        }

        public static string MakeListQuery(string user_and_list, ulong id)
        {
            string [] user_list = user_and_list.Split('/');
            if (user_list.Length != 2)
            {
                throw new Exception("リストの設定が不正です");
            }

            string query = "per_page=200";
            if (id != 0)
            {
                query = "max_id=" + id.ToString();
            }
            return "http://api.twitter.com/1/"
                + user_list[0]
                + "/lists/"
                + user_list[1]
                + "/statuses.json?" + query;
        }

        public static string MakeSearchQuery(string text, ulong id)
        {
            string query = "q="+ HttpUtility.UrlEncode(text);
            if (id != 0)
            {
                query += "&since_id=" + id.ToString();
            }
            return "http://search.twitter.com/search.json?" + query + "&rpp=100";
        }

    }
    
    public class Twitter : TwitterOAuth
    {
        public static string GetUserUrl(string user)
        {
            return "http://twitter.com/"+user;
        }

        public void StatusesUpdate(string status, ulong in_reply_to_status_id, Action<bool,string> completed)
        {
            //string query = "http://api.twitter.com/1/statuses/update.json";
            string query = "http://twitter.com/statuses/update.json";
            query += "?status=" + UrlEncode(status);
            OAuthPost(query, (sender, e) =>
            {
                if (e.Error == null)
                {
                    completed(true, e.Result);
                }
                else
                {
                    completed(false, e.Error.Message);
                }
            });
        }
    }

    public class MyWebClient
    {
        WebClient       _wc;
        DispatcherTimer _timeoutTimer = new DispatcherTimer();

        // コンストラクタ
        public MyWebClient()
        {
            _timeoutTimer.Interval = TimeSpan.FromSeconds(15);
            _timeoutTimer.Tick += new EventHandler(_timeoutTimer_Tick);
            _wc = new WebClient();
        }

        void _timeoutTimer_Tick(object sender, EventArgs e)
        {
            _timeoutTimer.Stop();
            _wc.CancelAsync();   
        }

        // タイムアウト
        public TimeSpan Timeout
        {
            get { return _timeoutTimer.Interval;  }
            set { _timeoutTimer.Interval = value; }
        }

        public event DownloadStringCompletedEventHandler DownloadStringCompleted;
        public event UploadStringCompletedEventHandler UploadStringCompleted;

        // DownloadStringAsync
        public void DownloadStringAsync(Uri address)
        {
            _wc.DownloadStringCompleted += (s, e) =>
            {
                _timeoutTimer.Stop();
                DownloadStringCompleted(s, e);
            };
            _wc.DownloadStringAsync(address);
            _timeoutTimer.Start();
        }

        // UploadStringAsync
        public void UploadStringAsync(Uri address, string data)
        {
            _wc.UploadStringCompleted += (s, e) =>
            {
                _timeoutTimer.Stop();
                UploadStringCompleted(s, e);
            };
            _wc.UploadStringAsync(address, data);
            _timeoutTimer.Start();
        }
    }




}
