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
    public class ColorButton : Button
    {
        Rectangle _colorBox = null;
        Grid _layoutRoot = null;

        public ColorButton()
        {
            this.DefaultStyleKey = typeof(ColorButton);
            //if (System.ComponentModel.DesignerProperties.IsInDesignTool)
             //   return;
            SizeChanged += new SizeChangedEventHandler(ColorButton_SizeChanged);
        }

        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static DependencyProperty ColorProperty = DependencyProperty.Register(
                "Color", typeof(SolidColorBrush), typeof(ColorButton), null);

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _colorBox = (Rectangle)GetTemplateChild("ColorBox");
            _layoutRoot = (Grid)GetTemplateChild("LayoutRoot");
        }

        void ColorButton_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _layoutRoot.ColumnDefinitions[0].Width = new GridLength(_layoutRoot.ActualHeight);
        }
    }
}
