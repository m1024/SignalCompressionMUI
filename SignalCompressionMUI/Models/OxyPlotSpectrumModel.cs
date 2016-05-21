using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalCompressionMUI.Models
{
    class OxyPlotSpectrumModel
    {
        private static int[] _spectrumSourse;
        private static int[] _spectrumNew;

        public static int[] SpectrumSourse
        {
            get { return _spectrumSourse; }
            set
            {
                _spectrumSourse = value;
                OnSpectrumSourseChanged?.Invoke();
            }
        }

        public static int[] SpectrumNew
        {
            get { return _spectrumNew; }
            set
            {
                _spectrumNew = value;
                OnSpectrumNewChanged?.Invoke();
            }
        }

        public delegate void SpectrumChanged();
        public static event SpectrumChanged OnSpectrumSourseChanged;
        public static event SpectrumChanged OnSpectrumNewChanged;
    }
}
