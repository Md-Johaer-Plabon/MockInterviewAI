using System.Windows.Input;

namespace MockInterviewAI.ViewModel
{
    public partial class InterviewViewModel
    {
        public ICommand UploadCVCommand { get; }
        public ICommand StartInterviewCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand SaveFeedbackCommand { get; }
        public ICommand StartRecordCommand { get; }
        public ICommand StopRecordCommand { get; }
        public ICommand ClearChatCommand { get; }

        public InterviewViewModel()
        {
            UploadCVCommand = new RelayCommand(async () => await UploadCV());
            StartInterviewCommand = new RelayCommand(async () => await StartInterview());
            SaveFeedbackCommand = new RelayCommand(async () => await SaveFeedback());
            StopRecordCommand = new RelayCommand(async () => await StopRecording());
            StartRecordCommand = new RelayCommand(async () => await StartRecording());
            ClearChatCommand = new RelayCommand(async () => await ClearChat());
        }
    }
}
