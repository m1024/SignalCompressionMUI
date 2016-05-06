using System.Windows;
using SignalCompressionMUI.ViewModels;
using SignalCompressionMUI.Views;

namespace SignalCompressionMUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var mw = new MainWindowView
            {
                DataContext = new MainWindowViewModel()
            };

            mw.Show();
        }
    }
}
