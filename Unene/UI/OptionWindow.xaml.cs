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
using System.Globalization;
using System.Windows.Data;
using System.IO.IsolatedStorage;
using System.Windows.Threading;

namespace Unene.UI
{
    public partial class OptionWindow : ChildWindow
    {

        public OptionWindow()
        {
            InitializeComponent();
            OverlayBrush = new SolidColorBrush(Color.FromArgb(0x00, 0, 0, 0));
            Unloaded += new RoutedEventHandler(OptionWindow_Unloaded);
        }

        void OptionWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Settings.Apply();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ChildWindow_Closed(object sender, EventArgs e)
        {
            tabControl1.Items.Clear();
        }
        

        /// <summary>
        /// 「認証/ログイン」ボタン
        /// </summary>
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            AuthWindow dlg = new AuthWindow();
            dlg.Show();
        }


        /// <summary>
        /// 「アップデート確認」ボタン
        /// </summary>
        private void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            CheckUpdateButton.IsEnabled = false;
            Application.Current.CheckAndDownloadUpdateCompleted += 
                new CheckAndDownloadUpdateCompletedEventHandler(Current_CheckAndDownloadUpdateCompleted);
            Application.Current.CheckAndDownloadUpdateAsync();
        }

        void Current_CheckAndDownloadUpdateCompleted(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            Application.Current.CheckAndDownloadUpdateCompleted -= Current_CheckAndDownloadUpdateCompleted;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                if (e.UpdateAvailable)
                {
                    MessageBox.Show("アップデートがありました。再起動後してください。");
                }
                else
                {
                    MessageBox.Show("アップデートはありません");
                }
            }
        }

    }
}

