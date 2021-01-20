using System;
using System.Collections.Generic;
using System.Net.Http;

namespace MachineTranslate
{
    public class BingTranslator
    {
        public static string Translate(string fromLanguage, string toLanguage, string UntranslatedText)
        {
            //Bing needs to get the IG and IID codes from the website
            HttpClient client = new HttpClient();
            var html = client.GetStringAsync("https://www.bing.com/translator").Result;

            //Getting the IG code
            string _IG = searchContent("\",IG:\"", html);

            //Getting IID
            string _IID = searchContent("data-iid=\"", html);

            //Making the url with the IG and IID 
            string url = "https://www.bing.com/ttranslatev3?isVertical=1&&IG=" + _IG + "&IID=" + _IID + "." + "1";

            //Making the Data format
            var data = new Dictionary<string, string>
            {
                { "fromLang", fromLanguage },
                { "text", UntranslatedText },
                { "to", toLanguage }
            };

            //Sending POST            
            var content = new FormUrlEncodedContent(data);
            var response = client.PostAsync(url, content).Result;
            var responseContent = response.Content;
            string responseString = responseContent.ReadAsStringAsync().Result;

            //Reading Translation
            string translatedText = searchContent("\"text\":\"", responseString);

            return translatedText;
        }
        static string searchContent(string searchFor, string content)
        {
            int startIndex = content.IndexOf(searchFor) + searchFor.Length;
            int endIndex = content.IndexOf("\",", startIndex);

            string output = content.Substring(startIndex, endIndex - startIndex);
            return output;
        }
    }
}
