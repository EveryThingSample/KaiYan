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
            //thirdColumnDefinition.Width = new GridLength((double)(Windows.Storage.ApplicationData.Current.LocalSettings.Values["thirdColumnDefinition.Width.Value"] ?? 1d), GridUnitType.Star);
            leftContentMask.Hide();
            rightContentMask.Hide();
        }

        public bool IsRightContentFilled { get; private set; }

        public object LeftContent { get => GetValue(LeftContentProperty); set { SetValue(LeftContentProperty, value); } }
        public static DependencyProperty LeftContentProperty { get; } = DependencyProperty.Register("LeftContent", typeof(object), typeof(SplitWindowControl), new PropertyMetadata(null));

        public object RightContent { get => GetValue(RightContentProperty); set { SetValue(RightContentProperty, value); } }
        public static DependencyProperty RightContentProperty { get; } = DependencyProperty.Register("RightContent", typeof(object), typeof(SplitWindowControl), new PropertyMetadata(null));

        public double SingleWindowMaxWidth { get => (double)GetValue(SingleWindowMaxWidthProperty); set { SetValue(SingleWindowMaxWidthProperty, value); } }
        public static DependencyProperty SingleWindowMaxWidthProperty { get; } = DependencyProperty.Register("SingleWindowMaxWidth", typeof(double), typeof(SplitWindowControl), new PropertyMetadata(800d, SingleWindowMaxWidthPropertyChangedCallback));
        private static void SingleWindowMaxWidthPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((double)e.NewValue != (double)e.OldValue)
            {
                (d as SplitWindowControl).thirdColumnDefinition.MinWidth = (double)e.NewValue / 2;
                (d as SplitWindowControl).firstColumnDefinition.MinWidth = (double)e.NewValue / 2;
            }
        }
        public SplitWindowMode CurrentSplitWindowMode => _splitWindowMode;
        public SplitWindowMode SplitWindowMode { get => (SplitWindowMode)GetValue(SplitWindowModeProperty); set { SetValue(SplitWindowModeProperty, value); } }
        public static DependencyProperty SplitWindowModeProperty { get; } = DependencyProperty.Register("SplitWindowMode", typeof(SplitWindowMode), typeof(SplitWindowControl), new PropertyMetadata(SplitWindowMode.Auto, SplitWindowModePropertyChangedCallback));

        private static void SplitWindowModePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((SplitWindowMode)e.OldValue != (SplitWindowMode)e.NewValue)
            {
                (d as SplitWindowControl).splitWindowModePropertyChangedCallback((SplitWindowMode)e.NewValue);
            }
        }
        /// <summary>
        /// Default is Default
        /// </summary>
        public SingleDisplayMode SingleDisplayMode { get => (SingleDisplayMode)GetValue(SingleDisplayModeProperty); set { SetValue(SingleDisplayModeProperty, value); } }
        public static DependencyProperty SingleDisplayModeProperty { get; } = DependencyProperty.Register("SingleDisplayMode", typeof(SingleDisplayMode), typeof(SplitWindowControl), new PropertyMetadata(SingleDisplayMode.Default, SingleDisplayModePropertyChangedCallback));

        private static void SingleDisplayModePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((SingleDisplayMode)e.OldValue != (SingleDisplayMode)e.NewValue)
            {
                (d as SplitWindowControl).singleDisplayModePropertyChangedCallback((SingleDisplayMode)e.NewValue);

            }
        }

        public double SplitRatio { get => (double)GetValue(SplitRatioProperty); set { SetValue(SplitRatioProperty, value); } }
        public static DependencyProperty SplitRatioProperty { get; } = DependencyProperty.Register("SplitRatio", typeof(double), typeof(SplitWindowControl), new PropertyMetadata(1d, SplitRatioPropertyChangedCallback));
        private static void SplitRatioPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((double)e.OldValue != (double)e.NewValue)
            {
                (d as SplitWindowControl).thirdColumnDefinition.Width = new GridLength((double)e.NewValue, GridUnitType.Star);
            }
        }

        public event EventHandler<object> ChangSplitRatioStared;
        public event EventHandler<object> ChangSplitRatioCompleted;
        public event EventHandler<SplitWindowMode> SplitWindowModeChanged;
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
            var thirdColumnWidth = _thirdColumnActualWidth - totalX;
            var firstColumnWidth = _firstColumnActualWidth + totalX;
            var minWid = SingleWindowMaxWidth / 2;
            if (firstColumnWidth < minWid)
            {
                thirdColumnWidth += firstColumnWidth - minWid;
                firstColumnWidth = minWid;

            }
            if (thirdColumnWidth < minWid)
            {
                firstColumnWidth += thirdColumnWidth - minWid;
                thirdColumnWidth = minWid;
            }
            var value = thirdColumnWidth / firstColumnWidth;
            if (value != thirdColumnDefinition.Width.Value)
                thirdColumnDefinition.Width = new GridLength(value, GridUnitType.Star);
            e.Handled = true;
        }
        double _firstColumnActualWidth, _thirdColumnActualWidth;
        bool isManipulationStarted;
        private void slider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            Grid grid = this.Content as Grid;
            _firstColumnActualWidth = firstColumnDefinition.ActualWidth;
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

            isManipulationStarted = false;
            leftContentMask.Hide();
            rightContentMask.Hide();
            SplitRatio = thirdColumnDefinition.Width.Value;
            LeftContentContainer.Opacity = 1;
            RightContentContainer.Opacity = 1;

            setContentSize();
            ChangSplitRatioCompleted?.Invoke(this, null);
        }
        private async void setLeftMaskImage()
        {
            leftContentImage.Opacity = 0;
            leftContentImage.Source = await EveryThingSampleTools.WP.Tools.SaveImageTools.GetImageSourceAsync(LeftContentContainer);

            LeftContentContainer.Opacity = 0;
            leftContentImage.Opacity = 1;
        }
        private async void setRightMaskImage()
        {
            rightContentImage.Opacity = 0;
            rightContentImage.Source = await EveryThingSampleTools.WP.Tools.SaveImageTools.GetImageSourceAsync(RightContentContainer);

            RightContentContainer.Opacity = 0;
            rightContentImage.Opacity = 1;
        }
        private void splitWindowModePropertyChangedCallback(SplitWindowMode newValue)
        {
            if (newValue == SplitWindowMode.Auto)
            {
                if (ActualWidth > SingleWindowMaxWidth)
                {
                    splitWindowMode = SplitWindowMode.Double;

                }
                else
                {
                    splitWindowMode = SplitWindowMode.Single;
                }
            }
            else
                splitWindowMode = newValue;
        }
        private void singleDisplayModePropertyChangedCallback(SingleDisplayMode newValue)
        {
            if (splitWindowMode == SplitWindowMode.Single)
            {
                if (newValue == SingleDisplayMode.Left)
                {
                    RightContentContainer.Visibility = Visibility.Collapsed;
                    LeftContentContainer.Visibility = Visibility.Visible;
                }
                else if (newValue == SingleDisplayMode.Right)
                {
                    RightContentContainer.Visibility = Visibility.Visible;
                    LeftContentContainer.Visibility = Visibility.Collapsed;
                }
                else if (newValue == SingleDisplayMode.Default)
                {
                    RightContentContainer.Visibility = Visibility.Visible;
                    LeftContentContainer.Visibility = Visibility.Visible;
                }
            }
        }
        private SplitWindowMode _splitWindowMode = SplitWindowMode.Double;
        private SplitWindowMode splitWindowMode
        {
            get => _splitWindowMode;
            set
            {
                if (_splitWindowMode != value)
                {
                    _splitWindowMode = value;
                    if (value == SplitWindowMode.Double)
                    {
                        Grid grid = this.Content as Grid;
                        if (grid.ColumnDefinitions.Count == 1)
                        {
                            grid.ColumnDefinitions.Add(secondColumnDefinition);
                            grid.ColumnDefinitions.Add(thirdColumnDefinition);
                            sliderContainer.Visibility = Visibility.Visible;
                        }
                        RightContentContainer.Visibility = Visibility.Visible;
                        LeftContentContainer.Visibility = Visibility.Visible;
                    }
                    else if (value == SplitWindowMode.Single)
                    {
                        Grid grid = this.Content as Grid;
                        if (grid.ColumnDefinitions.Count >= 3)
                        {
                            grid.ColumnDefinitions.RemoveAt(1);
                            grid.ColumnDefinitions.RemoveAt(1);
                            sliderContainer.Visibility = Visibility.Collapsed;
                        }
                        singleDisplayModePropertyChangedCallback(SingleDisplayMode);
                    }

                    SplitWindowModeChanged?.Invoke(this, splitWindowMode);
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (SplitWindowMode == SplitWindowMode.Auto)
            {
                if (e.NewSize.Width > SingleWindowMaxWidth)
                {
                    splitWindowMode = SplitWindowMode.Double;
                }
                else
                {
                    splitWindowMode = SplitWindowMode.Single;
                }
            }
            if (isManipulationStarted == false)
            {
                setContentSize();
            }
        }



        private void setContentSize()
        {
            if (splitWindowMode == SplitWindowMode.Double)
            {
                var allWid = this.ActualWidth - secondColumnDefinition.Width.Value;

                var leftWid = allWid * 1 / (SplitRatio + 1);
                var rightWid = allWid - leftWid;
                if (leftWid < SingleWindowMaxWidth / 2)
                {
                    leftWid = SingleWindowMaxWidth / 2;
                    rightWid = allWid - leftWid;
                }
                if (rightWid < SingleWindowMaxWidth / 2)
                {
                    rightWid = SingleWindowMaxWidth / 2;
                    leftWid = allWid - rightWid;
                }
                LeftContentContainer.Width = leftWid;
                RightContentContainer.Width = rightWid;
                //if (LeftContentContainer.Width != LeftContentBorder.ActualWidth)
                //    LeftContentContainer.Width = LeftContentBorder.ActualWidth;
                //if (RightContentContainer.Width != RightContentBorder.ActualWidth)
                //    RightContentContainer.Width = RightContentBorder.ActualWidth;
            }
            else if (splitWindowMode == SplitWindowMode.Single)
            {
                LeftContentContainer.Width = this.ActualWidth;
                RightContentContainer.Width = this.ActualWidth;
            }

        }
    }

    public enum SplitWindowMode
    {
        Auto,
        Single,
        Double,
    }
    public enum SingleDisplayMode
    {
        /// <summary>
        /// RightContent overlays LeftContent
        /// </summary>
        Default,
        Left,
        Right,
    }
}
