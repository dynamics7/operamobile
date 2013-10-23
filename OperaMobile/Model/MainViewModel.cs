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

namespace OperaMobile
{
    public class MainViewModel : BaseViewModel
    {

        private bool _isRunning = false;
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                _isRunning = value;
                OnChange("IsRunning");
                OnChange("RunStopText");
                OnChange("IsStoppedVisibility");
                OnChange("IsRunningVisibility");
            }
        }

        public string RunStopText
        {
            get
            {
                if (_isRunning)
                    return LocalizedResources.Stop;
                else
                    return LocalizedResources.Run;
            }
        }

        public Visibility IsStoppedVisibility
        {
            get
            {
                if (_isRunning == false)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility IsRunningVisibility
        {
            get
            {
                if (_isRunning)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

    }
}
