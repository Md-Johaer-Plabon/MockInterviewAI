using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MockInterviewAI.Utils
{
    public class PdfParser
    {
        public static async Task<string> ConvertPdfToBase64(StorageFile file)
        {
            try
            {
                IRandomAccessStream fileStream = await file.OpenReadAsync();

                byte[] bytes = new byte[fileStream.Size];
                DataReader dataReader = new DataReader(fileStream);
                await dataReader.LoadAsync((uint)fileStream.Size);
                dataReader.ReadBytes(bytes);

                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                return $"Error converting PDF to Base64: {ex.Message}";
            }
        }
    }
}
