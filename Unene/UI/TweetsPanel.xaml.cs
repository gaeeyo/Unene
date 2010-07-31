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
using System.Diagnostics;
using System.Collections.Generic;

namespace Unene
{
    public enum TweetsPanelLayoutMode
    {
        None,
        Relayout,
        AddNew,
        Left,
        Right
    }

    public partial class TweetsPanel : UserControl
    {
        public TweetsPanel()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(TweetsPanel_Loaded);
        }
        public Tweets Tweets = new Tweets();
        public int Columns { get; set; }

        private Duration moveDuration = new Duration(TimeSpan.FromSeconds(0.5));
        private SineEase moveEasing = new SineEase()

        {
            EasingMode = EasingMode.EaseInOut
        };
        private SineEase moveEasing2 = new SineEase()
        {
            EasingMode = EasingMode.EaseInOut
        };


        private PropertyPath canvasLeftPropertyPath = new PropertyPath(Canvas.LeftProperty);
        private PropertyPath canvasTopPropertyPath = new PropertyPath(Canvas.TopProperty);

        private List<TweetItemControl> tweetControls = new List<TweetItemControl>();
        private List<TweetItemControl> recycleTweetControls = new List<TweetItemControl>();


        public int TopItemIndex { get; set; }

        void TweetsPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // ビジュアル設定が変更されたときに表示中のツイートコントロールをすべて更新して
            // レイアウトしなおす
            AppSettings.VisualChanged += delegate(object sender2)
            {
                foreach (var tic in tweetControls)
                {
                    tic.ApplySettings();
                }
                foreach (var tic in recycleTweetControls)
                {
                    tic.ApplySettings();
                }
                LayoutTweets(TweetsPanelLayoutMode.Relayout);
            };
        }

        private void addTweet(Tweet tweet)
        {
            TweetItemControl t = new TweetItemControl();
            t.Tweet = tweet;
            tweetControls.Add(t);
            LayoutRoot.Children.Add(t);
        }

        private TweetItemControl AddTweetItemControl()
        {
            var tic = recycleTweetControls.FirstOrDefault(x => x.Parent == null) ?? new TweetItemControl();
            LayoutRoot.Children.Add(tic);
            tweetControls.Add(tic);
            return tic;
        }

        private void RemoveTweetItemControl(TweetItemControl tic, TweetsPanelLayoutMode mode, Storyboard sb)
        {
            recycleTweetControls.Add(tic);
            tweetControls.Remove(tic);
            switch (mode)
            {
                case TweetsPanelLayoutMode.Left:
                case TweetsPanelLayoutMode.Relayout: // 右に押し出す
                    {
                        DoubleAnimation da = new DoubleAnimation()
                        {
                            To = (double)tic.GetValue(Canvas.LeftProperty) + ActualWidth,
                            Duration = moveDuration,
                            EasingFunction = moveEasing,
                        };
                        da.Completed += (sender, e) =>
                        {
                            LayoutRoot.Children.Remove(tic);
                        };
                        Storyboard.SetTarget(da, tic);
                        Storyboard.SetTargetProperty(da, canvasLeftPropertyPath);
                        sb.Children.Add(da);
                    }
                    break;
                case TweetsPanelLayoutMode.Right:
                    {
                        DoubleAnimation da = new DoubleAnimation()
                        {
                            To = (double)tic.GetValue(Canvas.LeftProperty) - ActualWidth,
                            Duration = moveDuration,
                            EasingFunction = moveEasing,
                        };
                        da.Completed += (sender, e) =>
                        {
                            LayoutRoot.Children.Remove(tic);
                        };
                        Storyboard.SetTarget(da, tic);
                        Storyboard.SetTargetProperty(da, canvasLeftPropertyPath);
                        sb.Children.Add(da);
                    }
                    break;
                default:    // すぐ消す
                    LayoutRoot.Children.Remove(tic);
                    break;
            }
        }

        /// <summary>
        /// ツイートをレイアウトする
        /// </summary>
        public void LayoutTweets(TweetsPanelLayoutMode mode)
        {
            Panel container = (Panel)Parent;
            Debug.WriteLine("cursor: {0}/{1}", TopItemIndex, Tweets.Count);
            const int sideMargin = 4;
            double width = container.ActualWidth - (sideMargin * 2);
            double height = container.ActualHeight;
            double columnWidth = width / Columns;

            double x = sideMargin;
            double y = 0;
            int column = 0;

            TimeSpan newItemDelay = TimeSpan.FromSeconds(0.5);
            Duration newItemDuration = new Duration(TimeSpan.FromSeconds(0.5));

            Storyboard sb = new Storyboard();

            var newItems = new List<TweetItemControl>();

            switch (mode)
            {
                case TweetsPanelLayoutMode.Right:
                    TopItemIndex = Math.Min(TopItemIndex + tweetControls.Count, Tweets.Count - 1);
                    break;
                case TweetsPanelLayoutMode.Left:
                    {
                        for (; TopItemIndex >= 0; TopItemIndex--)
                        {
                            var tweet = Tweets[TopItemIndex];
                            var tic = tweetControls.FirstOrDefault(c => c.Tweet == tweet);
                            bool newItem = false;
                            if (tic == null)
                            {
                                newItem = true;
                                tic = AddTweetItemControl();
                                tic.Tweet = tweet;
                                tic.SetValue(Canvas.WidthProperty, columnWidth);
                                tic.UpdateLayout();
                                newItems.Add(tic);
                                Debug.WriteLine("newItems.Add {0} {1}", TopItemIndex, tweet.Text);
                            }
                            y += tic.ActualHeight;
                            if (y > height)
                            {
                                y = 0;
                                column++;
                                if (column >= Columns)
                                {
                                    if (newItem)
                                    {
                                        RemoveTweetItemControl(tic, TweetsPanelLayoutMode.None, sb);
                                        Debug.WriteLine("newItems.Remove {0} {1}", TopItemIndex, tweet.Text);
                                        newItems.Remove(tic);
                                    }
                                    TopItemIndex++;
                                    break;
                                }
                            }
                        }
                        if (TopItemIndex < 0) TopItemIndex = 0;
                        column = 0;
                        y = 0;
                    }
                    break;
            }

            bool outside = false;
            int index = -1;
            foreach (var tweet in Tweets)
            {
                index++;

                var tic = tweetControls.FirstOrDefault(c => c.Tweet == tweet);
                if (tic != null)
                {
                    if (outside || index < TopItemIndex)
                    {
                        RemoveTweetItemControl(tic, mode, sb);
                        continue;
                    }
                }
                if (index < TopItemIndex)
                {
                    continue;
                }

                bool newItem = false;
                if (tic == null)
                {
                    if (outside)
                    {
                        continue;
                    }
                    newItem = true;
                    tic = AddTweetItemControl();
                    tic.Tweet = tweet;
                }

                // 高さ決定
                tic.SetValue(Canvas.WidthProperty, columnWidth);
                tic.UpdateLayout();
                tic.UpdateTime();

                // 表示位置決定
                if (y + tic.ActualHeight > height)
                {
                    y = 0;
                    x += columnWidth;
                    column++;
                    if (column >= Columns)
                    {
                        outside = true;
                        RemoveTweetItemControl(tic, newItem ? TweetsPanelLayoutMode.None : mode, sb);
                        continue;
                    }
                }

                if (mode == TweetsPanelLayoutMode.None)
                {
                    tic.SetValue(Canvas.LeftProperty, x);
                    tic.SetValue(Canvas.TopProperty, y);
                }
                else
                {
                    DoubleAnimation daLeft = new DoubleAnimation()
                    {
                        Duration = moveDuration,
                        To = x,
                        EasingFunction = moveEasing,
                    };

                    DoubleAnimation daTop = new DoubleAnimation()
                    {
                        Duration = moveDuration,
                        To = y,
                        EasingFunction = moveEasing,
                    };

                    // 新アイテムの扱い
                    if (newItem)
                    {
                        switch (mode)
                        {
                            case TweetsPanelLayoutMode.AddNew:   // 左から入ってくる
                                daLeft.BeginTime = newItemDelay;
                                daTop.BeginTime = newItemDelay;
                                daLeft.Duration = newItemDuration;
                                daTop.Duration = newItemDuration;
                                tic.SetValue(Canvas.LeftProperty, -columnWidth);
                                tic.SetValue(Canvas.TopProperty, y);
                                break;
                            case TweetsPanelLayoutMode.Relayout: // 右から入ってくる
                            case TweetsPanelLayoutMode.Right:
                                tic.SetValue(Canvas.LeftProperty, width + x);
                                tic.SetValue(Canvas.TopProperty, y);
                                break;
                            case TweetsPanelLayoutMode.Left:
                                break;
                        }
                    }
                    else if (mode == TweetsPanelLayoutMode.Left && newItems.Remove(tic))
                    {   // 左から入ってくる
                        Debug.WriteLine("使った: {0} {1}", TopItemIndex, tweet.Text);
                        tic.SetValue(Canvas.LeftProperty, x - width);
                        tic.SetValue(Canvas.TopProperty, y);
                    }
                    sb.Children.Add(daLeft);
                    sb.Children.Add(daTop);

                    Storyboard.SetTarget(daLeft, tic);
                    Storyboard.SetTarget(daTop, tic);
                    Storyboard.SetTargetProperty(daLeft, canvasLeftPropertyPath);
                    Storyboard.SetTargetProperty(daTop, canvasTopPropertyPath);
                }

                y += (int)tic.ActualHeight;
            }
            foreach (var t in newItems)
            {
                Debug.WriteLine("残った!!: {0}", t.Tweet.Text);
            }

            Width = Math.Max(x + columnWidth, container.ActualWidth);
            container.UpdateLayout();

            sb.Begin();
        }

    }
}
