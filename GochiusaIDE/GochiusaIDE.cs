using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text.Editor;

namespace GochiusaIDE
{
    /// <summary>
    /// Adornment class that draws a square box in the top right hand corner of the viewport
    /// </summary>
    class GochiusaIDE
    {
        
        private Image _faceImage;
        private Image _coverImage;
        private Image _backgroundImage;
        private Image _buildImage;

        private Dictionary<string, BitmapImage> bitmapimages;
        

        private Random cRandom;
        private bool eyeClosed;
        private DispatcherTimer faceTimer;

        
        private bool buildDone;
        private bool building;
        private bool clean;
        private int imageCount;
        private DispatcherTimer buildTimer;

        private IWpfTextView _view;
        private IAdornmentLayer _adornmentLayer;
        private IAdornmentLayer _adornmentBackgroundLayer;
        private IAdornmentLayer _adornmentBuildLayer;

        /// <summary>
        /// Creates a square image and attaches an event handler to the layout changed event that
        /// adds the the square in the upper right-hand corner of the TextView via the adornment layer
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        public GochiusaIDE(IWpfTextView view)
        {
            _view = view;

            InitImages();

            eyeClosed = false;
            cRandom = new Random();

            building = false;
            buildDone = false;
            clean = false;

            //Grab a reference to the adornment layer that this adornment should be added to
            _adornmentLayer = view.GetAdornmentLayer("GochiusaIDE");
            _adornmentBackgroundLayer = view.GetAdornmentLayer("GochiusaIDE_Background");
            _adornmentBuildLayer = view.GetAdornmentLayer("GochiusaIDE_Build");

            _adornmentBackgroundLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _backgroundImage, null);

            faceTimer = new DispatcherTimer(DispatcherPriority.Normal);
            faceTimer.Interval = new TimeSpan(30000000);
            faceTimer.Tick += new EventHandler(faceTimer_Tick);
            faceTimer.Start();

            buildTimer = new DispatcherTimer(DispatcherPriority.Normal);
            buildTimer.Interval = new TimeSpan(5000000);
            buildTimer.Tick += new EventHandler(buildTimer_Tick);
            buildTimer.Start();

            _view.ViewportHeightChanged += delegate { this.onSizeChange(); };
            _view.ViewportWidthChanged += delegate { this.onSizeChange(); };
        }

        private void faceTimer_Tick(object sender, EventArgs e)
        {
            
            if (!eyeClosed)
            {
                _faceImage.Source = bitmapimages["close"];
                eyeClosed = true;
                faceTimer.Interval = new TimeSpan(700000);
                faceTimer.Start();
            }
            else
            {
                if (cRandom.Next(0, 10) < 8)
                {
                    _faceImage.Source = bitmapimages["open"];
                }
                else
                {
                    _faceImage.Source = bitmapimages["halfopen"];
                }
                eyeClosed = false;
                faceTimer.Interval = new TimeSpan(cRandom.Next(7000000, 100000000));
                faceTimer.Start();
            }
        }

        public void buildDoneAction(bool success)
        {
            if (!clean)
            {
                buildDone = true;
                buildTimer.Stop();
                if (success)
                {
                    _buildImage.Source = bitmapimages["buildSucceeded"];
                }
                else
                {
                    _buildImage.Source = bitmapimages["buildFailed"];
                }
                buildTimer.Interval = new TimeSpan(30000000);
                buildTimer.Start();
            }
        }

        public void buildBeginAction(bool cleanFrag)
        {
            building = true;
            buildDone = false;
            clean = cleanFrag;
            imageCount = 3;
            
        }

        private void buildTimer_Tick(object sender, EventArgs e)
        {
            if (building && !clean)
            {
                if (buildDone)
                {
                    _buildImage.Source = bitmapimages["dammy"];
                    building = false;
                }
                else
                {
                    imageCount = (imageCount == 3) ? 1 : imageCount + 1;
                    if (imageCount == 1)
                    {
                        _buildImage.Source = bitmapimages["building1"];
                    }
                    else if (imageCount == 2)
                    {
                        _buildImage.Source = bitmapimages["building2"];
                    }
                    else if (imageCount == 3)
                    {
                        _buildImage.Source = bitmapimages["building3"];
                    }
                }
            }
            buildTimer.Interval = new TimeSpan(5000000);
            buildTimer.Start();
        }

