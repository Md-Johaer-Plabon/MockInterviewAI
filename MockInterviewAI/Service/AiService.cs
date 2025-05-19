using MockInterviewAI.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MockInterviewAI.Service
{
    public class AiService
    {
        private static readonly string ApiKey = "AIzaSyALAScAimaiBrHWCtCuQ1n-G7vXEW-xgLo";
        private static readonly string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=" + ApiKey;
        private static Dictionary<string, List<string>> keyValueStore = new Dictionary<string, List<string>>();
        private static List<string> Questions { set; get; } = new List<string>();
        private static bool isAudioList;
        private static List<string> AudQuestions;
        private static bool isQuestionText { get; set; } = false;

        public static async Task<Dictionary<string, List<string>>> ExtractCvDetailsAsJsonFromPdf(string file)
        {
            try
            {
                //string pdfBase64 = await PdfParser.ConvertPdfToBase64(file);

                keyValueStore?.Clear();
                using (HttpClient client = new HttpClient())
                {
                    string text = "You are a smart AI assistant designed to extract structured information from a resume or CV. Your goal is to process resumes from any profession " +
                                  "or industry (e.g., software engineering, teaching, marketing, medicine, law, etc.) and extract all possible relevant details to be stored in a " +
                                  "database.\r\n\r\nYou must handle various formats (chronological, skill-based, mixed) and ensure no relevant information is missed. If any section " +
                                  "is not available, use \"Not Mentioned\" as the value.\r\n\r\nReturn the extracted information in the following JSON format:\n\n" +

                                  "{\r\n  \"full_name\": \"John Doe\",\r\n  \"email\": \"john.doe@example.com\",\r\n  \"phone\": \"+1234567890\",\r\n  \"linkedin\": " +
                                  "\"https://linkedin.com/in/johndoe\",\r\n  \"portfolio\": \"https://johndoe.dev\",\r\n  \"professional_summary\": \"Experienced software " +
                                  "engineer with 5+ years in backend development.\",\r\n  \"skills\": \"C# (Advanced), ASP.NET Core (Advanced), JavaScript (Intermediate), " +
                                  "SQL (Advanced)\",\r\n  \"education\": \"B.Sc. in Computer Science, XYZ University, 2018, CGPA: 3.75\",\r\n  \"certifications\": \"Microsoft" +
                                  " Certified: Azure Developer Associate (2022-06)\",\r\n  \"work_experience\": \"Software Engineer at Tech Solutions Ltd. (2019-2023): Developed" +
                                  " APIs, migrated to Azure, mentored devs.\",\r\n  \"projects\": \"Inventory Management System: Built using ASP.NET Core and SQL Server. Reduced" +
                                  " stock errors by 70%.\",\r\n  \"languages\": \"English (Fluent), Spanish (Basic)\",\r\n  \"awards\": \"Employee of the Year - 2022\",\r\n  " +
                                  "\"extracurricular\": \"Organized internal hackathons and tech talks\",\r\n  \"tools_technologies\": \"Visual Studio, Azure DevOps, Git, Postman\"," +
                                  "\r\n  \"interests\": \"Cloud computing, Microservices, System design\"\r\n}\r\n";

                    string requestPayload = GetRequestBody(text, file, "image/png");

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
                }
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Check your internet connection. AI Response Error: {ex.Message}").ShowAsync();
            }
            finally
            {
            }
            return null;
        }

        public static async Task<string> ReviewExam(string chatHistory, string language)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string text = $" You are an AI model that reviews interview conversations " +
                        $"between an interviewer and a candidate. Given the chat history below," +
                        $" provide the following:\r\n\r\n1. Evaluation:\r\n    - Assess the can" +
                        $"didate's performance overall.\r\n    - Mark the candidate's " +
                        $"performance out of 100, based on the answers provided in the " +
                        $"conversation.\r\n\r\n2. Better Results:\r\n    - Identify specific " +
                        $"questions or areas where the candidate could have provided a better " +
                        $"answer.\r\n    - Provide suggestions for improvement for each of " +
                        $"these areas.\r\n\r\n3. Required Improvements:\r\n    - List areas " +
                        $"where the candidate needs to improve, providing details about what " +
                        $"the candidate should have done better.\r\n    - For each area of " +
                        $"improvement, suggest specific actions the candidate could take to " +
                        $"perform better in future interviews." +
                        $"\n\n Must generate the review in {language}.\r\n\r\nChat History:\r\n\r\n" +
                        $"{chatHistory}" +
                        $"Format:\r\n\r\n- " +
                        $"  Overall Performance Rating:   [out of 100]\r\n-   Better Results:" +
                        $"   [List specific questions and areas for improvement]\r\n-   " +
                        $"Required Improvements:   [Detailed suggestions for improvement]\r\n\r\n  " +
                        $"Please provide your feedback in a clear and structured format, mentioning " +
                        $"Write and give proper answer of the questions separately also. " +
                        $"specific details from the interview.   \r\n\r\n---\r\n\r\n###   Example Usage: " +
                        $" \r\nFor instance, if the chat history includes a question like    Tell me about" +
                        $" a time you worked on a team project   , the model might provide feedback " +
                        $"like:\r\n\r\n-   Better Results:    The candidate's response was vague and " +
                        $"did not provide concrete examples. A more specific example of a team project" +
                        $" with details about the role and outcome would have been ideal. \r\n-   " +
                        $"Required Improvements:    The candidate needs to work on providing more " +
                        $"structured and specific examples when discussing past experiences. Using the" +
                        $" STAR method (Situation, Task, Action, Result) could be helpful. \r\n" +
                        $" As I will show the result in html. Give me the output string in html code \r\n";


                    string requestPayload = GetRequestBody(text, "", "");

                    HttpContent content = new StringContent(requestPayload, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(GeminiEndpoint, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();

                        dynamic result = JsonConvert.DeserializeObject(responseJson);

                        string responseArray = result?.candidates[0]?.content?.parts[0].text;
                        responseArray = responseArray.Replace("```json", "");
                        responseArray = responseArray.Replace("```", "");

                        keyValueStore?.Clear();
                        isQuestionText = false;
                        return responseArray;
                    }
                }
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Error in ReviewExam: {ex.Message}").ShowAsync();
            }

            return null;
        }

        public static async Task<List<string>> GenerateQuestions(string cvText, string questionLimit, string language)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string text = "You are a professional interview assistant AI. Your task is to generate a set of realistic, challenging, and profession-specific interview " +
                        "questions for a candidate based on the structured CV information provided below.\r\n\r\nThese questions should closely resemble those asked in actual " +
                        "interviews for the candidate’s profession, job roles, technologies, and domain expertise." +  
                        $"You have to must generate the questions in {language}\n"+

                        "\r\n\r\nGuidelines for generating questions:\n" +
                        "\r\n\r\n1. Include a mix of technical, situational, and behavioral questions." +
                        "\r\n\r\n2. Make sure the questions are aligned with the candidate’s experience level (fresh graduate,mid-level, senior, etc.)." +
                        "\r\n\r\n3. Use the candidate’s past projects, tools, skills, and work experience to generate tailored questions." +
                        "\r\n\r\n4. Avoid generic or irrelevant questions; be focused and specific." +
                        "\r\n\r\n5. Include at least one Basic question in starting of the interview about the specific expertise tool or stack.  " +
                        "\r\n\r\n Here is the information of the CV:\n" + cvText + "\r\n\r\n" +

                        "Sometimes you may call him/her with his/her real name so taht he can feel the interview as much realistic.\n\n " +

                        "Now, Must generate exactly" + questionLimit + " interview questions." +

                        "\r\n\r\nEnsure the questions are professional, relevant, and varied in complexity. Avoid generic questions and make them specific to " +
                        "the candidate’s background.\n" +

                        "Format the response as a numbered list.\r\n\r\n" +
                        "Return a Json string on questions where key name will be 'Questions'.\r\n\r\n" +
                        "Json string example:" +

                        "\r\n{\r\n  \"questions\": [\r\n    {\r\n      \"question\": \"What is the .Net Framework? " +
                        "\"\r\n    },\r\n    {\r\n      \"question\": \"Describe a situation where you had to refactor legacy code under a tight deadline.\"\r\n    " +
                        "}\r\n {\r\n      \"question\": \"Can you explain how you handled API versioning in your recent ASP.NET Core project?\"\r\\n    \" ]\r\n}\r\n\r\n";

                    string requestPayload = GetRequestBody(text, string.Empty, "");

                    HttpContent content = new StringContent(requestPayload, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(GeminiEndpoint, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();

                        dynamic result = JsonConvert.DeserializeObject(responseJson);

                        string responseArray = result?.candidates[0]?.content?.parts[0].text;
                        responseArray = responseArray.Replace("```json", "");
                        responseArray = responseArray.Replace("```", "");
                        isQuestionText = true;
                        Questions?.Clear();
                        ParseGeminiResponse(responseArray);

                        return Questions;
                    }
                }
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Error in GenerateQuestions: {ex.Message}").ShowAsync();
            }

            return null;
        }

        private static string GetRequestBody(string userInput, string base64Content, string type)
        {
            string json="";

            try
            {
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in GetRequestBody: " + ex.Message);
            }

            return json;
        }

        static void ExtractValues(JToken token, string parentKey = "")
        {
            try
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
                    if (!isQuestionText)
                    {
                        if (!keyValueStore.ContainsKey(parentKey))
                            keyValueStore[parentKey] = new List<string>();

                        keyValueStore[parentKey].Add(token.ToString());
                    }
                    if (isAudioList)
                    {
                        AudQuestions.Add(token.ToString());
                    }
                    else
                    {
                        Questions.Add(token.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Extract values: {ex.Message}");
            }
        }

        private static void ParseGeminiResponse(string responseJson)
        {
            try
            {
                JObject response = JObject.Parse(responseJson);
                ExtractValues(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ParseGeminiResponse: {ex.Message}");
            }
        }

        internal async static Task<string> Transcribe(string propmpt, string base64, string type)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string requestPayload = GetRequestBody(propmpt, base64, type);

                    HttpContent content = new StringContent(requestPayload, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(GeminiEndpoint, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();

                        dynamic result = JsonConvert.DeserializeObject(responseJson);

                        string responseArray = result?.candidates[0]?.content?.parts[0].text;
                        responseArray = responseArray.Replace("```json", "");
                        responseArray = responseArray.Replace("```", "");


                        return responseArray;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Transcribe: {ex.Message}");
            }

            return null;
        }

        public static void ClearAiServiceProps()
        {
            keyValueStore = new Dictionary<string, List<string>>();
            Questions = new List<string>();
            isAudioList = false;
            AudQuestions?.Clear();
            isQuestionText = false;
        }
    }
}
