using MockInterviewAI.Data;
using MockInterviewAI.Model;
using MockInterviewAI.Service;
using MockInterviewAI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MockInterviewAI.ViewModel
{
    public partial class InterviewViewModel : INotifyPropertyChanged
    {
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
                //
            }

            return text;
        }

        public async Task StartInterview()
        {
            try
            {
                if (cvText != null)
                {
                    await SaveData();
                }

                UserData userData = await DbHelper.GetUserData(1);

                if (userData == null)
                {
                    ChatHistory.Add("⚠️ Please try again.");
                    return;
                }

                ChatHistory.Add("🎤 Interview Starting...");
                string info = await PrepareText();

                List<string> questions = await AiService.GenerateQuestions(info);

                int idx = 1;

                foreach (var question in questions)
                {
                    ChatHistory.RemoveAt(ChatHistory.Count - 1);
                    ChatHistory.Add("🤖 Bot: " + question);
                    review += "Question " + idx++ + ": " + question + "\n";

                    string userInput = await WaitForUserInput();
                    
                    ChatHistory.Add("🧑‍💼 You: " + voiceAns);
                    review += $"Answer: { voiceAns}";
                    review += "\n";
                    await Task.Delay(300);
                    ChatHistory.Add("Loading...");
                    await Task.Delay(1000);

                }

                ChatHistory.Remove("Loading...");
                ChatHistory.Add("✅ Interview Ended. Click 'Generate Feedback Report'.");
            }
            catch
            {

            }
        }

        public async Task StartRecording()
        {
            try
            {
                ChatHistory.Add("Recording...");
                await SpeechToTextService.StartRecording();
            }
            catch
            {

            }
        }

        public async Task StopRecording()
        {
            try
            {
                ChatHistory.Remove("Recording...");
                await SpeechToTextService.StopRecording();

                string propmpt = "Transcribe the exact voice to text only.";
                string base64 = await AppUtil.AudToBase64(SpeechToTextService.audioFile);

                ChatHistory.Add("Loading...");
                voiceAns = await AiService.Transcribe(propmpt, base64, "audio/wav");

                ChatHistory.Remove("Loading...");
                if (waitingTaskCompletion != null && !waitingTaskCompletion.Task.IsCompleted)
                {
                    waitingTaskCompletion.SetResult("");
                }
            }
            finally
            {
                FileInfo aud = new FileInfo(SpeechToTextService.audioFile.Path);
                if (aud.Exists)
                {
                    aud.Delete();
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

        public async Task SaveFeedback()
        {
            ChatHistory.Clear();
            ChatHistory.Add("Loading Your Feedback...");
            string filePath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "History.txt");
            File.WriteAllText(filePath, review);
            string val = await AiService.ReviewExam(review);

            filePath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "Review.html");
            File.WriteAllText(filePath, val);

            ChatHistory.Clear();
            ChatHistory.Add("📄 Feedback saved as PDF!");
            await AppUtil.OpenFolder(filePath);
        }

        private async Task ClearChat()
        {
            ChatHistory.Clear();
            await Task.Delay(10);
        }
    }
}
