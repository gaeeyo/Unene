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
    public partial class NGWordSettingControl : UserControl
    {
        public NGWordSettingControl()
        {
            InitializeComponent();
            if (System.ComponentModel.DesignerProperties.IsInDesignTool)
                return;

            ngWords.Text = App.Settings.NGWords;

            Unloaded += new RoutedEventHandler(NGWordSettingControl_Unloaded);
        }

        void NGWordSettingControl_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Settings.NGWords = ngWords.Text;
        }
    }
}
