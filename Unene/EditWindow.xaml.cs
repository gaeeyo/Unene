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

namespace Unene
{
    public partial class EditWindow : ChildWindow
    {
        public static string DraftText = "";
        public Brush TestBrush { get; set; }

        public EditWindow()
        {
            InitializeComponent();
            tweetBody.Text = DraftText;
            tweetBody.TextChanged += new TextChangedEventHandler(tweetBody_TextChanged);
            tweetBody_TextChanged(null, null);

            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    CancelButton_Click(null, null);
                }
            };
        }

        /// <summary>
        /// テキストが変更されたとき、ツイート可能かどうか判断して状態を切り替える
        /// </summary>
        void tweetBody_TextChanged(object sender, TextChangedEventArgs e)
        {
            int len = tweetBody.Text.Length;
            bool enable = len > 0;
            if (OKButton.IsEnabled != enable)
            {
                OKButton.IsEnabled = enable;
            }

            int count = 140 - len;
            charCounter.Text = count.ToString();
            if (count < 10)
            {
                charCounter.Style = charCounterError;
            }
            else if (count < 20)
            {
                charCounter.Style = charCounterWarning;
            }
            else
            {
                charCounter.Style = charCounterNormal;
            }
        }

        /// <summary>
        /// ツイートボタン押下時
        /// </summary>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string text = tweetBody.Text;

            Twitter twitter = new Twitter();
            twitter.Account = TwitterAccount.CreateFromApplicationSettings();
            OKButton.IsEnabled = false;
            twitter.StatusesUpdate(text, 0, (success, message) =>
            {
                OKButton.IsEnabled = true;
                if (!success)
                {
                    MessageBox.Show("エラーが発生しました。\n" + message);
                }
                else
                {
                    this.DialogResult = true;
                    DraftText = "";
                }
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            DraftText = tweetBody.Text;
        }
    }
}

