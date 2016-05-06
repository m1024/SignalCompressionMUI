using System.Windows.Controls;
using SignalCompressionMUI.ViewModels;

namespace SignalCompressionMUI.Views
{
    /// <summary>
    /// Interaction logic for RDPView.xaml
    /// </summary>
    public partial class RDPView : UserControl
    {
        public RDPView()
        {
            InitializeComponent();
            DataContext = new RDPViewModel();
        }
    }
}
