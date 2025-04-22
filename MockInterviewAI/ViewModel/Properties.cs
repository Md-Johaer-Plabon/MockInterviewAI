using MockInterviewAI.Service;
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
        private bool isUpdatedExtraInfo { get; set; } = false;
        private string review;
        private string answer;
        private string _questionsLimit = "Max Questions";
        private string PrefLanguage { get; set; } = "English";
        private bool _isProgressRingActive = false;

        public string Answer
        {
            get => answer;
            set => SetProperty(ref answer, value);
        }

        private Dictionary<string, List<string>> cvText = new Dictionary<string, List<string>>();

        public ObservableCollection<string> ChatHistory { get; set; } = new ObservableCollection<string>();
        private TaskCompletionSource<string> waitingTaskCompletion { get; set; }
        public ObservableCollection<string> ComboBoxOptions { get; set; } = new ObservableCollection<string>
        {
            "1", "2", "3", "4", "5"
        };
        public ObservableCollection<string> LanguageOption { get; set; } = new ObservableCollection<string>
        {
            "English", "Bangla", "Spanish", "Arabic", "Korean"
        };

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

        public string QuestionLimit
        {
            get => _questionsLimit;
            set
            {
                if (_questionsLimit != value)
                {
                    _questionsLimit = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_questionsLimit)));
                }
            }
        }

        public string PrefLang
        {
            get => PrefLanguage;
            set
            {
                if (PrefLanguage != value)
                {
                    PrefLanguage = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrefLanguage)));
                }
            }
        }


        public bool IsProgressRingActive
        {
            get => _isProgressRingActive;
            set
            {
                if (_isProgressRingActive != value)
                {
                    _isProgressRingActive = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_isProgressRingActive)));
                }
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
