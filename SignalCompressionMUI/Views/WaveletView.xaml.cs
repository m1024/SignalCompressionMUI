using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SignalCompressionMUI.ViewModels;

namespace SignalCompressionMUI.Views
{
    /// <summary>
    /// Interaction logic for WaveletView.xaml
    /// </summary>
    public partial class WaveletView : UserControl
    {
        public WaveletView()
        {
            InitializeComponent();
            DataContext = new WaveletViewModel();
        }
    }
}
