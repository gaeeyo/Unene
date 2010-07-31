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

namespace Unene.UI
{
    public class TweetTextBox : RichTextBox
    {
        public static double FontHeightHint { get; set; }

        private int _fontSizing = 0;
        private double _baseFontSize = 0;
        private double _width = 0;
        
        public TweetTextBox()
        {
            this.DefaultStyleKey = typeof(TweetTextBox);
            AutoFontSize = false;
            this.SizeChanged += new SizeChangedEventHandler(TweetTextBox_SizeChanged);
            this.LayoutUpdated += new EventHandler(TweetTextBox_LayoutUpdated);
        }

        void TweetTextBox_LayoutUpdated(object sender, EventArgs e)
        {
            if (AutoFontSize && FontSize > 6)
            {
                if (ActualHeight <= DesiredSize.Height)
                {
                    //textBody.FontSize = Math.Floor(textBody.FontSize * 0.9);
                    _fontSizing++;
                    if (_fontSizing == 1)
                    {
                        FontSize = FontHeightHint * 0.75;
                    }
                    else
                    {
                        FontSize = Math.Floor((FontHeightHint - 1) / _fontSizing); //textBody.FontSize * 0.9);
                    }
                    //Debug.WriteLine("FontSize: {0}", textBody.FontSize);
                }
            }
            
        }

        bool IsAutoFontSize
        {
            get
            {
                return AutoFontSize && HorizontalAlignment == System.Windows.HorizontalAlignment.Stretch;
            }
        }

        void TweetTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsAutoFontSize)
            {
                if (_width != ActualWidth)
                {
                    _width = ActualWidth;
                    MaxHeight = double.MaxValue;
                    StartFontSizing();
                }
            }
            else
            {
                MaxHeight = double.MaxValue;
                FontSize = BaseFontSize;
            }
        }

        public void StartFontSizing()
        {
            if (IsAutoFontSize)
            {
                _width = ActualWidth;
                startFontSizing();
            }
        }

        void startFontSizing()
        {
            FontSize = FontHeightHint;
            _fontSizing = 0;
        }

        public static DependencyProperty AutoFontSizeProperty = DependencyProperty.Register(
                "AutoFontSize", typeof(bool), typeof(TweetTextBox), null);

        public bool AutoFontSize
        {
            get { return (bool)GetValue(AutoFontSizeProperty); }
            set { SetValue(AutoFontSizeProperty, value); }
        }

        public double BaseFontSize
        {
            get { return _baseFontSize; }
            set
            {
                _baseFontSize = value;
                FontSize = value;
            }
        }

    }
}
