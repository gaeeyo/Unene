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

namespace Gaeeyo
{
    public class OutOfBrowserUtil
    {
        public static bool IsOutOfBrowser(Panel parent)
        {
            // OutOfBrowserで動作しているかどうか確認
            bool isOutOfBrowser = Application.Current.IsRunningOutOfBrowser;

            if (!isOutOfBrowser) {
            }
            return isOutOfBrowser;
        }
    }
}
