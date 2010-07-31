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

namespace Gaeeyo
{
    public partial class ColorPicker : UserControl
    {
        Color _color;
        double _h;
        double _s;
        double _l = 0.5;

        //public event EventHandler Changed;
        private bool isInSetting = false;

        public Color Color
        {
            get { return _color; }
            set {
                _color = value;

                isInSetting = true;
                colorR.Value = _color.R;
                colorG.Value = _color.G;
                colorB.Value = _color.B;
                isInSetting = false;
                //Utils.RgbToHsl(_color, out _h, out _s, out _l);


                rgbToHsl();
            }
        }

        void GaeeyoColorPicker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetLeft(picker1Pointer, _h / 360 * picker1.ActualWidth);
            Canvas.SetTop(picker1Pointer, (1 - _s) * picker1.ActualHeight);
            Canvas.SetTop(picker2Pointer, (1 - _l) * picker2.ActualHeight);
        }

        public ColorPicker()
        {
            InitializeComponent();

            this.SizeChanged += new SizeChangedEventHandler(GaeeyoColorPicker_SizeChanged);

            if (System.ComponentModel.DesignerProperties.IsInDesignTool)
                return;

            bool dragging = false;

            // 色合いばーのドラッグ対応
            picker1.MouseLeftButtonDown += (s, e) =>
            {
                dragging = true;
                picker1.CaptureMouse();
            };
            picker1.MouseLeftButtonUp += (s, e) =>
            {
                dragging = false;
                picker1.ReleaseMouseCapture();
            };
            picker1.MouseMove += (s, e) =>
            {
                if (dragging)
                {
                    Point pt = e.GetPosition(picker1);

                    double x = pt.X;
                    if (x < 0) x = 0;
                    if (x >= picker1.ActualWidth) x = picker1.ActualWidth - 1;

                    double y = pt.Y;
                    if (y < 0) y = 0;
                    if (y > picker1.ActualHeight) y = picker1.ActualHeight;

                    Canvas.SetLeft(picker1Pointer, x);
                    Canvas.SetTop(picker1Pointer, y);

                    _h = 360 * x / picker1.ActualWidth;
                    _s = 1 - y / picker1.ActualHeight;
                    hslToRgb();
                }
            };

            // 明るさバーのドラッグ対応
            picker2.MouseLeftButtonDown += (s, e) =>
            {
                dragging = true;
                picker2.CaptureMouse();
            };
            picker2.MouseLeftButtonUp += (s,e) => {
                dragging = false;
                picker2.ReleaseMouseCapture();
            };
            picker2.MouseMove += (s,e) => {
                if (dragging) {
                    Point pt = e.GetPosition(picker2);
                    double y = pt.Y;
                    if (y < 0) y = 0;
                    if (y > picker2.ActualHeight) y = picker2.ActualHeight;
                    _l = 1 - y / picker2.ActualHeight;
                    Canvas.SetTop(picker2Pointer, y);

                    hslToRgb();
                }
            };

            colorR.ValueChanged += new RoutedPropertyChangedEventHandler<double>(rgb_ValueChanged);
            colorG.ValueChanged += new RoutedPropertyChangedEventHandler<double>(rgb_ValueChanged);
            colorB.ValueChanged += new RoutedPropertyChangedEventHandler<double>(rgb_ValueChanged);

            colorH.ValueChanged += new RoutedPropertyChangedEventHandler<double>(hsl_ValueChanged);
            colorS.ValueChanged += new RoutedPropertyChangedEventHandler<double>(hsl_ValueChanged);
            colorL.ValueChanged += new RoutedPropertyChangedEventHandler<double>(hsl_ValueChanged);
        }

        void rgb_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInSetting) 
            {
                _color = Color.FromArgb(0xff,
                    Convert.ToByte(colorR.Value),
                    Convert.ToByte(colorG.Value),
                    Convert.ToByte(colorB.Value));
                rgbToHsl();
            }
        }

        void hsl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInSetting)
            {
                _h = colorH.Value;
                _s = colorS.Value / 100;
                _l = colorL.Value / 100;
                GaeeyoColorPicker_SizeChanged(null, null);
                hslToRgb();
            }
        }


        void hslToRgb()
        {
            isInSetting = true;
            _color = Utils.HslToRgb(_h, _s, _l);
            updateHslValue();
            updateRgbValue();
            updatePreviewColor();
            isInSetting = false;
        }

        void rgbToHsl()
        {
            isInSetting = true;
            Utils.RgbToHsl(_color, out _h, out _s, out _l);
            updateHslValue();
            updatePreviewColor();
            GaeeyoColorPicker_SizeChanged(null, null);
            isInSetting = false;
        }

        void updatePreviewColor()
        {
            SolidColorBrush brush = colorPreview.Fill as SolidColorBrush;
            if (brush == null)
            {
                colorPreview.Fill = brush = new SolidColorBrush();
            }

            if (true /*_color != brush.Color*/)
            {
                brush.Color = _color;
                picker2Color.Color = Utils.HslToRgb(_h, _s, 0.5);
            }
        }

        void updateRgbValue()
        {
            colorR.Value = _color.R;
            colorG.Value = _color.G;
            colorB.Value = _color.B;
        }

        void updateHslValue()
        {
            colorH.Value = Convert.ToInt32(_h);
            colorS.Value = Convert.ToInt32(_s * 100);
            colorL.Value = Convert.ToInt32(_l * 100);
        }
    }
}
