using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using OxyPlot;
using SignalCompressionMUI.Models;

namespace SignalCompressionMUI.ViewModels
{
    class OxyPlotSpectrumViewModel : INotifyPropertyChanged
    {
        private List<DataPoint> _spectrumSourse;
        private List<DataPoint> _spectrumNew;

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
            SpectrumSourse = OxyPlotSpectrumModel.SpectrumSourse.Select((t, i) => new DataPoint(i, t)).ToList();
        }

        private void GetNewSpectrum()
        {
            SpectrumNew = OxyPlotSpectrumModel.SpectrumNew.Select((t, i) => new DataPoint(i, t)).ToList();
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