        public void onSizeChange()
        {
            //clear the adornment layer of previous adornments
            _adornmentLayer.RemoveAllAdornments(); 

            //Place the image in the top right hand corner of the Viewport
            double l = _view.ViewportRight - _faceImage.Width;
            double t = _view.ViewportBottom - _faceImage.Height;
            Canvas.SetLeft(_coverImage, l);
            Canvas.SetTop(_coverImage, t);
            Canvas.SetLeft(_faceImage, l);
            Canvas.SetTop(_faceImage, t);

            //add the image to the adornment layer and make it relative to the viewport
            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _coverImage, null);
            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _faceImage, null);

            
            _adornmentBuildLayer.RemoveAllAdornments();

            double bl = _view.ViewportRight - _buildImage.Width;
            double bt = _view.ViewportTop;
            Canvas.SetLeft(_buildImage, bl);
            Canvas.SetTop(_buildImage, bt);

            _adornmentBuildLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _buildImage, null);
            
        }

        private void InitImages()
        {
            bitmapimages = new Dictionary<string, BitmapImage>();

            _backgroundImage = new Image();
            BitmapImage bi_bg = new BitmapImage();
            bi_bg.BeginInit();
            //bi_bg.UriSource = new Uri(System.IO.Path.GetFullPath("bg.jpg"), UriKind.Absolute);
            bi_bg.UriSource = new Uri("/GochiusaIDE;component/bg.jpg", UriKind.Relative);
            bi_bg.EndInit();
            _backgroundImage.Stretch = Stretch.None;
            _backgroundImage.Opacity = 0.25;
            _backgroundImage.Source = bi_bg;

            if (Environment.GetEnvironmentVariable("GOCHIUSA_IDE_VER_SYARO", System.EnvironmentVariableTarget.User) == "true")  // ver sharo.
            {
                _coverImage = new Image();
                BitmapImage bi_c = new BitmapImage();
                bi_c.BeginInit();
                bi_c.UriSource = new Uri("/GochiusaIDE;component/cover_syaro400.png", UriKind.Relative);
                bi_c.EndInit();
                _coverImage.Stretch = Stretch.None;
                _coverImage.Opacity = 1.0;
                _coverImage.Width = 400;
                _coverImage.Height = 289;
                _coverImage.Source = bi_c;


                _faceImage = new Image();
                var open = new BitmapImage();
                open.BeginInit();
                open.UriSource = new Uri("/GochiusaIDE;component/open_syaro400.png", UriKind.Relative);
                open.EndInit();
                var halfopen = new BitmapImage();
                halfopen.BeginInit();
                halfopen.UriSource = new Uri("/GochiusaIDE;component/halfopen_syaro400.png", UriKind.Relative);
                halfopen.EndInit();
                var close = new BitmapImage();
                close.BeginInit();
                close.UriSource = new Uri("/GochiusaIDE;component/close_syaro400.png", UriKind.Relative);
                close.EndInit();
                bitmapimages.Add("open", open);
                bitmapimages.Add("halfopen", halfopen);
                bitmapimages.Add("close", close);

                _faceImage.Stretch = Stretch.None;
                _faceImage.Opacity = 0.4;
                _faceImage.Width = 400;
                _faceImage.Height = 289;
                _faceImage.Source = bitmapimages["open"];
            }
            else
            {
                _coverImage = new Image();
                BitmapImage bi_c = new BitmapImage();
                bi_c.BeginInit();
                bi_c.UriSource = new Uri("/GochiusaIDE;component/cover300.png", UriKind.Relative);
                bi_c.EndInit();
                _coverImage.Stretch = Stretch.None;
                _coverImage.Opacity = 1.0;
                _coverImage.Width = 300;
                _coverImage.Height = 270;
                _coverImage.Source = bi_c;


                _faceImage = new Image();
                var open = new BitmapImage();
                open.BeginInit();
                open.UriSource = new Uri("/GochiusaIDE;component/open300.png", UriKind.Relative);
                open.EndInit();
                var halfopen = new BitmapImage();
                halfopen.BeginInit();
                halfopen.UriSource = new Uri("/GochiusaIDE;component/halfopen300.png", UriKind.Relative);
                halfopen.EndInit();
                var close = new BitmapImage();
                close.BeginInit();
                close.UriSource = new Uri("/GochiusaIDE;component/close300.png", UriKind.Relative);
                close.EndInit();
                bitmapimages.Add("open", open);
                bitmapimages.Add("halfopen", halfopen);
                bitmapimages.Add("close", close);

                _faceImage.Stretch = Stretch.None;
                _faceImage.Opacity = 0.4;
                _faceImage.Width = 300;
                _faceImage.Height = 270;
                _faceImage.Source = bitmapimages["open"];
            }

            _buildImage = new Image();
            var build1 = new BitmapImage();
            build1.BeginInit();
            build1.UriSource = new Uri("/GochiusaIDE;component/building1.png", UriKind.Relative);
            build1.EndInit();
            var build2 = new BitmapImage();
            build2.BeginInit();
            build2.UriSource = new Uri("/GochiusaIDE;component/building2.png", UriKind.Relative);
            build2.EndInit();
            var build3 = new BitmapImage();
            build3.BeginInit();
            build3.UriSource = new Uri("/GochiusaIDE;component/building3.png", UriKind.Relative);
            build3.EndInit();
            var buildFailed = new BitmapImage();
            buildFailed.BeginInit();
            buildFailed.UriSource = new Uri("/GochiusaIDE;component/failed.png", UriKind.Relative);
            buildFailed.EndInit();
            var buildSucceeded = new BitmapImage();
            buildSucceeded.BeginInit();
            buildSucceeded.UriSource = new Uri("/GochiusaIDE;component/succeeded.png", UriKind.Relative);
            buildSucceeded.EndInit();
            bitmapimages.Add("building1", build1);
            bitmapimages.Add("building2", build2);
            bitmapimages.Add("building3", build3);
            bitmapimages.Add("buildFailed", buildFailed);
            bitmapimages.Add("buildSucceeded", buildSucceeded);
            bitmapimages.Add("dammy", new BitmapImage());

            _buildImage.Stretch = Stretch.None;
            _buildImage.Opacity = 1.0;
            _buildImage.Width = 480;
            _buildImage.Height = 104;
            _buildImage.Source = bitmapimages["dammy"];

        }
    }
}
