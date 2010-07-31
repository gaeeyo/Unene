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
using System.Windows.Threading;
using Gaeeyo;

namespace Unene
{
    public partial class ViewSettingControl : UserControl
    {
        public ViewSettingControl()
        {
            InitializeComponent();
            if (System.ComponentModel.DesignerProperties.IsInDesignTool)
                return;

            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(0.5);
            dt.Tick += (object sender, EventArgs e) =>
            {
                dt.Stop();
                App.Settings.ApplyVisual();
            };
            Action applyVisual = () =>
            {
                dt.Stop();
                dt.Start();
            };

            // フォントの一覧を設定
            foreach (string name in Gaeeyo.FontList.GetList())
            {
                FontName.Items.Add(name);
            }
            
            // フォント名
            string fontName = App.Settings.FontName;
            object i = FontName.Items.FirstOrDefault((x) => (string)x == fontName);
            FontName.SelectedItem = i ?? FontName.Items[0];

            FontName.SelectionChanged += (sender2, e2) =>
            {
                App.Settings.FontName = (string)FontName.SelectedItem;
                App.Settings.ApplyVisual();
            };

            // フォントサイズ
            bodyFontSizeSlider.Value = App.Settings.BodyFontSize;
            bodyFontSizeSlider.ValueChanged += (sender2, e2) =>
            {
                App.Settings.BodyFontSize = (int)e2.NewValue;
                applyVisual();
            };

            // アイコンサイズ設定
            iconSizeSlider.Value = App.Settings.IconSize;
            iconSizeSlider.ValueChanged += (sender2, e2) =>
            {
                App.Settings.IconSize = (int)e2.NewValue;
                applyVisual();
            };

            // 太文字
            bodyBold.IsChecked = App.Settings.BodyFontBold;
            bodyBold.Click += (sender2, e2) =>
            {
                App.Settings.BodyFontBold = bodyBold.IsChecked ?? false;
                applyVisual();
            };

            // ツイートの文字の色
            bindColorButton(tweetColor, App.Settings.BodyTextColor,
                (x)=> App.Settings.BodyTextColor = x );

            // ツイートの背景色
            bindColorButton(tweetBackground, App.Settings.BodyBackgroundColor,
                (x) => App.Settings.BodyBackgroundColor = x);

            // ツイートの新着の背景色
            bindColorButton(newTweetBackground, App.Settings.NewBodyBackgroundColor,
                (x) => App.Settings.NewBodyBackgroundColor = x);

            // 背景色
            bindColorButton(tweetsPanelColor, App.Settings.BackgroundColor, 
                (x) => App.Settings.BackgroundColor = x);


            // レイアウトモード
            viewStyles.SelectedIndex = App.Settings.TweetViewStyle;

            // 表示モード切替
            viewStyles.SelectionChanged += (s2, e2) =>
            {
                App.Settings.TweetViewStyle = viewStyles.SelectedIndex;
                //setControlState();
                applyVisual();
            };

            // 透明度設定
            bodyBackgroundOpacity.SelectedItem = bodyBackgroundOpacity.Items[0];
            foreach (ComboBoxItem cbi in bodyBackgroundOpacity.Items)
            {
                if (Convert.ToDouble(cbi.Tag) == App.Settings.BodyBackgroundOpacity)
                {
                    bodyBackgroundOpacity.SelectedItem = cbi;
                    break;
                }
            }
            bodyBackgroundOpacity.SelectionChanged += (s2, e2) =>
            {
                App.Settings.BodyBackgroundOpacity = Convert.ToDouble(((ComboBoxItem)(bodyBackgroundOpacity.SelectedItem)).Tag);
                applyVisual();
            };

            // 時計を表示
            clock.IsChecked = App.Settings.UseClock;
            clock.Click += (s2, e2) =>
            {
                App.Settings.UseClock = clock.IsChecked ?? false;
                applyVisual();
            };

            //setControlState();

        }

        /*
        void setControlState()
        {
            bodyFontSize.IsEnabled = bodyFontSizeSlider.IsEnabled = !App.Settings.AutoBodyFontSize;
        }
         * */

        void bindColorButton(UI.ColorButton btn, Color defaultValue, Action<Color> setter)
        {
            btn.Color.Color = defaultValue;
            btn.Click += (s2, e2) =>
            {
                Gaeeyo.ColorPickerDialog dlg = new Gaeeyo.ColorPickerDialog();
                dlg.EditColor = btn.Color.Color;
                dlg.Show();
                dlg.Closed += (sender, e) =>
                {
                    if (dlg.DialogResult == true)
                    {
                        setter(dlg.EditColor);
                        btn.Color.Color = dlg.EditColor;
                        App.Settings.ApplyVisual();
                    }
                };
            };

        }

        private void wallPaper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "画像ファイル(*.jpg, *.png)|*.jpg;*.png";
                ofd.FilterIndex = 1;
                if (ofd.ShowDialog() == true)
                {
                    var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
                    var ss = ofd.File.OpenRead();
                    var ds = store.CreateFile("wallpaper");

                    if (store.AvailableFreeSpace < ss.Length)
                    {
                        store.IncreaseQuotaTo(store.Quota + Convert.ToInt64(ss.Length * 1.5));
                    }


                    using (ss)
                    using (ds)
                    {
                        byte[] buffer = new byte[1000];

                        while (ss.Position < ss.Length)
                        {
                            int len = ss.Read(buffer, 0, buffer.Length);
                            if (len == 0) break;
                            ds.Write(buffer, 0, len);
                        }
                    }

                    App.Settings.UseWallPaper = true;
                    App.Settings.ApplyVisual();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

    }
}
