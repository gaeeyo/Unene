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
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Json;
using System.Windows.Threading;
using Unene.UI;
using System.Windows.Media.Imaging;

namespace Unene
{
    public partial class MainPage : UserControl
    {
        private const int       tweetsCountMax = 500;               // 保存する最大のツイート数
        private DispatcherTimer statusTextHideTimer;                // ステータステキストを隠すタイマ
        private DispatcherTimer autoReloadTimer = new DispatcherTimer();    // 自動リロードタイマ
        private Tweets          tweets = new Tweets();              // 現在のツイート
        private TwitterOAuth    twitter = new TwitterOAuth();       // Twitterクライアント
        private int             reloadIndex = 0;
        private DispatcherTimer uiHideTimer = new DispatcherTimer();
        private Random          _rand = new Random();
        List<MediaElement>      newItemSounds = new List<MediaElement>();

        public MainPage()
        {
            InitializeComponent();

            // OutOfBrowserで動作しているかどうか確認
            if (!Application.Current.IsRunningOutOfBrowser)
            {
                showOutOfBrowserError();
                return;
            }

            // アップデートの確認
            Application.Current.CheckAndDownloadUpdateCompleted +=
                new CheckAndDownloadUpdateCompletedEventHandler(Current_CheckAndDownloadUpdateCompleted);
            Application.Current.CheckAndDownloadUpdateAsync();

            Application.Current.Exit += new EventHandler(Current_Exit);

            statusTextHideTimer = new DispatcherTimer();
            statusTextHideTimer.Interval = TimeSpan.FromSeconds(4);
            statusTextHideTimer.Tick += delegate(object sender, EventArgs e)
            {
                statusTextHideTimer.Stop();
                statusTextOut.Begin();
            };

            ApplyVisualSettings();
            tweetsPanel.ApplySettings();

            // アカウントの情報をロード
            twitter.Account = TwitterAccount.CreateFromApplicationSettings();

            // サウンドの読み込み
            for (int j=0; j<5; j++) {
                MediaElement me = new MediaElement();
                me.AutoPlay = false;
                me.Source = new Uri(string.Format("/Sounds/piyo{0}.mp3", j), UriKind.Relative);
                newItemSounds.Add(me);
                LayoutRoot.Children.Add(me);
            }
            
            // Tweetsパネルの設定
            tweetsPanel.Tweets = tweets;
            tweetsPanelContainer.SizeChanged += (sender, e) =>
            {
                tweetsPanel.DelayLayout();
            };

            var mainWindow = Application.Current.MainWindow;

            // ドラッグによる移動とリサイズ
            var cursorIndex = 0;
            var cursors = new Cursor[] {
                Cursors.Arrow, Cursors.SizeWE, Cursors.SizeWE,
                Cursors.SizeNS, Cursors.SizeNWSE, Cursors.SizeNESW,
                Cursors.SizeNS, Cursors.SizeNESW, Cursors.SizeNWSE };
            
            LayoutRoot.MouseMove += (s, e) =>
            {
                Cursor cursor = null;
                if (mainWindow.WindowState == WindowState.Normal)
                {
                    Point pt = e.GetPosition(LayoutRoot);
                    int index = 0;
                    if (pt.X < 6) index = 1;
                    else if (pt.X >= LayoutRoot.ActualWidth - 6) index = 2;

                    if (pt.Y < 6) index += 3;
                    else if (pt.Y >= LayoutRoot.ActualHeight - 6) index += 6;

                    cursorIndex = index;
                    cursor = cursors[index];
                }
                else
                {
                    cursorIndex = 0;
                }
                if (LayoutRoot.Cursor != cursor) LayoutRoot.Cursor = cursor;
            };
            
            LayoutRoot.MouseLeftButtonDown += (sender, e) =>
            {
                if (cursorIndex == 0) {
                    if (!(e.OriginalSource.GetType().FullName == "MS.Internal.RichTextBoxView"))
                    {
                        mainWindow.DragMove();
                        e.Handled = true;
                    }
                }
                else {
                    WindowResizeEdge [] edges = new WindowResizeEdge [] {
                        WindowResizeEdge.Left,  WindowResizeEdge.Left,      WindowResizeEdge.Right,
                        WindowResizeEdge.Top,   WindowResizeEdge.TopLeft,   WindowResizeEdge.TopRight,
                        WindowResizeEdge.Bottom,WindowResizeEdge.BottomLeft, WindowResizeEdge.BottomRight
                    };
                    mainWindow.DragResize(edges[cursorIndex]);
                }
            };
            
            // 最大化
            maximizeButton.Click += (sender, e) =>
            {
                switch (mainWindow.WindowState)
                {
                    case WindowState.Normal:
                        mainWindow.WindowState = WindowState.Maximized;
                        break;
                    case WindowState.Maximized:
                        mainWindow.WindowState = WindowState.Normal;
                        break;
                }
            };

            // 最小化
            minimizeButtonn.Click += (sender, e) =>
            {
                mainWindow.WindowState = WindowState.Minimized;
            };

            // 閉じる
            closeButton.Click += (sender, e) =>
            {
                mainWindow.Close();
            };

            /*
            // ツールバーパネルを自動的に隠す
            toolbarPanelOut.Begin();

            toolbarPanel.MouseLeave += (sender, e) =>
            {
                toolbarPanelIn.Stop();
                toolbarPanelOut.Begin();
            };

            toolbarPanel.MouseEnter += (sender, e) =>
            {
                toolbarPanelOut.Stop();
                toolbarPanelIn.Begin();
            };
             * */

            // カラム数を読み込み
            tweetsPanel.Columns = Math.Max(1, App.Settings.Columns);

            // ツイートをログから読み込み
            tweets.Load("home.xml");

            // 自動的リロードタイマーを設定
            autoReloadTimer.Interval = TimeSpan.FromSeconds(App.Settings.AutoReload);
            autoReloadTimer.Tick += delegate(object sender, EventArgs e)
            {
                reloadTimeLine();
            };
            autoReloadTimer.Start();



            // ビジュアル設定が変更されたときに表示中のツイートコントロールをすべて更新して
            // レイアウトしなおす
            App.Settings.VisualChanged += delegate(object sender2)
            {
                ApplyVisualSettings();
                tweetsPanel.ApplySettings();
            };

            // アカウント設定が変更されたときリロードする
            App.Settings.AccountChanged += delegate(object sender)
            {
                twitter.Account = TwitterAccount.CreateFromApplicationSettings();
                autoReloadTimer.Stop();
                autoReloadTimer.Start();
                reloadTimeLine(true);
            };

            // 設定が変更されたときリロードする
            App.Settings.Changed += delegate(object sender)
            {
                autoReloadTimer.Interval = TimeSpan.FromSeconds(App.Settings.AutoReload);
            };

            uiHideTimer.Tick += (s, e) =>
            {
                showToolbar(false);
                uiHideTimer.Stop();
            };
            uiHideTimer.Interval = TimeSpan.FromSeconds(3);
            uiHideTimer.Start();

            this.MouseMove += (s, e) =>
            {
                if (toolbarPanel.Opacity < 1)
                {
                    //toolbarPanelOut.Stop();
                    //toolbarPanelIn.Begin();
                    showToolbar(true);
                }
                uiHideTimer.Start();
            };
        }

