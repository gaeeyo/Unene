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
using System.Linq;
using System.Diagnostics;
using System.Windows.Threading;

namespace Unene.UI
{
    public enum TweetsPanelLayoutMode
    {
        None,
        Relayout,
        AddNew,
        Left,
        Right
    }

    public class TweetsPanelItem : TweetView
    {

    }

    public class TweetsPanel : Canvas
    {
        public Tweets Tweets = new Tweets();
        public int Columns { get; set; }
        public int TweetMargin { get; set; }

        Duration moveDuration = new Duration(TimeSpan.FromSeconds(0.5));
        public double MoveAnimationTime
        {
            set
            {
                moveDuration = new Duration(TimeSpan.FromSeconds(0.25));
            }
        }


        SineEase moveEasing = new SineEase() { EasingMode = EasingMode.EaseInOut };
        SineEase moveEasing2 = new SineEase() { EasingMode = EasingMode.EaseInOut };
        PropertyPath canvasLeftPropertyPath = new PropertyPath(Canvas.LeftProperty);
        PropertyPath canvasTopPropertyPath = new PropertyPath(Canvas.TopProperty);

        List<TweetsPanelItem> tweetControls = new List<TweetsPanelItem>();
        List<TweetsPanelItem> recycleTweetControls = new List<TweetsPanelItem>();

        DispatcherTimer _layoutTimer = new DispatcherTimer();

        TextBlock _dummyText;

        public int TopItemIndex { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TweetsPanel()
        {
            TweetMargin = 0;
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                _layoutTimer.Interval = TimeSpan.FromSeconds(0.2);
                _layoutTimer.Tick += (s,e) => {
                    _layoutTimer.Stop();
                    LayoutTweets(TweetsPanelLayoutMode.Relayout);
                };

                // ツイートパネルをサイズ変更したときに、
                /*
                SizeChanged += (sender2, e2) =>
                {
                    _layoutTimer.Start();
                };
                 * */
                _dummyText = new TextBlock();
                _dummyText.Text = "0あ";
                _dummyText.Visibility = System.Windows.Visibility.Collapsed;
                Children.Add(_dummyText);
        
            }
        }

        // 少し遅れて再レイアウトするように
        public void DelayLayout() {
            _layoutTimer.Start();
        }

        public void ApplySettings()
        {
            _dummyText.FontFamily = new FontFamily(App.Settings.FontName);
            _dummyText.FontSize = App.Settings.IconSize;
            _dummyText.UpdateLayout();

            TweetViewResource.BodyBackground.Color = App.Settings.BodyBackgroundColor;
            TweetViewResource.NewBodyBackground.Color = App.Settings.NewBodyBackgroundColor;
            TweetViewResource.FontHeightHint = Math.Floor(_dummyText.FontSize * (_dummyText.FontSize / _dummyText.ActualHeight)) - 1;

            TweetViewResource.BodyTextBrush.Color = App.Settings.BodyTextColor;

            if (App.Settings.BodyBackgroundOpacity >= 0)
            {
                TweetViewResource.BodyBackgroundOpacity = Convert.ToByte(255 * (1 - App.Settings.BodyBackgroundOpacity));
            }
            else
            {
                TweetViewResource.BodyBackgroundOpacity = 255;
            }

            foreach (var tic in tweetControls)
            {
                tic.ApplySettings();
            }
            foreach (var tic in recycleTweetControls)
            {
                tic.ApplySettings();
            }
            LayoutTweets(TweetsPanelLayoutMode.Relayout);
        }

        public int FirstColumnsCount
        {
            get
            {
                int count = 0;
                if (tweetControls.Count > 0)
                {
                    double left = (double)tweetControls[0].GetValue(Canvas.LeftProperty);
                    count = tweetControls.Where(
                        x => (double)x.GetValue(Canvas.LeftProperty) == left
                        ).Count();
                }
                return count;
            }
        }
        public int VisibleTweetCount 
        {
            get { return tweetControls.Count; }
        }

        void addTweet(Tweet tweet)
        {
            TweetsPanelItem t = new TweetsPanelItem();
            t.Tweet = tweet;
            tweetControls.Add(t);
            Children.Add(t);
        }

        TweetsPanelItem createNewControl()
        {
            TweetsPanelItem tic = new TweetsPanelItem();
            return tic;
        }

