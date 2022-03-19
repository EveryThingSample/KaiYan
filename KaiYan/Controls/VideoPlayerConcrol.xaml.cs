using EveryThingSampleTools.WP.UI.Controls;
using KaiYan.Class;
using KaiYan.Core.Page.Card.Resource;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Media.Playback;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

#if DESKTOP
using System.Timers;
#else
using System.Threading;
#endif

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace KaiYan.Controls
{
    public sealed partial class VideoPlayerConcrol : UserControl
    {
        public VideoPlayerConcrol()
        {
            this.InitializeComponent();
            mediaPlayerElement.SetMediaPlayer(new Windows.Media.Playback.MediaPlayer() { AutoPlay = true });
            PlayInfos = new ObservableCollection<PlayInfo>();

            mediaPlayerElement.MediaPlayer.PlaybackSession.NaturalDurationChanged += PlaybackSession_NaturalDurationChanged;
            mediaPlayerElement.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;


            this.Loaded += VideoPlayerConcrol_Loaded; this.Unloaded += VideoPlayerConcrol_Unloaded;
        }
        public string CoverUrl { get => (string)GetValue(CoverUrlProperty); set { SetValue(CoverUrlProperty, value); } }
        public static DependencyProperty CoverUrlProperty { get; } = DependencyProperty.Register("CoverUrl", typeof(string), typeof(VideoPlayerConcrol), new PropertyMetadata(""));

        public ObservableCollection<PlayInfo> PlayInfos { get => (ObservableCollection<PlayInfo>)GetValue(PlayInfosProperty); set { SetValue(PlayInfosProperty, value); } }
        public static DependencyProperty PlayInfosProperty { get; } = DependencyProperty.Register("PlayInfos", typeof(double), typeof(VideoPlayerConcrol), new PropertyMetadata(new ObservableCollection<PlayInfo>()));

        public Visibility PlayInfoVisibility { get => (Visibility)GetValue(PlayInfoVisibilityProperty); set { SetValue(PlayInfoVisibilityProperty, value); } }
        public static DependencyProperty PlayInfoVisibilityProperty { get; } = DependencyProperty.Register("PlayInfoVisibility", typeof(double), typeof(VideoPlayerConcrol), new PropertyMetadata(Visibility.Collapsed));

        public PlayInfo SelectedPlayInfo { get => (PlayInfo)GetValue(SelectedPlayInfoProperty); set { SetValue(SelectedPlayInfoProperty, value); } }
        public static DependencyProperty SelectedPlayInfoProperty { get; } = DependencyProperty.Register("SelectedPlayInfo", typeof(PlayInfo), typeof(VideoPlayerConcrol), new PropertyMetadata(null));

        public bool IsFullWindow { get => (bool)GetValue(IsFullWindowProperty); set { SetValue(IsFullWindowProperty, value); } }
        public static DependencyProperty IsFullWindowProperty { get; } = DependencyProperty.Register("IsFullWindow", typeof(bool), typeof(VideoPlayerConcrol), new PropertyMetadata(false, IsFullWindowPropertyChenged));

        public event EventHandler<bool> IsFullWindowChanged;

        private static void IsFullWindowPropertyChenged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.OldValue != (bool)e.NewValue)
            {
                (d as VideoPlayerConcrol).changeIsFullScreenMode((bool)e.NewValue);
                (d as VideoPlayerConcrol).IsFullWindowChanged?.Invoke(d, (bool)e.NewValue);
            }
        }


        public MediaPlayerElement MediaPlayerElement => mediaPlayerElement;

        private void MediaControl_RootGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var RootGrid = sender as Grid;
            var BottomTransportContentControl = RootGrid.FindName("BottomTransportContentControl") as TransportContentControl;
            var position = e.GetPosition(RootGrid);
            if (BottomTransportContentControl.IsClosed || RootGrid.ActualHeight - position.Y > BottomTransportContentControl.ActualHeight)
            {
                if (!BottomTransportContentControl.IsOpened)
                    BottomTransportContentControl.Show();
                else if (!BottomTransportContentControl.IsClosed)
                    BottomTransportContentControl.Hide();
            }
        }
        private void FullWindowButton1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (sender as Control).IsEnabled = false;
                IsFullWindow = !IsFullWindow;
            }
            finally
            {
                (sender as Control).IsEnabled = true;
            }
        }
        private void changeIsFullScreenMode(bool isFullWindow)
        {

            if (FullWindowHelper.Current.IsFullWindow && isFullWindow == false)
            {
                FullWindowHelper.Current.ExitFullWindow();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
                mediaPlayerElement.MinHeight = 0;
                mediaPlayerElement.MaxHeight = Double.PositiveInfinity;
            }
            else if (isFullWindow)
            {
                FullWindowHelper.Current.EnterFullWindow();
                if (ApplicationView.GetForCurrentView().Orientation == ApplicationViewOrientation.Portrait)
                {
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                }
                var displayInformation = DisplayInformation.GetForCurrentView();
                var ScreenHeightInRawPixels = displayInformation.ScreenHeightInRawPixels;
                var ScreenWidthInRawPixels = displayInformation.ScreenWidthInRawPixels;
                var scale = displayInformation.RawPixelsPerViewPixel;
                mediaPlayerElement.MinHeight = Math.Min(ScreenWidthInRawPixels, ScreenHeightInRawPixels) / scale;
                mediaPlayerElement.MaxHeight = Math.Min(ScreenWidthInRawPixels, ScreenHeightInRawPixels) / scale;
            }
        }

        private void BottomTransportContentControl_Closing(FlyContent sender, ClosingEventArgs args)
        {
            if (MediaPlayerElement.MediaPlayer != null && MediaPlayerElement.MediaPlayer?.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
            {
                args.Cancel = true;
            }
        }
        Microsoft.UI.Xaml.Controls.ProgressBar progressBar = null; TransportContentControl progressBarContainer;
        //private void ProgressSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        //{
        //    if (progressBar == null)
        //    {
        //        progressBarContainer = (((sender as FrameworkElement).Parent as FrameworkElement).Parent as FrameworkElement).FindName("ProgressBarContainer") as TransportContentControl;
        //        progressBar = progressBarContainer.FindName("ProgressBar") as Microsoft.UI.Xaml.Controls.ProgressBar;
        //    }
        //    progressBar.Value = e.NewValue;
        //}

        private void BottomTransportContentControl_Closed(FlyContent sender, object args)
        {
            if (progressBar != null)
            {
                progressBarContainer.Show();
            }
        }

        private void BottomTransportContentControl_Opening(FlyContent sender, OpeningEventArgs args)
        {
            if (progressBarContainer != null)
            {
                progressBarContainer.Hide();
            }
        }

        private DisplayRequest appDisplayRequest = null;
        bool isDisplayRequestActive;
        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            if (sender.PlaybackState != MediaPlaybackState.Playing)
            {
#if DESKTOP
                timer.Stop();
#else
                timer.Change(0, System.Threading.Timeout.Infinite);
#endif
                
            }
            else
            {
#if DESKTOP
                timer.Start();
#else
                timer.Change(0, 1000);
#endif
            }
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (sender.PlaybackState != MediaPlaybackState.Playing)
                {
                    if (isDisplayRequestActive)
                    {
                        appDisplayRequest?.RequestRelease();
                        isDisplayRequestActive = false;
                    }
                }
                else
                {
                    if (isDisplayRequestActive == false)
                    {
                        appDisplayRequest?.RequestActive();
                        isDisplayRequestActive = true;
                    }
                }
            }).AsTask();
        }

        private void VideoPlayerConcrol_Unloaded(object sender, RoutedEventArgs e)
        {
#if DESKTOP
            timer.Stop();
            timer.Close();
#else
            timer.Change(0, System.Threading.Timeout.Infinite);
#endif

            timer = null;
            if (isDisplayRequestActive)
            {
                appDisplayRequest?.RequestRelease();
                isDisplayRequestActive = false;
            }
            appDisplayRequest = null;
        }

        private void VideoPlayerConcrol_Loaded(object sender, RoutedEventArgs e)
        {
#if DESKTOP
            timer = new Timer(1000) { Enabled = false };
            timer.Elapsed += timer_Elapsed;
            timer.AutoReset = true;
#else
            timer = new Timer(timer_callBack, null, 1, System.Threading.Timeout.Infinite);
#endif



           
            appDisplayRequest = new DisplayRequest();
            isDisplayRequestActive = false;
        }
        Run durationRun, timeElapsedRun;
        Timer timer;