        void showToolbar(bool b)
        {
            if (b)
            {
                toolbarPanelIn.Begin();
            }
            else
            {
                toolbarPanelIn.Stop();
                toolbarPanelOut.Begin();
            }
            /*
            System.Windows.Visibility hide = System.Windows.Visibility.Collapsed;
            System.Windows.Visibility show = System.Windows.Visibility.Visible;
            clock.Visibility = b ? hide : show;
            toolbarPanel.Visibility = b ? show : hide;
             * */
        }

        /// <summary>
        /// ビジュアル関連の設定を適用
        /// </summary>
        void ApplyVisualSettings()
        {
            if (App.Settings.UseClock)
            {
                clock.ClockFontFamily = new FontFamily("Arial Black"); //App.Settings.FontName);
                clock.ClockFontSize = App.Settings.BodyFontSize;
                clock.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                clock.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (App.Settings.UseWallPaper && App.Settings.BodyBackgroundOpacity>=0)
            {
                try
                {
                    BitmapImage bmp = new BitmapImage();
                    var src = IsolatedStorageFile.GetUserStoreForApplication().OpenFile("wallpaper", System.IO.FileMode.Open);
                    using (src)
                    {
                        bmp.SetSource(src);
                    }
                    wallpaper.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    wallpaper.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    wallpaper.Source = bmp;
                    //wallpaper.Source = new BitmapImage(new Uri("http://twitter.com/images/themes/theme1/bg.png"));
                }
                catch (Exception )
                {
                    MessageBox.Show("壁紙の読み込みエラー");
                }
                wallpaper.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                wallpaper.Visibility = System.Windows.Visibility.Collapsed;
            }
            LayoutRoot.Background = new SolidColorBrush(App.Settings.BackgroundColor);
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        void Current_Exit(object sender, EventArgs e)
        {
            App.Settings.SetWindowPosition(Application.Current.MainWindow);
            App.Settings.Save();
            SaveTweets();
        }


        /// <summary>
        /// OutOfBrowserで実行されていないときのエラー画面を表示する
        /// </summary>
        private void showOutOfBrowserError()
        {
            toolbarPanel.Visibility = System.Windows.Visibility.Collapsed;

            var label = new Label();
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            label.Margin = new Thickness(0, -50, 0, 0);
            label.Content = "このアプリケーションは Out-of-Browser(ブラウザの外で動作するやつ) 専用です。\n下のインストールボタンをクリックしてインストールすると実行できます。";
            label.FontSize = 16;
            LayoutRoot.Children.Add(label);

            var btn = new Button();
            btn.Margin = new Thickness(0, 40, 0, 0);
            btn.Width = 150;
            btn.Height = 40;
            btn.Content = "  インストール  ";
            btn.FontSize = 20;
            btn.Click += (sender, e) =>
            {
                if (Application.Current.InstallState == InstallState.NotInstalled)
                {
                    Application.Current.Install();
                }
                else
                {
                    MessageBox.Show("すでにインストールされていますね...");
                }
            };
            LayoutRoot.Children.Add(btn);
        }

        /// <summary>
        /// アップデート完了時のメッセージを表示
        /// </summary>
        void Current_CheckAndDownloadUpdateCompleted(object sender, 
            CheckAndDownloadUpdateCompletedEventArgs e)
        {
            Application.Current.CheckAndDownloadUpdateCompleted -= 
                Current_CheckAndDownloadUpdateCompleted;
            if (e.UpdateAvailable)
            {
                MessageBox.Show("アップデートしました。アプリケーションを再起動してください。");
            }
        }

        /// <summary>
        /// フルスクリーン
        /// </summary>
        private void fullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Host.Content.IsFullScreen = 
                !Application.Current.Host.Content.IsFullScreen;
        }

        /// <summary>
        /// カラムを増やす
        /// </summary>
        private void columnIncButton_Click(object sender, RoutedEventArgs e)
        {
            if (tweetsPanel.Columns < 10)
            {
                tweetsPanel.Columns++;
                App.Settings.Columns = tweetsPanel.Columns;
                tweetsPanel.LayoutTweets(TweetsPanelLayoutMode.Relayout);
            }
        }

        /// <summary>
        /// カラムを減らす
        /// </summary>
        private void columnDecButton_Click(object sender, RoutedEventArgs e)
        {
            if (tweetsPanel.Columns > 1)
            {
                tweetsPanel.Columns--;
                App.Settings.Columns = tweetsPanel.Columns;
                tweetsPanel.LayoutTweets(TweetsPanelLayoutMode.Relayout);
            }
        }

        /// <summary>
        /// オプションを表示
        /// </summary>
        private void optionsButton_Click(object sender, RoutedEventArgs e)
        {
            //OptionDialog dlg = new OptionDialog();
            //AuthWindow dlg = new AuthWindow();
            OptionWindow dlg = new OptionWindow();
            dlg.Show();
        }

        /// <summary>
        /// ツイートを保存
        /// </summary>
        void SaveTweets()
        {
            this.tweets.Save("home.xml");
        }
        
        /// <summary>
        /// タイムラインをリロードする
        /// </summary>
        void reloadTimeLine(bool startup = false)
        {
            SaveTweets();
            autoReloadTimer.Stop();

            if (!TwitterAccount.IsValid(twitter.Account))
            {
                SetStatus("i", Colors.Yellow, "アカウントが設定されていません");
                return;
            }

            var timeLines = App.Settings.TimeLines;
            TimeLineSetting tl = null;

            for (var j = 0; j < timeLines.Count; j++)
            {
                if (reloadIndex >= timeLines.Count)
                {
                    reloadIndex = 0;
                }
                if (timeLines[reloadIndex].IsEnabled)
                {
                    tl = timeLines[reloadIndex];
                    break;
                }
                reloadIndex++;
            }

            if (tl != null)
            {
                reloadButton.IsEnabled = false;
                reloadTimeLineMain(tl);
            }
            else
            {
                SetStatus("i", Colors.Yellow, "取得対象のTLが設定されていません。オプションの「タイムライン」タブを確認してください");
            }
            reloadIndex++;
        }

        void reloadTimeLineMain(TimeLineSetting tl)
        {
            SetStatus("q", Colors.Cyan, string.Format("{0}を取得中...", tl.Description));

            try
            {
                switch (tl.TimeLineType)
                {
                    case TimeLineTypes.Home:
                        ReloadTimeLine_REST(tl);
                        break;
                    case TimeLineTypes.Search:
                        ReloadTimeLine_Search(tl);
                        break;
                    case TimeLineTypes.List:
                        ReloadTimeLine_REST(tl);
                        break;
                }
            }
            catch (Exception err)
            {
                ReloadTimeLineCompleted(false, err.Message);
            }
        }


        /// <summary>
        /// ホームを読み込む
        /// </summary> 
        void ReloadTimeLine_REST(TimeLineSetting tl)
        {
            string query = "";
            switch (tl.TimeLineType)
            {
                case TimeLineTypes.Home:
                    query = TwitterOAuth.MakeHomeQuery(0);
                    break;
                case TimeLineTypes.List:
                    query = TwitterOAuth.MakeListQuery(tl.Text, 0);
                    break;
            }
            twitter.GetTimeline(query, (success, body) =>
            {
                string errorMessage = null;
                if (success)
                {
                    try
                    {
                        Tweets newTweets = Tweets.ParseTimelineFromJson(body);
                        AddTweets(newTweets);
                    }
                    catch (Exception e)
                    {
                        success = false;
                        errorMessage = e.Message;
                    }
                }
                else
                {
                    errorMessage = body;
                }
                ReloadTimeLineCompleted(success, errorMessage);
            });
        }

        void ReloadTimeLine_Search(TimeLineSetting tl)
        {
            twitter.GetTimeline(TwitterOAuth.MakeSearchQuery(tl.Text, tl.NextID), (success, body) =>
            {
                string errorMessage = null;
                if (success)
                {
                    try
                    {
                        ulong id;
                        Tweets newTweets = Tweets.ParseSearchFromJson(body, out id);
                        AddTweets(newTweets);
                        tl.NextID = id;
                    }
                    catch (Exception e)
                    {
                        success = false;
                        errorMessage = e.Message;
                    }
                }
                else
                {
                    errorMessage = body;
                }
                ReloadTimeLineCompleted(success, errorMessage);
            });
        }

        void ReloadTimeLineCompleted(bool success, string errorMessage)
        {
            if (success)
            {
                SetStatus("a", Color.FromArgb(0xff, 0, 0xff, 0), string.Format(
                    "{0} 取得完了",
                    DateTime.Now.ToString(),
                    this.tweets.Count
                    ));
            }
            else
            {
                string msg = string.Format("{0} 取得エラー",
                    DateTime.Now.ToString());
                if (!string.IsNullOrEmpty(errorMessage)) {
                    msg += string.Format("({0})", errorMessage);
                }
                SetStatus("r", Color.FromArgb(0xff, 0xff, 0, 0), msg);
            }
            reloadButton.IsEnabled = true;
            autoReloadTimer.Start();
        }

        /// <summary>
        /// ステータスバーのテキストを更新
        /// </summary>
        public void SetStatus(string text)
        {
            statusText.Text = (string)text;
            statusTextIn.Begin();
            statusTextHideTimer.Start();

        }
        public void SetStatus(string symbol, Color color, string text)
        {
            statusText.Inlines.Clear();

            Run symbolText = new Run();
            symbolText.FontFamily = new FontFamily("Webdings");
            symbolText.Text = symbol;
            symbolText.Foreground = new SolidColorBrush(color);
            symbolText.FontSize = 18;
            statusText.Inlines.Add(symbolText);

            Run bodyText = new Run();
            bodyText.Text = " "+text;
            statusText.Inlines.Add(bodyText);
            
            statusTextIn.Begin();
            statusTextHideTimer.Start();

        }

        /// <summary>
        /// ロードしたツイートを追加する
        /// </summary>
        /// <param name="newTweets"></param>
        public void AddTweets(Tweets newTweets)
        {
            Debug.WriteLine("AddTweets");
            bool ageChanged = false;

            foreach (var tweet in this.tweets.Where(x => x.Age < 4))
            {
                tweet.Age++;
                ageChanged = true;
            }

            // 重複チェックして、新着だけ取り出す
            Tweets uniqueTweets = new Tweets();

            foreach (var tweet in newTweets.Where(x => tweets.FindId(x.Id) == null))
            {
                tweet.Age = 0;
                uniqueTweets.Add(tweet);
            }

            // 追加するツイートがある?
            if (uniqueTweets.Count > 0)
            {
                if (tweetsPanel.TopItemIndex > 0)
                {
                    AddNewTweets(uniqueTweets);
                }
                else
                {
                    if (uniqueTweets.Count < tweetsPanel.VisibleTweetCount)
                    {
                        AddNewTweets(uniqueTweets);
                    }
                    else
                    {
                        double stopSec = 1;
                        double divAdd = ((App.Settings.AutoReload - 2) / stopSec);
                        DispatcherTimer dt = new DispatcherTimer();
                        int addCountPerOne = (int)Math.Ceiling(uniqueTweets.Count / Math.Max(1, divAdd));
                        if (addCountPerOne < 1) addCountPerOne = 1;

                        dt.Interval = TimeSpan.FromSeconds(0);
                        dt.Tick += (s, e) =>
                        {
                            dt.Interval = TimeSpan.FromSeconds(stopSec);
                            int oneIdx = Math.Max(0, uniqueTweets.Count - addCountPerOne);
                            int oneCount = Math.Min(addCountPerOne, uniqueTweets.Count);
                            AddNewTweets(uniqueTweets.GetRange(oneIdx, oneCount));
                            uniqueTweets.RemoveRange(oneIdx, oneCount);
                            if (uniqueTweets.Count == 0)
                            {
                                dt.Stop();
                            }
                        };
                        dt.Start();
                    }
                }
            }
            else if (ageChanged)
            {
                // 追加はないけど、Ageが変更された
                tweetsPanel.LayoutTweets(TweetsPanelLayoutMode.Relayout);
            }
            Debug.WriteLine("/AddTweets");
        }


        public void AddNewTweets(List<Tweet> newTweets)
        {
            tweets.InsertRange(0, newTweets);
            if (tweetsPanel.TopItemIndex > 0)
            {
                tweetsPanel.TopItemIndex += newTweets.Count;
            }
            else
            {
                tweetsPanel.LayoutTweets(TweetsPanelLayoutMode.AddNew);
            }
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(0.8);
            dt.Tick += (s, e) =>
            {
                dt.Stop();

                MediaElement me = newItemSounds[_rand.Next(0, newItemSounds.Count)];
                me.Stop();
                me.Play();
            };
            dt.Start();

            if (tweets.Count > tweetsCountMax)
            {
                tweets.RemoveRange(tweetsCountMax, this.tweets.Count - tweetsCountMax);
            }
        }



        /// <summary>
        /// リロード
        /// </summary>
        private void reloadButton_Click(object sender, RoutedEventArgs e)
        {
            reloadTimeLine();
        }

        /// <summary>
        /// 「新しい」ボタン
        /// </summary>
        private void newerButton_Click(object sender, RoutedEventArgs e)
        {
            tweetsPanel.LayoutTweets(TweetsPanelLayoutMode.Left);
        }

        /// <summary>
        /// 「古い」ボタン
        /// </summary>
        private void olderButton_Click(object sender, RoutedEventArgs e)
        {
            tweetsPanel.LayoutTweets(TweetsPanelLayoutMode.Right);
        }

        /// <summary>
        /// 「最新」ボタン
        /// </summary>
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            tweetsPanel.TopItemIndex = 0;
            tweetsPanel.LayoutTweets(TweetsPanelLayoutMode.Left);
        }

        /// <summary>
        /// ツイート
        /// </summary>
        private void newPostButton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow dlg = new EditWindow();
            dlg.FontSize = App.Settings.BodyFontSize;
            dlg.FontFamily = new FontFamily(App.Settings.FontName);
            dlg.FontWeight = App.Settings.BodyFontBold ? FontWeights.Bold : FontWeights.Normal; ;
            dlg.Show();
        }

    }
}