        TweetsPanelItem AddTweetItemControl()
        {
            var tic = recycleTweetControls.FirstOrDefault(x => x.Parent == null)
                ?? createNewControl();
            Children.Add(tic);
            tweetControls.Add(tic);
            return tic;
        }

        void RemoveTweetItemControl(TweetsPanelItem tic, TweetsPanelLayoutMode mode, Storyboard sb)
        {
            tweetControls.Remove(tic);
            switch (mode)
            {
                case TweetsPanelLayoutMode.AddNew:
                case TweetsPanelLayoutMode.Left:
                case TweetsPanelLayoutMode.Relayout: // 右に押し出す
                    {
                        DoubleAnimation da = new DoubleAnimation() {
                            To = (double)tic.GetValue(Canvas.LeftProperty) + ActualWidth,
                            Duration = moveDuration,
                            EasingFunction = moveEasing,
                        };
                        da.Completed += (sender, e) =>
                        {
                            Children.Remove(tic);
                            recycleTweetControls.Add(tic);
                        };
                        Storyboard.SetTarget(da, tic);
                        Storyboard.SetTargetProperty(da, canvasLeftPropertyPath);
                        sb.Children.Add(da);
                    }
                    break;
                case TweetsPanelLayoutMode.Right:
                    {
                        DoubleAnimation da = new DoubleAnimation() {
                            To = (double)tic.GetValue(Canvas.LeftProperty) - ActualWidth,
                            Duration = moveDuration,
                            EasingFunction = moveEasing,
                        };
                        da.Completed += (sender, e) =>
                        {
                            Children.Remove(tic);
                            recycleTweetControls.Add(tic);
                        };
                        Storyboard.SetTarget(da, tic);
                        Storyboard.SetTargetProperty(da, canvasLeftPropertyPath);
                        sb.Children.Add(da);
                    }
                    break;
                default:    // すぐ消す
                    Children.Remove(tic);
                    recycleTweetControls.Add(tic);
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
            const int sideMargin = 6;
            int tweetMargin = TweetMargin;

            double width = container.ActualWidth - (sideMargin * 2);
            double height = container.ActualHeight;
            double columnWidth = Math.Floor( width / Columns);
            double tweetWidth = columnWidth - tweetMargin;

            double x = sideMargin;
            double y = 0;
            int column = 0;

            TimeSpan newItemDelay = moveDuration.TimeSpan;
            Duration newItemDuration = moveDuration;

            Storyboard sb = new Storyboard();

            var newItems = new List<TweetsPanelItem>();

            switch (mode)
            {
                case TweetsPanelLayoutMode.Right:
                    TopItemIndex = Math.Min(TopItemIndex + tweetControls.Count, Tweets.Count - 1);
                    break;
                case TweetsPanelLayoutMode.Left:
                    {
                        for (;  TopItemIndex >= 0; TopItemIndex--)
                        {
                            var tweet = Tweets[TopItemIndex];
                            var tic = tweetControls.FirstOrDefault(c => c.Tweet == tweet);
                            bool newItem = false;
                            if (tic == null)
                            {
                                newItem = true;
                                tic = AddTweetItemControl();
                                tic.Tweet = tweet;
                                tic.SetValue(Canvas.WidthProperty, tweetWidth);
                                tic.UpdateLayout();
                                newItems.Add(tic);
                                Debug.WriteLine("newItems.Add {0} {1}", TopItemIndex, tweet.Text);
                            }
                            y += tic.ActualHeight + tweetMargin;
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
                //if (index > 0) break;   // テスト用

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
                if (tic.Width != tweetWidth)
                {
                    tic.SetValue(Canvas.WidthProperty, tweetWidth);
                }
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
                    DoubleAnimation daLeft = new DoubleAnimation() {
                        Duration = moveDuration,
                        To = x,
                        EasingFunction = moveEasing,
                    };

                    DoubleAnimation daTop = new DoubleAnimation() {
                        Duration = moveDuration,
                        To = y,
                        EasingFunction = moveEasing,
                    };

                    // 新アイテムの扱い
                    if (newItem)
                    {
                        switch (mode) {
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
                            tic.SetValue(Canvas.LeftProperty, width+x);
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

                y += (int)tic.ActualHeight + tweetMargin;
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
