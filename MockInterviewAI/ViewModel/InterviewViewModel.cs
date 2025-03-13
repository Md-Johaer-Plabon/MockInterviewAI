using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Core;
using System.IO;
using System.Windows.Input;
using MockInterviewAI.Model;
using MockInterviewAI.Data;
using MockInterviewAI.Service;
using Newtonsoft.Json;

namespace MockInterviewAI.ViewModel
{
    public class InterviewViewModel : INotifyPropertyChanged
    {
        private string cvFilePath = "";
        private string _cvFileName = "No file selected";
        private string _extraInfo = "";
        private Dictionary<string, List<string>> cvText = new Dictionary<string, List<string>>();

        public ICommand UploadCVCommand { get; }
        public ICommand StartInterviewCommand { get; }
        public ICommand SaveFeedbackCommand { get; }

        public ObservableCollection<string> ChatHistory { get; set; } = new ObservableCollection<string>();

        public string CvFileName
        {
            get => _cvFileName;
            set { _cvFileName = value; OnPropertyChanged(); }
        }
        private bool isUpdatedExtraInfo = false;
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public InterviewViewModel()
        {
            UploadCVCommand = new RelayCommand(async () => await UploadCV());
            StartInterviewCommand = new RelayCommand(async () => await StartInterview());
            SaveFeedbackCommand = new RelayCommand(async () => await SaveFeedback());
        }

        public async Task UploadCV()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".pdf");
            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                cvFilePath = file.Path;
                CvFileName = file.Name;
                ChatHistory.Add("Uploading...");

                cvText = await AiService.ExtractCvDetailsAsJsonFromPdf(file);

                ChatHistory.Clear();
                ChatHistory.Add("Upload Completed!");
            }
        }

        private async Task SaveData()
        {
            ChatHistory.Clear();
            ChatHistory.Add("Loading...");

            UserData data = new UserData();
            foreach (string val in cvText.Keys)
            {
                List<string> list = cvText[val];
                string obj = JsonConvert.SerializeObject(list);

                if (val == "About")
                {
                    data.About = obj;
                }
                else if (val == "Skills")
                {
                    data.Skills = obj;
                }
                else if (val == "Projects")
                {
                    data.Projects = obj;
                }
                else if (val == "Technologies")
                {
                    data.Technologies = obj;
                }
                else if (val == "Experience")
                {
                    data.Experience = obj;
                }
            }

            if (!string.IsNullOrEmpty(ExtraInfo) && isUpdatedExtraInfo)
            {
                data.AdditionalInfo = ExtraInfo;
            }

            await DbHelper.SaveUserData(data);
            cvText.Clear();
            cvText = null;
            ChatHistory.Clear();
        }

        public async Task StartInterview()
        {
            if (cvText != null)
            {
                await SaveData();
            }

            UserData userData = await DbHelper.GetUserData(1); // Fetch data for user 1

            if (userData == null)
            {
                ChatHistory.Add("⚠️ Please try again.");
                return;
            }

            ChatHistory.Add("🎤 Interview Started...");
            //string questions = await GeminiService.GetInterviewQuestions(userData.CvText, userData.AdditionalInfo);

            //foreach (var question in questions.Split('\n'))
            //{
            //    ChatHistory.Add("🤖 Bot: " + question);
            //    await SpeechService.TextToSpeech(question); // Bot speaks

            //    string response = await SpeechService.SpeechToText(); // User replies
            //    ChatHistory.Add("🧑‍💼 You: " + response);
            //}

            ChatHistory.Add("✅ Interview Ended. Click 'Generate Feedback Report'.");
        }

        // Save Feedback as PDF
        public async Task SaveFeedback()
        {
            //string feedbackText = string.Join("\n", ChatHistory);
            //string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "InterviewFeedback.pdf");
            //ReportGenerator.CreatePDF(feedbackText, filePath);
            //ChatHistory.Add("📄 Feedback saved as PDF!");
        }
    }
}
