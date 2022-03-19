using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace KaiYan.Controls
{
    public sealed partial class SplitWindowControl : UserControl
    {
        public SplitWindowControl()
        {
            this.InitializeComponent();
            thirdColumnDefinition.Width = new GridLength((double)(Windows.Storage.ApplicationData.Current.LocalSettings.Values["thirdColumnDefinition.Width.Value"] ?? 1d), GridUnitType.Star);
            leftContentMask.Hide();
            rightContentMask.Hide();
        }

        public bool IsRightContentFilled { get; private set; }

        public object LeftContent { get => GetValue(LeftContentProperty); set { SetValue(LeftContentProperty, value); } }
        public static DependencyProperty LeftContentProperty { get; } = DependencyProperty.Register("LeftContent", typeof(object), typeof(SplitWindowControl), new PropertyMetadata(null));

        public object RightContent { get => GetValue(RightContentProperty); set { SetValue(RightContentProperty, value); } }
        public static DependencyProperty RightContentProperty { get; } = DependencyProperty.Register("RightContent", typeof(object), typeof(SplitWindowControl), new PropertyMetadata(null));

        public event EventHandler<object> ChangSplitRatioStared;
        public event EventHandler<object> ChangSplitRatioCompleted;
        public void FillRightContent()
        {
            if (IsRightContentFilled == false)
            {
                Grid.SetColumnSpan(RightContentBorder, 3);
                Grid.SetColumn(RightContentBorder, 0);
                IsRightContentFilled = true;
            }
        }
        public void UnfillRightContent()
        {
            if (IsRightContentFilled)
            {
                Grid.SetColumnSpan(RightContentBorder, 1);
                Grid.SetColumn(RightContentBorder, 2);
                IsRightContentFilled = false;
            }
        }

        private void slider_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var totalX = e.Cumulative.Translation.X;


            //var firstColumnWidth = grid.ColumnDefinitions.First().ActualWidth;
            //var thirdColumnWidth = thirdColumnDefinition.ActualWidth;
            var thirdColumnWidth = _thirdColumnActualWidth - totalX;
            var firstColumnWidth = _firstColumnActualWidth + totalX;
            thirdColumnDefinition.Width = new GridLength(thirdColumnWidth / firstColumnWidth, GridUnitType.Star);
            e.Handled = true;
        }
        double _firstColumnActualWidth, _thirdColumnActualWidth;
        bool isManipulationStarted;
        private void slider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            Grid grid = this.Content as Grid;
            _firstColumnActualWidth = grid.ColumnDefinitions.First().ActualWidth;
            _thirdColumnActualWidth = thirdColumnDefinition.ActualWidth;
            e.Handled = true;
            isManipulationStarted = true;
            leftContentMask.Show();
            rightContentMask.Show();

            setLeftMaskImage();
            setRightMaskImage();
            ChangSplitRatioStared?.Invoke(this, null);
        }

        private void slider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values["thirdColumnDefinition.Width.Value"] = thirdColumnDefinition.Width.Value;
            setContentSize();
            isManipulationStarted = false;
            leftContentMask.Hide();
            rightContentMask.Hide();
            ChangSplitRatioCompleted?.Invoke(this, null);
        }
        private async void setLeftMaskImage()
        {
            leftContentImage.Opacity = 0;
            leftContentImage.Source = await EveryThingSampleTools.WP.Tools.SaveImageTools.GetImageSourceAsync(LeftContentContainer);
            leftContentImage.Opacity = 1;
        }
        private async void setRightMaskImage()
        {
            rightContentImage.Opacity = 0;
            rightContentImage.Source = await EveryThingSampleTools.WP.Tools.SaveImageTools.GetImageSourceAsync(RightContentContainer);
            rightContentImage.Opacity = 1;
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 800)
            {
                Grid grid = this.Content as Grid;
                if (grid.ColumnDefinitions.Count == 1)
                {
                    grid.ColumnDefinitions.Add(secondColumnDefinition);
                    grid.ColumnDefinitions.Add(thirdColumnDefinition);
                    sliderContainer.Visibility = Visibility.Visible;

                }
            }
            else
            {
                Grid grid = this.Content as Grid;
                if (grid.ColumnDefinitions.Count >= 3)
                {
                    grid.ColumnDefinitions.RemoveAt(1);
                    grid.ColumnDefinitions.RemoveAt(1);
                    sliderContainer.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void RightContentBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isManipulationStarted == false && e.NewSize.Width != e.PreviousSize.Width)
            {
                setContentSize();
            }
        }
        private void setContentSize()
        {
            if (LeftContentContainer.Width != LeftContentBorder.ActualWidth)
                LeftContentContainer.Width = LeftContentBorder.ActualWidth;
            if (RightContentContainer.Width != RightContentBorder.ActualWidth)
                RightContentContainer.Width = RightContentBorder.ActualWidth;
        }
    }
}
