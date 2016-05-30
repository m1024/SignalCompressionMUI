using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using OxyPlot;
using SignalCompressionMUI.Models;

namespace SignalCompressionMUI.ViewModels
{
    class OxyPlotSpectrumViewModel : INotifyPropertyChanged
    {
        private List<DataPoint> _spectrumSourse;
        private List<DataPoint> _spectrumNew;
        private Color _sourseColor = Color.FromRgb(0x1b, 0xa1, 0xe2);

        public Color SourseColor
        {
            get { return _sourseColor; }
            set
            {
                _sourseColor = value;
                OnPropertyChanged("SourseColor");
            }
        }

        public List<DataPoint> SpectrumSourse
        {
            get { return _spectrumSourse; }
            set
            {
                _spectrumSourse = value;
                OnPropertyChanged("SpectrumSourse");
            }
        }

        public List<DataPoint> SpectrumNew
        {
            get { return _spectrumNew; }
            set
            {
                _spectrumNew = value;
                OnPropertyChanged("SpectrumNew");
            }
        }

        public OxyPlotSpectrumViewModel()
        {
            OxyPlotSpectrumModel.OnSpectrumSourseChanged += GetSourseSpectrum;
            OxyPlotSpectrumModel.OnSpectrumNewChanged += GetNewSpectrum;

            if (OxyPlotSpectrumModel.SpectrumSourse != null) GetSourseSpectrum();
            if (OxyPlotSpectrumModel.SpectrumNew != null) GetNewSpectrum();
        }

        private void GetSourseSpectrum()
        {
            var k = 5000f / OxyPlotSpectrumModel.SpectrumSourse.Length;
            SpectrumSourse = OxyPlotSpectrumModel.SpectrumSourse.Select((t, i) => new DataPoint(i*k, t)).ToList();
        }

        private void GetNewSpectrum()
        {
            var k = 5000f / OxyPlotSpectrumModel.SpectrumNew.Length;
            SpectrumNew = OxyPlotSpectrumModel.SpectrumNew.Select((t, i) => new DataPoint(i*k, t)).ToList();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
