namespace SignalCompressionMUI.Models
{
    public static class OxyPlotModel
    {
        private static short[] _sequenceSourse;
        private static short[] _sequenceNew;

        public static short[] SequenceSourse
        {
            get { return _sequenceSourse; }
            set
            {
                _sequenceSourse = value;
                OnSequenceSourseChanged?.Invoke();
            }
        }

        public static short[] SequenceNew
        {
            get { return _sequenceNew; }
            set
            {
                _sequenceNew = value;
                OnSequenceNewChanged?.Invoke();
            }
        }

        public delegate void SequenceChanged();
        public static event SequenceChanged OnSequenceSourseChanged;
        public static event SequenceChanged OnSequenceNewChanged;
    }
}
