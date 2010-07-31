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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;

namespace GifImageLib
{
    public partial class GifImage : UserControl
    {
        private GifAnimation gifAnimation = null;
        public GifImage()
        {
            InitializeComponent();
            gifAnimation = new GifAnimation();
        }

        public Uri UriSource
        {
            set
            {
                WebClient myClient = new WebClient();
                myClient.OpenReadCompleted += new OpenReadCompletedEventHandler(myClient_OpenReadCompleted);
                myClient.OpenReadAsync(value);
            }
        }

        void myClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (frameTimer != null)
            {
                frameTimer.Stop();
            }
            SetSoruce(e.Result as Stream);
        }

        public void SetSoruce(Stream GifSoruce)
        {
            if (frameTimer != null)
            {
                frameTimer.Stop();
            }
            gifAnimation.Read(GifSoruce);
            RootImage.Width = gifAnimation.Width;
            RootImage.Height = gifAnimation.Height;
            LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            RunTimer();
        }

        void RunTimer()
        {
            frameTimer = new System.Windows.Threading.DispatcherTimer();
            frameTimer.Tick += NextFrame;
            frameTimer.Interval = new TimeSpan(0, 0, 0, 0, gifAnimation.frames[0].delay);
            numberOfFrames = gifAnimation.frames.Count;
            numberOfLoops = gifAnimation.loopCount;          
            frameTimer.Start();

        }

        private DispatcherTimer frameTimer = null;
        public void NextFrame()
        {
            NextFrame(null, null);
        }
        private int numberOfFrames = 0;
        private int frameCounter = 0;
        private int numberOfLoops = -1;
        private int currentLoop = 0;
        public void NextFrame(object sender, EventArgs e)
        {
            frameTimer.Stop();
            if (numberOfFrames == 0) return;

            frameCounter++;

            if (frameCounter < numberOfFrames)
            {
                RootImage.Source = gifAnimation.frames[frameCounter].image;
                frameTimer.Interval = new TimeSpan(0, 0, 0, 0, gifAnimation.frames[frameCounter].delay);
                frameTimer.Start();
            }
            else
            {
                if (numberOfLoops != 0)
                {
                    currentLoop++;
                }
                if (currentLoop < numberOfLoops || numberOfLoops == 0)
                {
                    frameCounter = 0;
                    RootImage.Source = gifAnimation.frames[frameCounter].image;
                    frameTimer.Interval = new TimeSpan(0, 0, 0, 0, gifAnimation.frames[frameCounter].delay);
                    frameTimer.Start();
                }
            }
        }
    }
}
