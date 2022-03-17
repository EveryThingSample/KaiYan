using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace KaiYan.Class
{
    public class FullWindowHelper
    {
        ApplicationView applicationView;
        private FullWindowHelper()
        {
            applicationView = ApplicationView.GetForCurrentView();
        }
        public bool IsFullWindow => applicationView.IsFullScreenMode;
        public static FullWindowHelper Current { get; } = new FullWindowHelper();

        public event EventHandler<object> FullWindowExisted;
        public event EventHandler<object> FullWindowEntered;


        public void EnterFullWindow()
        {
            if (!IsFullWindow)
            {
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                FullWindowEntered?.Invoke(this, null);
            }
        }
        public void ExitFullWindow()
        {
            if (IsFullWindow)
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                FullWindowExisted?.Invoke(this, null);
            }
        }
    }
}
