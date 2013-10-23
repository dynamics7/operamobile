using System.Windows;
using Microsoft.Phone.Controls;

namespace OperaMobile
{
    public partial class pageOptions : PhoneApplicationPage
    {
        public pageOptions()
        {
            InitializeComponent();
        }

        private void btnTaskDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(LocalizedResources.FolderDeletion, LocalizedResources.Warning, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                MainPage.Cleanup();
                MainPage.Opera_StuffCopied = false;
            }
        }


    }
}
