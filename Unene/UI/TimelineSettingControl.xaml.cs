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
    public partial class TimelineSettingControl : UserControl
    {
        public TimelineSettingControl()
        {
            InitializeComponent();

            if (System.ComponentModel.DesignerProperties.IsInDesignTool)
                return;

            // リロード間隔のスライダーの設定
            int autoReload = App.Settings.AutoReload;
            object i = AutoReload.Items.FirstOrDefault((x) =>
            {
                return Convert.ToInt32(((ComboBoxItem)x).Tag) == autoReload;
            });
            AutoReload.SelectedItem = i ?? AutoReload.Items[0];

            Unloaded += new RoutedEventHandler(TimelineSettingControl_Unloaded);

            // タイムラインの設定を設定画面に反映
            foreach (TimeLineSetting t in App.Settings.TimeLines)
            {
                switch (t.TimeLineType)
                {
                    case TimeLineTypes.Home:
                        homeCheck.IsChecked = t.IsEnabled;
                        break;
                    case TimeLineTypes.Search:
                        searchCheck.IsChecked = t.IsEnabled;
                        searchText.Text = t.Text;
                        break;
                    case TimeLineTypes.List:
                        listCheck.IsChecked = t.IsEnabled;
                        listText.Text = t.Text;
                        break;
                }
            }
        }

        void TimelineSettingControl_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Settings.AutoReload = 
                Convert.ToInt32(((ComboBoxItem)AutoReload.SelectedItem).Tag);

            TimeLineSetting tl;

            // ホーム
            if (App.Settings.TimeLines.Count < 1)
            {
                App.Settings.TimeLines.Add(new TimeLineSetting());
            }
            tl = App.Settings.TimeLines[0];
            tl.TimeLineType = TimeLineTypes.Home;
            tl.IsEnabled = (bool)homeCheck.IsChecked;
            tl.NextID = 0;

            // 検索
            if (App.Settings.TimeLines.Count < 2)
            {
                App.Settings.TimeLines.Add(new TimeLineSetting());
            }
            tl = App.Settings.TimeLines[1];
            tl.TimeLineType = TimeLineTypes.Search;
            tl.IsEnabled = (bool)searchCheck.IsChecked;
            tl.Text = searchText.Text;
            tl.NextID = 0;

            // リスト
            if (App.Settings.TimeLines.Count < 3)
            {
                App.Settings.TimeLines.Add(new TimeLineSetting());
            }
            tl = App.Settings.TimeLines[2];
            tl.TimeLineType = TimeLineTypes.List;
            tl.IsEnabled = (bool)listCheck.IsChecked;
            tl.Text = listText.Text;
            tl.NextID = 0;
        }
    }
}
