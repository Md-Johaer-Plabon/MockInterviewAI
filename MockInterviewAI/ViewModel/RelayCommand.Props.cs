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
        public ICommand ResetAllCommand { get; }

        public InterviewViewModel()
        {
            UploadCVCommand = new RelayCommands(async () => await UploadCV());
            StartInterviewCommand = new RelayCommands(async () => await StartInterview());
            SaveFeedbackCommand = new RelayCommands(async () => await SaveFeedback());
            StopRecordCommand = new RelayCommands(async () => await StopRecording());
            StartRecordCommand = new RelayCommands(async () => await StartRecording());
            ClearChatCommand = new RelayCommands(async () => await ClearChat());
            ResetAllCommand = new RelayCommands(async () => await ResetAll());
        }
    }
}
