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
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace Unene
{
    public class ImageManager
    {
        class MyImage
        {
            ImageSource image;
            bool loaded = false;
            private event Action<ImageSource> setters;

            public MyImage(string url)
            {
                var wc = new WebClient();                wc.OpenReadCompleted += new OpenReadCompletedEventHandler(wc_OpenReadCompleted);
                wc.OpenReadAsync(new Uri(url));
            }

            void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
            {
                loaded = true;
                // 読み込み完了
                if (e.Error == null)
                {
                    //Debug.WriteLine("画像読み込み完了: {0}", url);
                    try
                    {
                        BitmapImage bmp = new BitmapImage();

                        bmp.SetSource(e.Result);
                        image = bmp;
                    }
                    catch (Exception )
                    {
                        e.Result.Seek(0, System.IO.SeekOrigin.Begin);
                        GifImageLib.GifAnimation ga = new GifImageLib.GifAnimation();
                        ga.Read(e.Result);
                        if (ga.frames.Count > 0)
                        {
                            image = ga.frames[0].image;
                        }
                    }
                    CallSetters();
                }
            }

            public void AddSetter(Action<ImageSource> newSetter)
            {
                if (loaded)
                {
                    newSetter(image);
                }
                else
                {
                    setters += newSetter;
                }
            }

            private void CallSetters()
            {
                setters(image);
                setters = null;
            }
        }

        private static Dictionary<string, MyImage> images = new Dictionary<string, MyImage>();

        public static void GetImage(string url, Action<ImageSource> setter)
        {
            MyImage myImage;
            if (!images.TryGetValue(url, out myImage))
            {
                Debug.WriteLine("画像キャッシュ無し: {0}", url);
                myImage = new MyImage(url);
                images.Add(url, myImage);
            }
            else
            {
                Debug.WriteLine("画像キャッシュ有り: {0}", url);
            }
            myImage.AddSetter(setter);
        }

    }
}
