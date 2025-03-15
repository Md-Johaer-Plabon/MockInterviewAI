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
using Org.BouncyCastle.Asn1.Crmf;
using Windows.UI.Xaml.Controls;
using Windows.System;
using Windows.UI.Xaml.Input;
using System.Diagnostics;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml;
using Windows.Storage.Streams;

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
        public ICommand SubmitCommand { get; }
        public ICommand SaveFeedbackCommand { get; }
        public ICommand StartRecordCommand { get; }
        public ICommand StopRecordCommand { get; }

        public ObservableCollection<string> ChatHistory { get; set; } = new ObservableCollection<string>();

        private TaskCompletionSource<string> waitingTaskCompletion { get; set; }
        public string CvFileName
        {
            get => _cvFileName;
            set { _cvFileName = value; OnPropertyChanged(); }
        }

        private string voiceAns;
        private bool isUpdatedExtraInfo { get; set; } = false;
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

        private MediaCapture mediaCapture;
        private bool isRecording = false;
        private StorageFile audioFile;
        private bool isInitialized;

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
            StopRecordCommand = new RelayCommand(async () => await StopRecording());
            StartRecordCommand = new RelayCommand(async () => await StartRecording());
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

                string listVal = "";

                foreach (string lst in list)
                {
                    listVal += lst;
                }

                if (val == "About")
                {
                    data.About = listVal;
                }
                else if (val == "Skills")
                {
                    data.Skills = listVal;
                }
                else if (val == "Projects")
                {
                    data.Projects = listVal;
                }
                else if (val == "Technologies")
                {
                    data.Technologies = listVal;
                }
                else if (val == "Experience")
                {
                    data.Experience = listVal;
                }
            }

            if (!string.IsNullOrEmpty(ExtraInfo) && isUpdatedExtraInfo)
            {
                data.AdditionalInfo = ExtraInfo;
                isUpdatedExtraInfo = false;
            }

            await DbHelper.SaveUserData(data);
            cvText.Clear();
            cvText = null;
            ChatHistory.Clear();
        }

        private string Make(string topic, string text)
        {
            try
            {

            }
            catch
            {

            }

            return topic + text;
        }

        private async Task<string> PrepareText()
        {

            string text = "";
            try
            {
                UserData data = new UserData();
                data = await DbHelper.GetUserData(1);
                text += Make("About:\n", data.About);
                text += Make("Skills:\n", data.Skills);
                text += Make("Technologies:\n", data.Technologies);
                text += Make("Projects:\n", data.Projects);
                text += Make("Experiences:\n", data.Experience);
                text += Make("Additional Info:\n", data.AdditionalInfo);


            }
            catch
            {

            }

            return text;
        }

        private string review;

        public async Task StartInterview()
        {
            try
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
                string info = await PrepareText();

                List<string> questions = await AiService.GenerateQuestions(info);

                //.Click += async (sender, e) => await StartLoop();
                int idx = 1;

                foreach (var question in questions)
                {
                    ChatHistory.Add("🤖 Bot: " + question);
                    review += "Question " + idx++ + ": " + question + "\n";

                    string userInput = await WaitForUserInput();
                    
                    //string response = await SpeechService.SpeechToText(); // User replies
                    ChatHistory.Add("🧑‍💼 You: " + voiceAns);
                    review += voiceAns;
                    review += "\n";
                    await Task.Delay(300);
                    ChatHistory.Add("Loading...");
                    await Task.Delay(2000);
                    ChatHistory.Remove("Loading...");

                }

                ChatHistory.Add("✅ Interview Ended. Click 'Generate Feedback Report'.");
            }
            catch
            {

            }
        }

        public void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
        }

        private async Task InitializeMediaCaptureAsync()
        {
            try
            {
                if (isInitialized) return;  // Prevent re-initialization

                mediaCapture = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                };

                await mediaCapture.InitializeAsync(settings);  // Initialize before using

                isInitialized = true;
                Debug.WriteLine("✅ MediaCapture Initialized Successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ MediaCapture Initialization Error: {ex.Message}");
                await new Windows.UI.Popups.MessageDialog($"MediaCapture Initialization Failed: {ex.Message}").ShowAsync();
            }

        }

        public async Task StartRecording()
        {
            try
            {
                if (isRecording) return;  // Prevent multiple recordings

                ChatHistory.Add("Recording...");
                // Ensure MediaCapture is initialized
                if (mediaCapture == null)
                {
                    await InitializeMediaCaptureAsync();
                }

                // Create a new file for recording
                //audioFile = await KnownFolders.MusicLibrary.CreateFileAsync("recordedAudio.mp3", CreationCollisionOption.GenerateUniqueName);
                audioFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("recordedAudio.mp3", CreationCollisionOption.GenerateUniqueName);

                // Set up encoding profile
                MediaEncodingProfile encodingProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);

                // Start recording
                await mediaCapture.StartRecordToStorageFileAsync(encodingProfile, audioFile);
                isRecording = true;
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Error: {ex.Message}").ShowAsync();
            }
        }
        public async Task StopRecording()
        {
            try
            {
                if (!isRecording) return;

                ChatHistory.Remove("Recording...");
                await mediaCapture.StopRecordAsync();
                isRecording = false;

                await new Windows.UI.Popups.MessageDialog($"Recording saved: {audioFile.Path}").ShowAsync();

                string propmpt = "Transcribe the exact voice to text only.";
                string base64 = await AudToBase64(audioFile);

                ChatHistory.Add("Loading...");
                voiceAns = await AiService.Transcribe(propmpt, base64, "audio/wav");

                ChatHistory.Remove("Loading...");
                if (waitingTaskCompletion != null && !waitingTaskCompletion.Task.IsCompleted)
                {
                    review += voiceAns;
                    //Answer = "";
                    review += "\n";
                    await Task.Delay(1); // Simulate async work
                    waitingTaskCompletion.SetResult("");
                    //waitingTaskCompletion = null; // Reset only after completion
                }
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Error: {ex.Message}").ShowAsync();
            }
            finally
            {

            }
        }
        private async Task<string> AudToBase64(StorageFile file)
        {
            if (file == null) return null;

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                using (DataReader reader = new DataReader(stream))
                {
                    byte[] bytes = new byte[stream.Size];
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(bytes);
                    return Convert.ToBase64String(bytes);
                }
            }
        }

        private Task<string> WaitForUserInput()
        {
            if (waitingTaskCompletion == null || waitingTaskCompletion.Task.IsCompleted)
            {
                waitingTaskCompletion = new TaskCompletionSource<string>();
            }

            return waitingTaskCompletion.Task;
        }

        // Save Feedback as PDF
        public async Task SaveFeedback()
        {
            ChatHistory.Clear();
            string feedbackText = string.Join("\n", ChatHistory);
            ChatHistory.Add("Loading Your Feedback...");
            string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "History.txt");
            File.WriteAllText(filePath, review);
            string val = await AiService.ReviewExam(review);

            //val = val.Replace("\n", "<br><br>");
            //val = val.Replace("\r", "<br><br>");
            //val = val.Replace("*", "");
            //val = val.Replace("\\", "");
            //val = val.Replace("//", "");


            filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Review.html");
            File.WriteAllText(filePath, val);

            ChatHistory.Clear();
            ChatHistory.Add("📄 Feedback saved as PDF!");
        }

        public async Task Submit()
        {

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

        private string answer;

        public string Answer
        {
            get => answer;
            set => SetProperty(ref answer, value);
        }
    }
}
