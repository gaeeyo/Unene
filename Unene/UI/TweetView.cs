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
using System.Text.RegularExpressions;
using Gaeeyo;

namespace Unene.UI
{
    public class TweetViewResource
    {
        public static byte BodyBackgroundOpacity = 0xff;
        public static SolidColorBrush BodyBackground = new SolidColorBrush(Colors.LightGray);
        public static SolidColorBrush NewBodyBackground = new SolidColorBrush(Colors.White);
        public static SolidColorBrush BodyTextBrush = new SolidColorBrush(Colors.Black);
        public static SolidColorBrush UserNameBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x60, 0x91, 0x51));
        public static SolidColorBrush HashLinkBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xB0, 0x6F, 0xA4));
        public static SolidColorBrush LinkBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x3C, 0x81, 0xCC));
        public static SolidColorBrush RTBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xbb, 0x00, 0x00));
        public static SolidColorBrush RetweetBrush = new SolidColorBrush(Colors.Gray);

        public static double FontHeightHint;
    }

    public class TweetView : Control
    {
        public TweetView()
        {
            this.DefaultStyleKey = typeof(TweetView);
            this.Style = (Style)Application.Current.Resources["TweetView_0"];
            this.CacheMode = new BitmapCache();

            BodyBackgroundDark = Colors.Green;
            ApplySettings();
        }

        // property
        public static DependencyProperty IconSizeProperty = DependencyProperty.Register(
            "IconSize", typeof(double), typeof(TweetView), null);

        public static DependencyProperty BodyBackgroundColorProperty = DependencyProperty.Register(
            "BodyBackgroundColor", typeof(Color), typeof(TweetView), null);
        public Color BodyBackgroundColor
        {
            get { return (Color)GetValue(BodyBackgroundColorProperty); }
            set { SetValue(BodyBackgroundColorProperty, value); }
        }

        public static DependencyProperty BodyBackgroundDarkProperty = DependencyProperty.Register(
                "BodyBackgroundDark", typeof(Color), typeof(TweetView), null);
        public Color BodyBackgroundDark
        {
            get { 
                return (Color)GetValue(BodyBackgroundDarkProperty); 
            }
            set { SetValue(BodyBackgroundDarkProperty, value); }
        }

        // private
        Tweet _tweet;
        bool _autoFontSize = false;
        int _fontSizing = 0;

        Button profileImage { get; set; }
        TweetTextBox textBody { get; set; }
        HyperlinkButton nameText { get; set; }
        HyperlinkButton timeText { get; set; }
        int textBodyFontSize = 12;

        // public
        public Tweet Tweet
        {
            get { return _tweet; }
            set {
                _tweet = value;

                updateStates();
            }
        }
        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            profileImage = GetTemplateChild("profileImage") as Button;
            textBody = GetTemplateChild("textBody") as TweetTextBox;
            nameText = GetTemplateChild("nameText") as HyperlinkButton;
            timeText = GetTemplateChild("timeText") as HyperlinkButton;
            //ApplySettings();
            updateStates();
        }

        void updateStates()
        {
            // 画像を設定
            if (profileImage != null)
            {
                string imageUrl = IconSize > 48 ? _tweet.BigImage : _tweet.Image;
                //profileImage.Width = _iconSize;
                //profileImage.Height = _iconSize;
                profileImage.Background = null;
                ImageManager.GetImage(imageUrl, (image) =>
                {
                    profileImage.Background = new ImageBrush()
                    {
                        ImageSource = image,
                        Stretch = Stretch.UniformToFill,
                    };
                });
            }
            // 
            if (textBody != null)
            {
                textBody.FontWeight = App.Settings.BodyFontBold ? FontWeights.Bold : FontWeights.Normal;
                textBody.FontFamily = new FontFamily(App.Settings.FontName);
                parseText(_tweet.Text);
                resetTextBodyFontSize();
            }
            if (nameText != null)
            {
                nameText.Content = _tweet.ScreenName;
            }
            if (timeText != null)
            {
                timeText.Content = _tweet.Time.ToString();
            }
            UpdateTime();
        }

        public void ApplySettings()
        {
            Style newStyle = (Style)Application.Current.Resources["TweetView_" + App.Settings.TweetViewStyle];
            if (!newStyle.Equals(this.Style))
            {
                this.Style = newStyle;
                profileImage = null;
                textBody = null;
                nameText = null;
                timeText = null;
            }
            IconSize = App.Settings.IconSize;
            _autoFontSize = App.Settings.AutoBodyFontSize;

            textBodyFontSize = App.Settings.BodyFontSize;
            Foreground = new SolidColorBrush(App.Settings.BodyTextColor);
            resetTextBodyFontSize();
         }

        public void UpdateTime()
        {
            if (_tweet != null)
            {
                //RichTextBox bg = textBody;

                //SolidColorBrush brush = textBody.Background as SolidColorBrush;
                if (Background == null)
                {
                    Background = new SolidColorBrush();
                }
                SolidColorBrush brush = Background as SolidColorBrush;

                double opacity = 0;
                switch (_tweet.Age)
                {
                    case 0: opacity = 0; break;
                    case 1: opacity = 0.40; break;
                    case 2: opacity = 0.60; break;
                    case 3: opacity = 0.80; break;
                    default:
                        opacity = 1;
                        break;
                }

                Color c;
                if (opacity < 0)
                {
                    c = Color.FromArgb(TweetViewResource.BodyBackgroundOpacity,
                        TweetViewResource.BodyBackground.Color.R,
                        TweetViewResource.BodyBackground.Color.G,
                        TweetViewResource.BodyBackground.Color.B);
                }
                else
                {
                    c = Utils.BlendColor(
                        TweetViewResource.NewBodyBackground.Color,
                        TweetViewResource.BodyBackground.Color, 
                        opacity);
                    c.A = TweetViewResource.BodyBackgroundOpacity;
                }
                brush.Color = c;
                BodyBackgroundColor = c;
                
            }
        }


        void resetTextBodyFontSize()
        {
            if (textBody != null)
            {
                if (_autoFontSize)
                {
                    textBody.MaxHeight = double.MaxValue;
                    textBody.FontSize = TweetViewResource.FontHeightHint;
                    _fontSizing = 0;
                    //textBodyContainer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                }
                else
                {
                    textBody.MaxHeight = double.MaxValue;
                    textBody.BaseFontSize = textBodyFontSize;
                }
            }
        }
        

        private void parseText(string text)
        {
            Paragraph paragraph = new Paragraph();

            Regex re = new Regex("http:\\/\\/[a-zA-Z0-9.-]+\\/[a-zA-Z0-9\\-/.$,;:&=?!*~@#%_()]*|[RQ]T |#[a-zA-Z_]\\S*( |$)|@[a-zA-Z0-9_]+");

            int start = 0;
            Brush currentColor = TweetViewResource.BodyTextBrush;

            while (true)
            {
                Match m = re.Match(text, start);
                if (!m.Success)
                {
                    paragraph.Inlines.Add(text.Substring(start));
                    paragraph.Inlines.Last().Foreground = currentColor;
                    break;
                }
                else
                {
                    paragraph.Inlines.Add(text.Substring(start, m.Index - start));
                    paragraph.Inlines.Last().Foreground = currentColor;

                    if (m.Value == "RT " || m.Value == "QT ")
                    {
                        Run run = new Run()
                        {
                            Text = m.Value,
                            Foreground = TweetViewResource.RTBrush
                        };
                        paragraph.Inlines.Add(run);
                    }
                    else
                    {
                        Hyperlink hl = new Hyperlink();
                        hl.Inlines.Add(m.Value);
                        if (string.Compare(m.Value, 0, "@", 0, 1) == 0)
                        {
                            hl.NavigateUri = new Uri(Twitter.GetUserUrl(m.Value.Substring(1)));
                            hl.Foreground = TweetViewResource.UserNameBrush;
                            hl.TextDecorations = null;
                        }
                        else if (string.Compare(m.Value, 0, "#", 0, 1) == 0)
                        {
                            hl.NavigateUri = new Uri("http://twitter.com/#search?q=" +
                                System.Windows.Browser.HttpUtility.UrlEncode(m.Value.Trim()));
                            hl.Foreground = TweetViewResource.HashLinkBrush;
                            hl.TextDecorations = null;
                        }
                        else
                        {
                            hl.NavigateUri = new Uri(m.Value);
                            hl.Foreground = TweetViewResource.LinkBrush;
                        }
                        paragraph.Inlines.Add(hl);
                    }
                    start = m.Index + m.Length;
                }
            }
            textBody.Blocks.Clear();
            textBody.Blocks.Add(paragraph);
            textBody.StartFontSizing();
        }

    }
}
