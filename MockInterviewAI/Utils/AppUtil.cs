using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace MockInterviewAI.Utils
{
    public class AppUtil
    {
        public static async Task OpenFolder(string path)
        {
            StorageFile folder = await StorageFile.GetFileFromPathAsync(path);
            await Launcher.LaunchFileAsync(folder);
        }

        public static async Task<string> AudToBase64(StorageFile file)
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

        public static void TraverseJson(JsonElement jsonElement)
        {
            try
            {
                if (jsonElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in jsonElement.EnumerateObject())
                    {
                        TraverseJson(property.Value);
                    }
                }
                else if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in jsonElement.EnumerateArray())
                    {
                        TraverseJson(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in TraverseJson: {ex.Message}");
            }
        }
    }
}
