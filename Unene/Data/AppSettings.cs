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
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Unene
{
    [DataContract]
    public enum TimeLineTypes
    {
        [EnumMember]
        Home,
        [EnumMember]
        Search,
        [EnumMember]
        List,
    }

    [DataContract]
    [KnownType(typeof(TimeLineTypes))]
    public class TimeLineSetting
    {
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public TimeLineTypes TimeLineType { get; set; }
        [DataMember]
        public string Text;

        public ulong NextID;

        public string Description
        {
            get {
                switch (TimeLineType)
                {
                    case TimeLineTypes.Home:
                        return "ホーム";
                    case TimeLineTypes.List:
                        return "リスト";
                    case TimeLineTypes.Search:
                        return "'" + Text + "' の検索結果";
                }
                return "?";
            }                    
        }
    }

    [DataContract]
    [KnownType(typeof(TimeLineSetting))]
    public class UneneSettings
    {
        [DataMember] public string  FontName { get; set; }
        [DataMember] public int     IconSize { get; set; }
        [DataMember]
        public int AutoReload { get; set; }
        [DataMember]
        public int BodyFontSize { get; set; }
        [DataMember]
        public bool BodyFontBold { get; set; }
        [DataMember]
        public bool AutoBodyFontSize { get; set; }
        [DataMember]
        public Color BackgroundColor { get; set; }
        [DataMember]
        public Color BodyTextColor { get; set; }
        [DataMember]
        public Color BodyBackgroundColor { get; set; }
        [DataMember]
        public Color NewBodyBackgroundColor { get; set; }
        [DataMember]
        public double MainWindowLeft { get; set; }
        [DataMember]
        public double MainWindowTop { get; set; }
        [DataMember]
        public double MainWindowWidth { get; set; }
        [DataMember]
        public double MainWindowHeight { get; set; }
        [DataMember]
        public WindowState MainWindowState { get; set; }
        [DataMember]
        public int Columns { get; set; }
        [DataMember]
        public string NGWords { get; set; }
        [DataMember]
        public List<TimeLineSetting> TimeLines { get; set; }
        [DataMember]
        public bool AutoIconHeight { get; set; }
        [DataMember]
        public bool UseWallPaper { get; set; }
        [DataMember]
        public double BodyBackgroundOpacity { get; set; }
        [DataMember]
        public bool UseClock { get; set; }
        [DataMember]
        public int TweetViewStyle { get; set; }


        [OnDeserializingAttribute()]
        public void RunThisMethod(StreamingContext context)
        {
            SetDefaults();
        }

        public event Action<object> VisualChanged;
        public event Action<object> Changed;
        public event Action<object> AccountChanged;

        public UneneSettings()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Columns = 1;
            IconSize = 48;
            FontName = "Arial";
            AutoReload = 60;
            BodyFontSize = 20;
            BodyFontBold = true;
            BackgroundColor = Color.FromArgb(0xff, 30, 30, 30);
            BodyTextColor = Color.FromArgb(0xff, 50, 50, 50);
            NGWords = "";
            AutoIconHeight = false;
            BodyBackgroundColor = Color.FromArgb(0xff, 236, 236, 236);
            NewBodyBackgroundColor = Color.FromArgb(0xff, 231, 215, 170);
            UseClock = false;
            BodyBackgroundOpacity = -1.0;
            TweetViewStyle = 0;

            TimeLines = new List<TimeLineSetting>();
            TimeLines.Add(new TimeLineSetting());
            TimeLines[0].TimeLineType = TimeLineTypes.Home;
            TimeLines[0].IsEnabled = true;
        }

        public void ApplyVisual()
        {
            if (VisualChanged != null)
            {
                VisualChanged(null);
            }
        }

        public void ApplyAccount()
        {
            if (AccountChanged != null)
            {
                AccountChanged(null);
            }
        }

        public void Apply()
        {
            if (Changed != null)
            {
                Changed(null);
            }
        }

        private static Stream OpenSettingFile(bool isSave)
        {
            string path = "Settings.xml";
            var isf = IsolatedStorageFile.GetUserStoreForApplication();
            Stream stream = null;
            if (isSave)
            {
                stream = new IsolatedStorageFileStream(path,
                               FileMode.Create, FileAccess.Write, isf);
            }
            else {
                if (isf.FileExists(path)) {
                    stream = new IsolatedStorageFileStream(path,
                        FileMode.Open, FileAccess.Read, isf);
                }
            }
            return stream;
        }

        public void Save()
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                var dcs = new DataContractSerializer(typeof(UneneSettings));
                dcs.WriteObject(ms, this);  // 試しに書き込む

                var stream = OpenSettingFile(true);
                using (stream)
                {
                    dcs.WriteObject(stream, this);
                    stream.Dispose();
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("設定の保存中にエラーが発生しました。\n"+  e.Message);
            }
        }

        public static UneneSettings Load()
        {
            UneneSettings settings = new UneneSettings();
            try
            {
                var stream = OpenSettingFile(false);
                if (stream != null)
                {
                        
                    using (stream)
                    {
                        var dcs = new DataContractSerializer(typeof(UneneSettings));
                        XmlReader xd = XmlDictionaryReader.Create(stream);
                        settings = (UneneSettings)dcs.ReadObject(xd, true);
                        stream.Dispose();
                        stream.Close();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("設定の読み込み中にエラーが発生しました。\n" + e.Message);
            }
            return settings;
        }

        public void GetWindowPosition(Window window)
        {
            if (MainWindowWidth != 0 && MainWindowHeight != 0)
            {
                try
                {
                    window.Height = MainWindowHeight;
                    window.Width = MainWindowWidth;
                    window.Left = MainWindowLeft;
                    window.Top = MainWindowTop;
                    //window.WindowState = MainWindowState;
                }
                catch (Exception )
                {
                }
            }
        }
        public void SetWindowPosition(Window window)
        {
            MainWindowState = window.WindowState;
            if (window.WindowState != WindowState.Normal)
            {
                window.WindowState = WindowState.Normal;
            }
            MainWindowLeft = window.Left;
            MainWindowTop = window.Top;
            MainWindowWidth = window.Width;
            MainWindowHeight = window.Height;
        }
    }

}