#if DESKTOP
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var mediaPlayer = mediaPlayerElement.MediaPlayer;
                timeElapsedRun.Text = ((int)mediaPlayer.PlaybackSession.Position.TotalMinutes).ToString("D2") + ":" + mediaPlayer.PlaybackSession.Position.Seconds.ToString("D2");

                progressBar.Value = mediaPlayer.PlaybackSession.Position.TotalMilliseconds / mediaPlayer.PlaybackSession.NaturalDuration.TotalMilliseconds * 100;


            }).AsTask();
        }
#else
        private void timer_callBack (object state)
        {

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var mediaPlayer = mediaPlayerElement.MediaPlayer;
              
                timeElapsedRun.Text = ((int)mediaPlayer.PlaybackSession.Position.TotalMinutes).ToString("D2") + ":" + mediaPlayer.PlaybackSession.Position.Seconds.ToString("D2");
                if (mediaPlayer.PlaybackSession.NaturalDuration.TotalMilliseconds > 0)
                    progressBar.Value = mediaPlayer.PlaybackSession.Position.TotalMilliseconds / mediaPlayer.PlaybackSession.NaturalDuration.TotalMilliseconds * 100;


            }).AsTask();
        }
#endif


        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Loaded -= RootGrid_Loaded;
            var TimeElement = (sender as FrameworkElement).FindName("TimeElement") as TextBlock;
            timeElapsedRun = TimeElement.FindName("TimeElapsedRun") as Run;
            durationRun = TimeElement.FindName("DurationRun") as Run;
            progressBarContainer = (sender as FrameworkElement).FindName("ProgressBarContainer") as TransportContentControl;
            progressBar = progressBarContainer.FindName("ProgressBar") as Microsoft.UI.Xaml.Controls.ProgressBar;
        }

        private void PlaybackSession_NaturalDurationChanged(MediaPlaybackSession sender, object args)
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                durationRun.Text = ((int)sender.NaturalDuration.TotalMinutes).ToString("D2") + ":" + sender.NaturalDuration.Seconds.ToString("D2");

            }).AsTask();

        }
    }
}
