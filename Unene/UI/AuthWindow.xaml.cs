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
using System.IO.IsolatedStorage;
using Unene;

namespace Unene.UI
{
    public partial class AuthWindow : ChildWindow
    {
        private TwitterOAuth oauth = new TwitterOAuth();

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void ChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            /// リクエストトークンを取得
            oauth.GetRequestToken((success, result) =>
            {
                if (success)
                {   // 取得成功したらログイン用のリンクを設定して、リンクを有効化
                    loginButton.Content = "Twitterにログイン";
                    loginButton.NavigateUri = new Uri((string)result);
                    loginButton.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("ログインのURLを取得できませんでしたorz");
                }
            });
        }

        /// <summary>
        /// ログインリンククリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            // pinとOKボタンを有効化
            pinText.IsEnabled = true;
            OKButton.IsEnabled = true;
        }

        /// <summary>
        /// OKボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            OKButton.IsEnabled = false;
            oauth.GetAccessToken(pinText.Text, (success, result)=>{
                OKButton.IsEnabled = true;
                if (success)
                {
                    oauth.Account.Save();
                    App.Settings.ApplyAccount();
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("うまくいきませんでした。\n" + result);
                }
            });
        }

        /// <summary>
        /// キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

    }
}

