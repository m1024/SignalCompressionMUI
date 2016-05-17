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
    /// Interaction logic for DCTView.xaml
    /// </summary>
    public partial class DCTView : UserControl
    {
        public DCTView()
        {
            InitializeComponent();
            DataContext = new DCTViewModel();
        }
    }
}
