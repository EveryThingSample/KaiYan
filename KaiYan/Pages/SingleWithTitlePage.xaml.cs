using KaiYan.Core.Page;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KaiYan.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SingleWithTitlePage : Page
    {
        public SingleWithTitlePage()
        {
            this.InitializeComponent();
        }
        public string Title { get => (string)GetValue(TitleProperty); set { SetValue(TitleProperty, value); } }
        public static DependencyProperty TitleProperty { get; } = DependencyProperty.Register("Title", typeof(string), typeof(SingleWithTitlePage), new PropertyMetadata(""));

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is PageTool pageTool)
            {
                Title = pageTool.Name;
                frame.Navigate(typeof(ListPage), pageTool, new Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (frame != null)
                frame.Navigate(typeof(Page));
        }
    }
}
