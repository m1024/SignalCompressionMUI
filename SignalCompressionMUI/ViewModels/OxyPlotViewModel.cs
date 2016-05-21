using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalCompressionMUI.Models;
using OxyPlot;

namespace SignalCompressionMUI.ViewModels
{
    class OxyPlotViewModel : INotifyPropertyChanged
    {
        private List<DataPoint> _curveSourse;
        private List<DataPoint> _curveNew; 

        public List<DataPoint> CurveSourse
        {
            get { return _curveSourse; }
            set
            {
                _curveSourse = value;
                OnPropertyChanged("CurveSourse");
            }
        }

        public List<DataPoint> CurveNew
        {
            get { return _curveNew; }
            set
            {
                _curveNew = value;
                OnPropertyChanged("CurveNew");
            }
        } 

        public OxyPlotViewModel()
        {
            OxyPlotModel.OnSequenceSourseChanged += GetSourseSequence;
            OxyPlotModel.OnSequenceNewChanged += GetNewSequence;

            if (OxyPlotModel.SequenceSourse != null) GetSourseSequence();
            if (OxyPlotModel.SequenceNew != null) GetNewSequence();
        }

        private void GetSourseSequence()
        {
            CurveSourse = OxyPlotModel.SequenceSourse.Select((t, i) => new DataPoint(i, t)).ToList();
        }

        private void GetNewSequence()
        {
            CurveNew = OxyPlotModel.SequenceNew.Select((t, i) => new DataPoint(i, t)).ToList();
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
