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
using System.Windows.Data;

namespace Gaeeyo
{
    public class Utils
    {
        //public static void SliderValueChanged_ToInt32(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    Slider ctrl = sender as Slider;
        //    ctrl.Value = Convert.ToInt32(e.NewValue);
        //}

        public static void Navigate(Uri uri)
        {
            var hb = new MyHyperlinkButton(uri);
        }

        private class MyHyperlinkButton : HyperlinkButton
        {
            public MyHyperlinkButton(Uri uri)
            {
                NavigateUri = uri;
                TargetName = "_balnk";
                OnClick();
            }
        }

        /// <summary>
        /// Color 型から HSL 値に変換する
        /// </summary>
        /// <param name="color"></param>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="lightness"></param>
        public static void RgbToHsl(Color color, out double hue, out double saturation, out double lightness)
        {
            double r, g, b, h, s, l;

            r = color.R / 255.0;
            g = color.G / 255.0;
            b = color.B / 255.0;

            double maxColor = Math.Max(r, Math.Max(g, b));
            double minColor = Math.Min(r, Math.Min(g, b));

            if (r == g && r == b)
            {
                h = 0.0;
                s = 0.0;
                l = r;
            }
            else
            {
                l = (minColor + maxColor) / 2;
                if (l < 0.5)
                    s = (maxColor - minColor) / (maxColor + minColor);
                else
                    s = (maxColor - minColor) / (2.0 - maxColor - minColor);

                if (r == maxColor)
                    h = (g - b) / (maxColor - minColor);
                else if (g == maxColor)
                    h = 2.0 + (b - r) / (maxColor - minColor);
                else
                    h = 4.0 + (r - g) / (maxColor - minColor);

                h /= 6;

                if (h < 0)
                    ++h;
            }

            // 0 ～ 1 の範囲内に制限する
            if (h < 0) h = 0;
            if (h > 1) h = 1;
            if (s < 0) s = 0;
            if (s > 1) s = 1;
            if (l < 0) l = 0;
            if (l > 1) l = 1;

            hue = h * 360;
            saturation = s;
            lightness = l;
        }
        // HSL 値から Color 型に変換する
        // hue        : 0 ～ 360
        // saturation : 0 ～ 1
        // lightness  : 0 ～ 1
        public static Color HslToRgb(double hue, double saturation, double lightness)
        {
            double r, g, b, h, s, l, s1, s2, r1, g1, b1;

            h = hue / 360.0;
            s = saturation;
            l = lightness;

            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                if (l < 0.5)
                {
                    s2 = l * (1 + s);
                }
                else
                {
                    s2 = (l + s) - (l * s);
                }

                s1 = 2 * l - s2;
                r1 = h + 1.0 / 3.0;

                if (r1 > 1)
                {
                    --r1;
                }

                g1 = h;
                b1 = h - 1.0 / 3.0;

                if (b1 < 0)
                    ++b1;

                // R
                if (r1 < 1.0 / 6.0)
                    r = s1 + (s2 - s1) * 6.0 * r1;
                else if (r1 < 0.5)
                    r = s2;
                else if (r1 < 2.0 / 3.0)
                    r = s1 + (s2 - s1) * ((2.0 / 3.0) - r1) * 6.0;
                else
                    r = s1;

                // G
                if (g1 < 1.0 / 6.0)
                    g = s1 + (s2 - s1) * 6.0 * g1;
                else if (g1 < 0.5)
                    g = s2;
                else if (g1 < 2.0 / 3.0)
                    g = s1 + (s2 - s1) * ((2.0 / 3.0) - g1) * 6.0;
                else g = s1;

                // B
                if (b1 < 1.0 / 6.0)
                    b = s1 + (s2 - s1) * 6.0 * b1;
                else if (b1 < 0.5)
                    b = s2;
                else if (b1 < 2.0 / 3.0)
                    b = s1 + (s2 - s1) * ((2.0 / 3.0) - b1) * 6.0;
                else
                    b = s1;
            }

            // 0 ～ 1 の範囲内に制限する
            if (h < 0) h = 0;
            if (h > 1) h = 1;
            if (s < 0) s = 0;
            if (s > 1) s = 1;
            if (l < 0) l = 0;
            if (l > 1) l = 1;

            return Color.FromArgb(0xff, Convert.ToByte(r * 255), Convert.ToByte(g * 255), Convert.ToByte(b * 255));
        }

        public static Color BlendColor(Color c1, Color c2, double p2)
        {
            if (p2 <= 0) return c1;
            if (p2 >= 1) return c2;

            //c1.A = TweetViewResource.BodyBackgroundOpacity;
            //c2.A = TweetViewResource.BodyBackgroundOpacity;
            double p1 = 1 - p2;
            return Color.FromArgb(255/*TweetViewResource.BodyBackgroundOpacity*/,
                    Convert.ToByte((c1.R * p1 + c2.R * p2)),
                    Convert.ToByte((c1.G * p1 + c2.G * p2)),
                    Convert.ToByte((c1.B * p1 + c2.B * p2)));
        }

    }

    public class BrushToColorConverter : IValueConverter
    {
        // This converts the DateTime object to the string to display.
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            SolidColorBrush brush = value as SolidColorBrush;
            if (brush != null) {
                return brush.Color;
            }
            return Colors.Transparent;
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }    
    }
}
