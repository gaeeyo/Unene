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

namespace Unene
{
    public partial class ClockControl : UserControl
    {
        public ClockControl()
        {
            InitializeComponent();

            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(0);
                timer.Tick += (s, e) =>
                {
                    var today = DateTime.Now;
                    clockText.Text = today.ToString(" H:mm ");
                    timer.Interval = TimeSpan.FromSeconds(60 - today.Second);
                    System.Diagnostics.Debug.WriteLine("時計更新");
                };
                timer.Start();
            }
        }

        public double ClockFontSize
        {
            get { return clockText.FontSize; }
            set { clockText.FontSize = value; }
        }
        public FontFamily ClockFontFamily
        {
            get { return clockText.FontFamily; }
            set { clockText.FontFamily = value; }
        }
    }
}
