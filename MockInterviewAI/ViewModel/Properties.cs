using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MockInterviewAI.ViewModel
{
    public partial class InterviewViewModel
    {
        private string cvFilePath = "";
        private string _cvFileName = "No file selected";
        private string _extraInfo = "";
        private string voiceAns;
        private bool isUpdatedExtraInfo = false;
        private string review;
        private string answer;
        public string Answer
        {
            get => answer;
            set => SetProperty(ref answer, value);
        }

        private Dictionary<string, List<string>> cvText = new Dictionary<string, List<string>>();

        public ObservableCollection<string> ChatHistory { get; set; } = new ObservableCollection<string>();
        private TaskCompletionSource<string> waitingTaskCompletion { get; set; }

        public string CvFileName
        {
            get => _cvFileName;
            set { _cvFileName = value; OnPropertyChanged(); }
        }

        public string ExtraInfo
        {
            get => _extraInfo;
            set
            {
                isUpdatedExtraInfo = true;
                _extraInfo = value;
                OnPropertyChanged();
            }
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
