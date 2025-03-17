using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace MockInterviewAI.Service
{
    public class SpeechToTextService
    {
        public static MediaCapture mediaCapture;
        public static bool isInitialized;
        private static bool isRecording = false;
        public static StorageFile audioFile;

        public static async Task InitializeMediaCaptureAsync()
        {
            try
            {
                if (isInitialized) return;

                mediaCapture = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                };

                await mediaCapture.InitializeAsync(settings);

                isInitialized = true;
                Debug.WriteLine("✅ MediaCapture Initialized Successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ MediaCapture Initialization Error: {ex.Message}");
            }
        }

        public static async Task StartRecording()
        {
            try
            {
                if (isRecording) return;

                if (mediaCapture == null)
                {
                    await InitializeMediaCaptureAsync();
                }

                audioFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("recordedAudio.mp3", CreationCollisionOption.GenerateUniqueName);

                MediaEncodingProfile encodingProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);

                await mediaCapture.StartRecordToStorageFileAsync(encodingProfile, audioFile);
                isRecording = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ StartRecording Error: {ex.Message}");
            }
        }

        public static async Task StopRecording()
        {
            try
            {
                if (!isRecording) return;

                await mediaCapture.StopRecordAsync();
                isRecording = false;
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Error: {ex.Message}").ShowAsync();
            }
        }
    }
}
