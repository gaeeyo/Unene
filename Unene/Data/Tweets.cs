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
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO.IsolatedStorage;
using System.Xml;

namespace Unene
{
    [DataContract(Namespace="Tweet")]
    public class Tweet
    {
        [DataMember]
        public string ScreenName { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public ulong Id { get; set; }
        [DataMember]
        public ulong UserId { get; set; }
        [DataMember]
        public string Image { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public ulong ReplyToID { get; set; }
        [DataMember]
        public string ReplyToScreenName { get; set; }
        [DataMember]
        public byte Age { get; set; }
        [DataMember]
        public ulong RetweetId { get; set; }
        [DataMember]
        public string RetweetUserScreenName { get; set; }

        public Tweet(ulong s_id, string s_text, DateTime s_time, ulong s_userId, 
            string s_screenName, string s_source, string s_image, 
            string s_name, ulong s_replyToId, string s_replyToScreenName,
            ulong s_retweetId,
            string s_retweetUserScreenName) 
        {
            this.Id = s_id;
            this.Text = s_text;
            this.Time = s_time;
            this.UserId = s_userId;

            this.ScreenName = s_screenName;
            this.Source = s_source;
            this.Image = s_image;

            this.Name = s_name;
            this.ReplyToID = s_replyToId;
            this.ReplyToScreenName = s_replyToScreenName;
            this.Age = 255;
            this.RetweetId = s_retweetId;
            this.RetweetUserScreenName = s_retweetUserScreenName;
        }

        public string StatusUrl {
            get
            {
                return "http://twitter.com/" + ScreenName + "/statuses/" + 
                    (this.RetweetId != 0 ? this.RetweetId.ToString() :  Id.ToString());
            }
        }

        public string BigImage
        {
            get
            {
                return Image.Replace("_normal.", "_bigger.");
            }
        }
    }

    public class Tweets : List<Tweet>
    {


        /// <summary>
        /// JSONをパースしてtimelineに
        /// </summary>
        public static Tweets ParseTimelineFromJson(string json)
        {
            Tweets tweets = new Tweets();
            foreach (JsonValue jv in JsonValue.Parse(json))
            {
                if (jv == null) continue;
                string created_at = (string)jv["created_at"];
                string [] time = created_at.Split(' ');

                Tweet tweet;

                if (jv.ContainsKey("retweeted_status"))
                {
                    JsonValue rt = jv["retweeted_status"];
                    tweet = new Tweet(
                        jv["id"],
                        System.Windows.Browser.HttpUtility.HtmlDecode(rt["text"]),
                        DateTime.Parse(time[0] + ',' + time[2] + ' ' + time[1] + ' ' + time[5] + ' ' + time[3] + time[4]),
                        rt["user"]["id"],
                        rt["user"]["screen_name"],
                        rt["source"],
                        rt["user"]["profile_image_url"],
                        rt["user"]["name"],
                        rt["in_reply_to_status_id"] ?? 0,
                        rt["in_reply_to_screen_name"],
                        rt["id"],
                        jv["user"]["screen_name"]
                        );
                }
                else
                {
                    tweet = new Tweet(
                        jv["id"],
                        System.Windows.Browser.HttpUtility.HtmlDecode(jv["text"]),
                        DateTime.Parse(time[0] + ',' + time[2] + ' ' + time[1] + ' ' + time[5] + ' ' + time[3] + time[4]),
                        jv["user"]["id"],
                        jv["user"]["screen_name"],
                        jv["source"],
                        jv["user"]["profile_image_url"],
                        jv["user"]["name"],
                        jv["in_reply_to_status_id"] ?? 0,
                        jv["in_reply_to_screen_name"],
                        0,
                        null
                        );
                }
                tweets.Add(tweet);
            }
            return tweets;
        }

        public static Tweets ParseSearchFromJson(string json, out ulong id)
        {
            Tweets tweets = new Tweets();
            JsonValue root = JsonValue.Parse(json);
            try
            {
                id = root["max_id"];
            }
            catch (Exception)
            {
                id = 0;
            }
            foreach (JsonValue jv in root["results"]) {
                string created_at = (string)jv["created_at"];
                string[] time = created_at.Split(' ');
                Tweet tweet = new Tweet(
                    jv["id"],
                    System.Windows.Browser.HttpUtility.HtmlDecode(jv["text"]),
                    DateTime.Parse(created_at),
                    jv["from_user_id"],
                    jv["from_user"],
                    jv["source"],
                    jv["profile_image_url"],
                    null,//ユーザ名
                    0,   // in_reply_to_status_id
                    null,  // in_reply_to_screen_name
                    0,
                    null
                    );
                tweets.Add(tweet);
            }
            return tweets;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save(string filename)
        {
            var storage = IsolatedStorageFile.GetUserStoreForApplication();
            using (var fs = storage.CreateFile(filename))
            {
                using (var writer = XmlWriter.Create(fs))
                {
                    DataContractSerializer s = new DataContractSerializer(this.GetType());
                    s.WriteObject(writer, this);
                }
            }
        }

        /// <summary>
        /// 読み込み
        /// </summary>
        public void Load(string filename)
        {
            var storage = IsolatedStorageFile.GetUserStoreForApplication();

            try
            {
                using (var fs = storage.OpenFile(filename, System.IO.FileMode.OpenOrCreate))
                {
                    using (var reader = XmlReader.Create(fs))
                    {
                        DataContractSerializer s = new DataContractSerializer(this.GetType());
                        Tweets tweets = (Tweets)s.ReadObject(reader);
                        foreach (var t in tweets)
                        {
                            Add(t);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public Tweet FindId(ulong id)
        {
            return this.FirstOrDefault(x => x.Id == id);
        }

        public void SortByTime()
        {
            this.Sort((x, y) => {
                int diff = y.Time.CompareTo(x.Time);
                if (diff != 0) return diff;
                return (int)(y.Id - x.Id);
            });
        }

    }
}
