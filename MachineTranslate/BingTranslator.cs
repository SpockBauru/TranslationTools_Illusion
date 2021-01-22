using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace MachineTranslate
{
    public class BingTranslator
    {
        //Bing needs to get the IG and IID codes from the website before translate
        public static string[] Setup(HttpClient httpClient)
        {
            
            //HttpClient client = new HttpClient();
            var html = httpClient.GetStringAsync("https://www.bing.com/translator").Result;

            //Getting the IG code
            string _IG = searchContent("\",IG:\"", html);

            //Getting IID
            string _IID = searchContent("data-iid=\"", html);
            string[] setup =new string[] { _IG, _IID };
            return setup;
        }
        public static string Translate(string fromLanguage, string toLanguage, string UntranslatedText, HttpClient httpClient, string[] setup, int count)
        {
            string _IG = setup[0];
            string _IID = setup[1];
            //Making the url with the IG and IID 
            string url = "https://www.bing.com/ttranslatev3?isVertical=1&&IG=" + _IG + "&IID=" + _IID + "." + count.ToString();

            //Making the Data format
            var data = new Dictionary<string, string>
            {
                { "fromLang", fromLanguage },
                { "text", UntranslatedText },
                { "to", toLanguage }
            };

            //Sending POST            
            var content = new FormUrlEncodedContent(data);
            var response = httpClient.PostAsync(url, content).Result;
            var responseContent = response.Content;
            string responseString = responseContent.ReadAsStringAsync().Result;

            //Reading Translation
            string translatedText = searchContent("\"text\":\"", responseString);
            translatedText = Regex.Unescape(translatedText);
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
