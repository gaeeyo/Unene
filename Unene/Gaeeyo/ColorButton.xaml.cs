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
using System.Windows.Controls.Primitives;

namespace Gaeeyo
{
    public partial class ColorButton : Button
    {
        Rectangle _colorBox = null;
        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static DependencyProperty ColorProperty = DependencyProperty.Register(
                "Color", typeof(SolidColorBrush), typeof(ColorButton), null);

        public ColorButton() : base()
        {
            this.DefaultStyleKey = typeof(ColorButton);
            SizeChanged += new SizeChangedEventHandler(ColorButton_SizeChanged);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _colorBox = (Rectangle)GetTemplateChild("ColorBox");
        }

        void ColorButton_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _colorBox.Width = _colorBox.ActualHeight;
        }
    }
}
