using iText.Forms.Form.Element;
using MockInterviewAI.Model;
using MockInterviewAI.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MockInterviewAI.Service
{
    public class AiService
    {
        private static readonly string ApiKey = "AIzaSyALAScAimaiBrHWCtCuQ1n-G7vXEW-xgLo";
        //private static readonly string ApiKey = "AIzaSyCxATOqKYc7pAYO8Zx4gylesJ9487R0zdI";
        //private static readonly string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent?key=" + ApiKey;
        private static readonly string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=" + ApiKey;
        private static Dictionary<string, List<string>> keyValueStore = new Dictionary<string, List<string>>();


        public static async Task<Dictionary<string, List<string>>> ExtractCvDetailsAsJsonFromPdf(StorageFile file)
        {
            string pdfBase64 = await PdfParser.ConvertPdfToBase64(file); 

            using (HttpClient client = new HttpClient())
            {
                string text = $"Extract key details from the following PDF content and return as JSON:\n\n" +
                               "Format the response as JSON with the following fields:\n" +
                               "{\n" +
                               "  \"About\": \"Short summary about the candidate\",\n" +
                               "  \"Skills\": [\"Skill1\", \"Skill2\", \"Skill3\"],\n" +
                               "  \"Projects\": [{ \"Title\": \"Project Name\", \"Description\": \"Project details\" }],\n" +
                               "  \"Technologies\": [\"Technology1\", \"Technology2\"],\n" +
                               "  \"Experience\": [{ \"Project\": \"Project Name\", \"Role\": \"Job Role\", \"Platforms\": \"Platform Name\", \"Language\": \"Languages 1, Languages 2...\", \"Specific project works\": \"Responsibilities in Project and the specific project works in 2 lines maximum in 20 words\"}]\n" +
                               "}";

                string requestPayload = GetRequestBody(text, pdfBase64, "application/pdf");

                HttpContent content = new StringContent(requestPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(GeminiEndpoint, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();

                    dynamic result = JsonConvert.DeserializeObject(responseJson);

                    string responseArray = result?.candidates[0]?.content?.parts[0].text;
                    responseArray = responseArray.Replace("```json", "");
                    responseArray = responseArray.Replace("```", "");
                    ParseGeminiResponse(responseArray);

                    return keyValueStore;
                }
                else
                {
                    return null;
                }
            }
        }

        private static string GetRequestBody(string userInput, string base64Content, string type)
        {
            string json = "";

            if (string.IsNullOrEmpty(base64Content))
            {
                var requestBody = new
                {
                    contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = userInput }
                                }
                            }
                        }
                };

                json = JsonConvert.SerializeObject(requestBody);
            }
            else
            {
                var requestBody = new
                {
                    contents = new[]
                        {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = userInput },
                                new
                                {
                                    inlineData = new
                                    {
                                        mimeType = type,
                                        data = base64Content
                                    }
                                }
                            }
                        }
                    }

                };
                json = JsonConvert.SerializeObject(requestBody);
            }

            return json;
        }

        static void ExtractValues(JToken token, string parentKey = "")
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                {
                    string newKey = string.IsNullOrEmpty(parentKey) ? property.Name : $"{parentKey}";
                    ExtractValues(property.Value, newKey);
                }
            }
            else if (token is JArray array)
            {
                int index = 0;
                foreach (var item in array)
                {
                    string newKey = $"{parentKey}";
                    ExtractValues(item, newKey);
                    index++;
                }
            }
            else
            {
                // Store values in the dictionary
                if (!keyValueStore.ContainsKey(parentKey))
                    keyValueStore[parentKey] = new List<string>();

                keyValueStore[parentKey].Add(token.ToString());
            }
        }

        private static List<string> ParseGeminiResponse(string responseJson)
        {
            List<string> base64StringList = new List<string>();
            try
            {
                JObject response = JObject.Parse(responseJson);
                ExtractValues(response);
            }
            catch
            {
                return null;
            }
            return null;
        }

        public static void TraverseJson(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in jsonElement.EnumerateObject())
                {
                    Console.WriteLine($"Key: {property.Name}, Value: {property.Value}");

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
            else
            {
                Console.WriteLine($"Value: {jsonElement}");
            }
        }
    }
}
