using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using Gaeeyo;
using System.Diagnostics;

namespace Unene.UI
{
    public partial class TweetItemControl : UserControl
    {
        public int CornerRadius { get; set; }
        private static DateTime _baseTime = DateTime.Now;

        public static SolidColorBrush BodyTextBrush = new SolidColorBrush(Colors.Black);
        public static SolidColorBrush UserNameBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x60, 0x91, 0x51));
        public static SolidColorBrush HashLinkBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xB0, 0x6F, 0xA4));
        public static SolidColorBrush LinkBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x3C, 0x81, 0xCC));
        public static SolidColorBrush RTBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xbb, 0x00, 0x00));
        public static SolidColorBrush RetweetBrush = new SolidColorBrush(Colors.Gray);
        public static SolidColorBrush NewBodyBackground = new SolidColorBrush(Colors.White);
        public static SolidColorBrush BodyBackground = new SolidColorBrush(Colors.LightGray);
        public static byte BodyBackgroundOpacity = 0xff;

        public static double FontHeightHint { get; set; }

        private int textBodyFontSize = 12;
        private bool _autoFontSize = false;
        private bool _autoIconHeight = false;
        private double _iconSize = 48;
        private Tweet tweet;
        private double _width;
        private int _fontSizing = 0;
        ToolTip _toolTip;

        public static DateTime BaseTime
        {
            get
            {
                return _baseTime;
            }
            set
            {
                _baseTime = value;
            }
        }

        public TweetItemControl()
        {
            InitializeComponent();
            ApplySettings();

            _toolTip = new ToolTip();
            _toolTip.Opened += (s, e) =>
            {
                _toolTip.Content = ToolTipText;
            };
            ToolTipService.SetToolTip(profileImage, _toolTip);

            profileImage.Click += delegate(object sender, RoutedEventArgs e)
            {
                if (tweet != null) {
                    //Gaeeyo.Utils.Navigate(new Uri("http://twitter.com/" + tweet.ScreenName + "/statuses/" + tweet.Id));
                    Gaeeyo.Utils.Navigate(new Uri(tweet.StatusUrl));
                }
            };
            textBody.SizeChanged += new SizeChangedEventHandler(textBody_SizeChanged);
            textBody.LayoutUpdated += new EventHandler(textBody_LayoutUpdated);
        }

        /*
        public override void OnApplyTemplate()
        {
            textBody.Background = BodyBackground;
        }*/

        void textBody_LayoutUpdated(object sender, EventArgs e)
        {
            if (_autoFontSize && textBody.FontSize > 6)
            {
                if (textBody.ActualHeight <= textBody.DesiredSize.Height)
                {
                    //textBody.FontSize = Math.Floor(textBody.FontSize * 0.9);
                    _fontSizing++;
                    if (_fontSizing == 1)
                    {
                        textBody.FontSize = FontHeightHint * 0.75;
                    }
                    else
                    {
                        textBody.FontSize = Math.Floor((FontHeightHint - 1) / _fontSizing); //textBody.FontSize * 0.9);
                    }
                    //Debug.WriteLine("FontSize: {0}", textBody.FontSize);
                }
            }
            else if (_autoIconHeight)
            {
                profileImage.Height = textBody.ActualHeight;
            }
        }


        void resetTextBodyFontSize()
        {
            if (_autoFontSize)
            {
                textBody.MaxHeight = double.MaxValue;
                textBody.FontSize = FontHeightHint;
                _fontSizing = 0;
                //textBodyContainer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            else
            {
                textBody.MaxHeight = double.MaxValue;
                textBody.FontSize = textBodyFontSize;
            }
        }

        void textBody_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != _width)
            {
                _width = e.NewSize.Width;
                resetTextBodyFontSize();
            }
        }

        ~TweetItemControl()
        {
            //System.Diagnostics.Debug.WriteLine("~TweetItemControl");
        }

        public void ApplySettings()
        {
            _iconSize = App.Settings.IconSize;
            _autoFontSize = App.Settings.AutoBodyFontSize;
            _autoIconHeight = App.Settings.AutoIconHeight;

            if (_autoIconHeight)
            {
                profileImage.Height = double.NaN;
            }
            else
            {
                profileImage.Height = _iconSize;
            }
            profileImage.MaxHeight = _iconSize;

            profileImage.Width = _iconSize;
            LayoutRoot.ColumnDefinitions[0].Width = new GridLength(_iconSize);
            textBodyFontSize = App.Settings.BodyFontSize;
            textBody.FontWeight = App.Settings.BodyFontBold ? FontWeights.Bold : FontWeights.Normal;
            textBody.FontFamily = new FontFamily(App.Settings.FontName);

            if (!_autoFontSize && App.Settings.BodyBackgroundOpacity >= 0)
            {
                textBody.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            }
            else
            {
                textBody.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            }
            //SubTextFontSize = App.Settings.SubTextFontSize;
            UpdateTime();
            resetTextBodyFontSize();
        }

        public string ToolTipText
        {
            get
            {
                StringBuilder tip = new StringBuilder(tweet.ScreenName);
                if (tweet.Name != tweet.ScreenName)
                {
                    tip.AppendFormat("({0})", tweet.Name);
                }
                tip.Append(" " + tweet.Time.ToString());

                TimeSpan min = (DateTime.Now - tweet.Time);
                if (min.TotalMinutes < 0)
                {
                }
                else if (min.TotalMinutes < 60)
                {
                    tip.AppendFormat("({0:F0}分前)", min.TotalMinutes);
                }
                else if (min.TotalHours < 24)
                {
                    tip.AppendFormat("({0:F0}時間前)", Math.Round(min.TotalHours));
                }
                else
                {
                    tip.AppendFormat("({0:F0}日前)", Math.Round(min.TotalDays));
                }
                return tip.ToString();
            }
        }

        public Tweet Tweet {
            get {
                return this.tweet;
            }
            set {
                this.tweet = value;

                if (value != null)
                {
                    resetTextBodyFontSize();
                    textBody.Selection.Select(textBody.ContentStart, textBody.ContentStart);
                    parseText(tweet.Text);

                    if (tweet.RetweetId != 0 && tweet.RetweetUserScreenName != null)
                    {
                        Hyperlink hl = new Hyperlink();
                        hl.Inlines.Add(tweet.RetweetUserScreenName);
                        hl.NavigateUri = new Uri(Twitter.GetUserUrl(tweet.RetweetUserScreenName));

                        Paragraph p = new Paragraph();
                        p.Foreground = RetweetBrush;
                        p.Inlines.Add("(");
                        p.Inlines.Add(hl);
                        p.Inlines.Add("がリツイート)");
                        textBody.Blocks.Add(p);
                    }

                    string imageUrl = _iconSize > 48 ? tweet.BigImage : tweet.Image;
                    profileImage.Background = null;
                    ImageManager.GetImage(imageUrl, (image) => { 
                        profileImage.Background = new ImageBrush() { 
                            ImageSource = image,
                            Stretch = Stretch.UniformToFill,
                        };
                    });
                    UpdateTime();
                }
            }
        }

        public void UpdateTime()
        {
            if (tweet != null)
            {
                RichTextBox bg = textBody;

                SolidColorBrush brush = textBody.Background as SolidColorBrush;

                double opacity = 0;
                switch (tweet.Age)
                {
                    case 0: opacity = 0; break;
                    case 1: opacity = 0.40; break;
                    case 2: opacity = 0.60; break;
                    case 3: opacity = 0.80; break;
                    default:
                        opacity = 1;
                        break;
                }

                if (opacity < 0) {
                    brush.Color = Color.FromArgb(BodyBackgroundOpacity, 
                        BodyBackground.Color.R, BodyBackground.Color.G, BodyBackground.Color.B);
                }
                else {
                    brush.Color = blendColor(NewBodyBackground.Color, BodyBackground.Color, opacity);
                }
            }
        }

        Color blendColor(Color c1, Color c2, double p2)
        {
            c1.A = BodyBackgroundOpacity;
            c2.A = BodyBackgroundOpacity;
            if (p2 <= 0) return c1;
            if (p2 >= 1) return c2;

            double p1 = 1 - p2;
            return Color.FromArgb(BodyBackgroundOpacity,
                    Convert.ToByte((c1.R * p1 + c2.R * p2)),
                    Convert.ToByte((c1.G * p1 + c2.G * p2)),
                    Convert.ToByte((c1.B * p1 + c2.B * p2)));
        }

        private void parseText(string text)
        {
            Paragraph paragraph = new Paragraph();

            Regex re = new Regex("http:\\/\\/[a-zA-Z0-9.-]+\\/[a-zA-Z0-9\\-/.$,;:&=?!*~@#_()]*|[RQ]T |#[a-zA-Z_]\\S*( |$)|@[a-zA-Z0-9_]+");

            int start = 0;
            Brush currentColor = BodyTextBrush;

            while (true) {
                Match m = re.Match(text, start);
                if (!m.Success) {
                    paragraph.Inlines.Add(text.Substring(start));
                    paragraph.Inlines.Last().Foreground = currentColor;
                    break;
                }
                else {
                    paragraph.Inlines.Add(text.Substring(start, m.Index - start));
                    paragraph.Inlines.Last().Foreground = currentColor;

                    if (m.Value == "RT " || m.Value == "QT ")
                    {
                        Run run = new Run()
                        {
                            Text = m.Value,
                            Foreground = RTBrush
                        };
                        paragraph.Inlines.Add(run);
                    }
                    else {
                        Hyperlink hl = new Hyperlink();
                        hl.Inlines.Add(m.Value);
                        if (string.Compare(m.Value, 0, "@", 0, 1) == 0) {
                            hl.NavigateUri = new Uri(Twitter.GetUserUrl(m.Value.Substring(1)));
                            hl.Foreground = UserNameBrush;
                            hl.TextDecorations = null;
                        }
                        else if (string.Compare(m.Value, 0, "#", 0, 1) == 0)
                        {
                            hl.NavigateUri = new Uri("http://twitter.com/#search?q=" + 
                                System.Windows.Browser.HttpUtility.UrlEncode(m.Value.Trim()) );
                            hl.Foreground = HashLinkBrush;
                            hl.TextDecorations = null;
                        }   
                        else
                        {
                            hl.NavigateUri = new Uri(m.Value);
                            hl.Foreground = LinkBrush;
                        }
                        paragraph.Inlines.Add(hl);
                    }
                    start = m.Index + m.Length;
                }
            }
            textBody.Blocks.Clear();
            textBody.Blocks.Add(paragraph);
        }
    }
}
