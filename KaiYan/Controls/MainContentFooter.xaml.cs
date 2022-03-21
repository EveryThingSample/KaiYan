using EveryThingSampleTools.WP.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace KaiYan.Controls
{
    [ContentProperty(Name = "Items")]
    public sealed partial class MainContentFooter : UserControl
    {

        public MainContentFooter()
        {
            Items = new ObservableCollection<object>();
            this.InitializeComponent();
            ItemsContainer_Grid.ColumnDefinitions.Clear();
            ItemsContainer_Grid.Children.Clear();
            sliderTranslateTransformXDoubleAnimation = new DoubleAnimation()
            {
                Duration = new TimeSpan(0, 0, 0, 0, 650),
                From = -100,
                To = -100,
                EasingFunction = new PowerEase()
                {
                    Power = 5,
                    EasingMode = EasingMode.EaseOut,

                }
            };
            Storyboard.SetTarget(sliderTranslateTransformXDoubleAnimation, sliderTranslateTransform);
            Storyboard.SetTargetProperty(sliderTranslateTransformXDoubleAnimation, "X");
            sliderWidthDoubleAnimation = new DoubleAnimation()
            {
                Duration = new TimeSpan(0, 0, 0, 0, 650),
                From = 0,
                To = 0,
                EasingFunction = new PowerEase()
                {
                    Power = 5,
                    EasingMode = EasingMode.EaseOut,

                },
                EnableDependentAnimation = true
            };
            Storyboard.SetTarget(sliderWidthDoubleAnimation, slider);
            Storyboard.SetTargetProperty(sliderWidthDoubleAnimation, "Width");

            storyboard.Children.Add(sliderTranslateTransformXDoubleAnimation);
            storyboard.Children.Add(sliderWidthDoubleAnimation);
            this.SizeChanged += MainContentFooter_SizeChanged;
            Root_Grid.SizeChanged += Root_Grid_SizeChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    ItemsContainer_Grid.ColumnDefinitions.Insert(e.NewStartingIndex, new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    ItemsContainer_Grid.Children.Insert(e.NewStartingIndex, CreateBorder(e.NewItems[0]));
                    ResetGridColumn();
                    changeSelectIndex(SelectedIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    ItemsContainer_Grid.ColumnDefinitions.RemoveAt(e.OldStartingIndex);
                    ItemsContainer_Grid.Children.RemoveAt(e.OldStartingIndex);
                    ResetGridColumn();
                    if (e.OldStartingIndex == SelectedIndex)
                    {
                        SelectedIndex = -1;
                    }
                    else
                    {
                        var i = ItemsContainer_Grid.Children.IndexOf(selectedElement);
                        if (i != SelectedIndex)
                        {
                            SelectedIndex = i;
                        }
                        else
                            changeSelectIndex(SelectedIndex);
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    ((ItemsContainer_Grid.Children[e.OldStartingIndex] as Border).Child as ContentPresenter).Content = e.NewItems[0];
                    changeSelectIndex(SelectedIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    ItemsContainer_Grid.Children.Clear();
                    SelectedIndex = -1;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    ItemsContainer_Grid.Children.Move((uint)e.OldStartingIndex, (uint)e.NewStartingIndex);

                    if (e.OldStartingIndex == SelectedIndex)
                    {
                        SelectedIndex = ItemsContainer_Grid.Children.IndexOf(selectedElement);
                    }

                    ResetGridColumn();
                    changeSelectIndex(SelectedIndex);
                    break;
            }
        }

        public ObservableCollection<object> Items { get; }
        private FrameworkElement selectedElement { get; set; }
        public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set { SetValue(SelectedIndexProperty, value); } }
        public static DependencyProperty SelectedIndexProperty { get; } = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(MainContentFooter), new PropertyMetadata(-1, new PropertyChangedCallback(SelectedIndexPropertyChanged)));
        private static void SelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.OldValue != (int)e.NewValue)
            {
                (d as MainContentFooter).selectedIndexPropertyChanged((int)e.NewValue, (int)e.OldValue);
            }

        }

        private Storyboard storyboard { get; } = new Storyboard();
        private DoubleAnimation sliderTranslateTransformXDoubleAnimation, sliderWidthDoubleAnimation;

        public event SelectionChangedEventHandler SelectionChanged;
        public event TypedEventHandler<MainContentFooter, int> DoubleSelected;


        private void Item_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var index = ItemsContainer_Grid.Children.IndexOf(sender as UIElement);
            if (SelectedIndex != index)
                SelectedIndex = index;
            else
            {
                DoubleSelected?.Invoke(this, index);
            }
        }
        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var index = ItemsContainer_Grid.Children.IndexOf(sender as UIElement);
            if (SelectedIndex != index)
                SelectedIndex = index;
        }
        private void Root_Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            changeSelectIndex(SelectedIndex);
        }
        private void MainContentFooter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Root_Grid.Width = e.NewSize.Width;
            Root_Grid.Height = e.NewSize.Height;

        }
        private void changeSelectIndex(int i)
        {
            if (i < 0 || i >= ItemsContainer_Grid.Children.Count)
            {
                sliderTranslateTransformAndScale(0, 0);
                selectedElement = null;
            }
            else
            {
                var element = ItemsContainer_Grid.Children[i] as FrameworkElement;
                selectedElement = element;
                Rect elementBounds = element.TransformToVisual(Root_Grid).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
                sliderTranslateTransformAndScale(elementBounds.Left, element.ActualWidth);
            }
        }

        private void sliderTranslateTransformAndScale(double value_transform, double value_width)
        {
            try
            {
                storyboard.Pause();

                {
                    var animation_x = sliderTranslateTransformXDoubleAnimation;
                    var from = (double)animation_x.From;
                    var to = (double)animation_x.To;
                    var total = animation_x.Duration.TimeSpan.TotalMilliseconds;
                    var curentTime = storyboard.GetCurrentTime().TotalMilliseconds;
                    var progressValue = curentTime / total;
                    animation_x.EasingFunction.Ease(progressValue);
                    animation_x.From = from + animation_x.EasingFunction.Ease(progressValue) * (to - from);
                    animation_x.To = value_transform;
                }
                {
                    var animation_scale = sliderWidthDoubleAnimation;
                    var from = (double)animation_scale.From;
                    var to = (double)animation_scale.To;
                    var total = animation_scale.Duration.TimeSpan.TotalMilliseconds;
                    var curentTime = storyboard.GetCurrentTime().TotalMilliseconds;
                    var progressValue = curentTime / total;
                    animation_scale.EasingFunction.Ease(progressValue);
                    animation_scale.From = from + animation_scale.EasingFunction.Ease(progressValue) * (to - from);
                    animation_scale.To = value_width;
                    slider.Width = (double)animation_scale.From;
                }



                storyboard.Stop();
                storyboard.Children.Clear();
                storyboard.Seek(TimeSpan.Zero);
                storyboard.Children.Add(sliderTranslateTransformXDoubleAnimation);
                storyboard.Children.Add(sliderWidthDoubleAnimation);
                storyboard.Begin();
            }
            catch
            { }
        }



        private void ResetGridColumn()
        {
            for (int i = 0; i < ItemsContainer_Grid.Children.Count; i++)
            {
                Grid.SetColumn((FrameworkElement)ItemsContainer_Grid.Children[i], i);
            }
        }
        private Border CreateBorder(object item)
        {
            var border = new Border()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidColorBrush(Colors.Transparent),
                Child = new ContentPresenter() { Content = item, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) },
            };
            border.Tapped += Item_Tapped;
            border.DoubleTapped += Item_DoubleTapped;
            return border;
        }



        private void selectedIndexPropertyChanged(int newValue, int oldValue)
        {
            changeSelectIndex(newValue);
            IList<object> oldItems = new List<object>();
            IList<object> newItems = new List<object>();
            if (oldValue >= 0 && oldValue < Items.Count)
            {
                oldItems.Add(Items[oldValue]);
            }
            if (newValue >= 0 && newValue < Items.Count)
            {
                newItems.Add(Items[newValue]);
            }
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(oldItems, newItems));
        }
    }
}
