using MockInterviewAI.Data;
using MockInterviewAI.Model;
using MockInterviewAI.Service;
using MockInterviewAI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MockInterviewAI.ViewModel
{
    public partial class InterviewViewModel : INotifyPropertyChanged
    {
        private List<string> questions { get; set; }
        private bool _forceEnd { get; set; } = false;
        private bool _isCvTextReady = false;
        private bool _isCvUploadOngoing = false;

        public async Task UploadCV()
        {
            try
            {
                FileOpenPicker picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".pdf");
                StorageFile file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    cvFilePath = file.Path;
                    CvFileName = file.Name;
                    ChatHistory.Add("Uploading...");
                    _isCvUploadOngoing = true;

                    cvText = await AiService.ExtractCvDetailsAsJsonFromPdf(file);

                    if (cvText != null && cvText.Count > 0)
                    {
                        _isCvTextReady = true;
                    }

                    ChatHistory?.Clear();
                    ChatHistory.Add("Upload Completed!");

                    await SaveData();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in Uploading CV: " + ex.Message);
            }
            finally
            {
                _isCvUploadOngoing = false;
            }
        }

        private async Task SaveData()
        {
            try
            {
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
                    else
                    {
                        data.Others = listVal;
                    }
                }

                if (!string.IsNullOrEmpty(ExtraInfo) && isUpdatedExtraInfo)
                {
                    data.AdditionalInfo = ExtraInfo;
                    isUpdatedExtraInfo = false;
                }

                await DbHelper.SaveUserData(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in SaveData: " + ex.Message);
            }
            finally
            {
                cvText?.Clear();
            }
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
                data = await DbHelper.GetUserData();

                text += Make("About: ", data.About);
                text += Make("\nSkills: ", data.Skills);
                text += Make("\nTechnologies: ", data.Technologies);
                text += Make("\nProjects: ", data.Projects);
                text += Make("\nExperiences: ", data.Experience);
                text += Make("\nAdditional Info: ", data.AdditionalInfo);
                text += Make("\nOthers Info: ", data.Others);
                text += "\n";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in PrepareText: " + ex.Message);
            }

            return text;
        }

        public async Task StartInterview()
        {
            try
            {
                if (_isCvUploadOngoing && !_isCvTextReady)
                {
                    ChatHistory.Add("⚠️ CV is not uploaded yet! Please wait...");
                    return;
                }

                ChatHistory.Clear();
                if (waitingTaskCompletion != null && !waitingTaskCompletion.Task.IsCompleted)
                {
                    ChatHistory.Add("⚠️ Illegal Move! Plese answer the question.");
                    return;
                }

                UserData userData = await DbHelper.GetUserData();

                if (userData == null)
                {
                    if (!string.IsNullOrEmpty(ExtraInfo))
                    {
                        userData = new UserData();
                        userData.AdditionalInfo = ExtraInfo;
                        isUpdatedExtraInfo = false;
                        await DbHelper.SaveUserData(userData);
                    }
                    else
                    {
                        ChatHistory.Add("⚠️ No records found. Please try again.");
                        return;
                    }
                }

                if (isUpdatedExtraInfo)
                {
                    userData.AdditionalInfo = ExtraInfo;
                    await DbHelper.SaveUserData(userData);
                }

                ChatHistory.Clear();
                ChatHistory.Add("🎤 Interview Starting...");
                string info = await PrepareText();
                string limit = "";

                if(QuestionLimit == "Max Questions")
                {
                    limit = "2";
                }
                else
                {
                    limit = QuestionLimit;
                }

                questions = await AiService.GenerateQuestions(info, limit, PrefLang);
                ChatHistory.Clear();

                int idx = 1;

                if (questions?.Count == 0)
                {
                    ChatHistory?.Clear();
                    ChatHistory?.Add("🙁 No question found!");
                }

                foreach (var question in questions)
                {
                    if (ChatHistory?.Count - 1 >= 0)
                    {
                        ChatHistory?.RemoveAt(ChatHistory.Count - 1);
                    }

                    ChatHistory?.Add("🤖 Bot: " + question);
                    review += "Question " + idx++ + ": " + question + "\n\n";

                    string userInput = await WaitForUserInput();
                    
                    if (_forceEnd)
                    {
                        _forceEnd = false;
                        break;
                    }

                    ChatHistory.Add("🧑‍💼 You: " + voiceAns);
                    review += $"Answer: { voiceAns}";
                    review += "\n\n";
                    await Task.Delay(300);
                    ChatHistory.Add("Loading...");
                    await Task.Delay(1000);

                }

                ChatHistory.Remove("Loading...");
                ChatHistory.Add("✅ Interview Ended. " + (!_forceEnd? "Click 'Generate Feedback Report'.": ""));
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Error in StartInterview: {ex.Message}").ShowAsync();
            }

            questions?.Clear();
        }

        public async Task StartRecording()
        {
            try
            {
                if (questions == null || questions?.Count == 0)
                {
                    ChatHistory.Add("⚠️ Please confirm the interview has been started properly.");
                    return;
                }

                ChatHistory.Remove("⚠️ Start the recording first.");
                ChatHistory.Add("Recording...");
                await SpeechToTextService.StartRecording();
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Error in StartRecording: {ex.Message}").ShowAsync();
            }
        }

        public async Task StopRecording()
        {
            try
            {
                if (!ChatHistory.Contains("Recording..."))
                {
                    ChatHistory.Add("⚠️ Start the recording first.");
                    return;
                }

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
                if (SpeechToTextService.audioFile != null)
                {
                    FileInfo aud = new FileInfo(SpeechToTextService.audioFile.Path);
                    if (aud.Exists)
                    {
                        aud.Delete();
                    }
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
            try
            {
                if (string.IsNullOrEmpty(review))
                {
                    ChatHistory.Clear();
                    ChatHistory.Add("⚠️ No Data Found.");
                    return;
                }

                ChatHistory.Clear();
                ChatHistory.Add("Loading Your Feedback...");

                IsProgressRingActive = true;

                string filePath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "History.txt");
                File.WriteAllText(filePath, review);
                string val = await AiService.ReviewExam(review, PrefLang);

                filePath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "Review.html");
                File.WriteAllText(filePath, val);

                ChatHistory.Clear();
                ChatHistory.Add("📄 Feedback saved as PDF!");
                await AppUtil.OpenFolder(filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in SaveFeedBack: " + ex);
            }
            finally
            {
                IsProgressRingActive = false;
                review = "";
            }
        }

        private async Task ClearChat()
        {
            ChatHistory.Clear();
            review = "";
            await Task.Delay(10);
        }

        private async Task ResetAll()
        {
            await ClearChat();
            await DbHelper.DeleteEntity();
            QuestionLimit = "Max Questions";
            CvFileName = "";
            ExtraInfo = "";
            review = "";
            _isCvTextReady = false;

            AiService.ClearAiServiceProps();

            if (waitingTaskCompletion != null && !waitingTaskCompletion.Task.IsCompleted)
            {
                _forceEnd = true;
                waitingTaskCompletion.SetResult("");
            }
        }
    }
}
