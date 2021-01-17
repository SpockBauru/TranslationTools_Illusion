using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Globalization;

namespace MachineTranslate
{
    public class GoogleTranslate
    {
        public static string Translate(string fromLanguage, string toLanguage, string input)
        {
            //Making the url
            string url = String.Format
            ("https://translate.googleapis.com/translate_a/single?client=webapp&sl={0}&tl={1}&dt=t&tk={2}&q={3}",
              fromLanguage,
              toLanguage,
              Tk(input),
              Uri.EscapeDataString(input));

            //Requesting translations synchronously 
            HttpClient httpClient = new HttpClient();
            var response = httpClient.GetAsync(url).Result;

            string result = "";
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;

                // by calling .Result you are synchronously reading the result
                result = responseContent.ReadAsStringAsync().Result;
            }

            //Getting translation from result
            string translation = result.Substring(4, result.IndexOf("\",\"") - 4);
            return translation;
        }

        // TKK Approach stolen from Translation Aggregator r190, all credits to Sinflower
        static long Vi(long r, string o)
        {
            for (var t = 0; t < o.Length; t += 3)
            {
                long a = o[t + 2];
                a = a >= 'a' ? a - 87 : a - '0';
                a = '+' == o[t + 1] ? r >> (int)a : r << (int)a;
                r = '+' == o[t] ? r + a & 4294967295 : r ^ a;
            }

            return r;
        }

        static string Tk(string r)
        {
            long m = 427761;
            long s = 1179739010;
            List<long> S = new List<long>();

            for (var v = 0; v < r.Length; v++)
            {
                long A = r[v];
                if (128 > A)
                    S.Add(A);
                else
                {
                    if (2048 > A)
                        S.Add(A >> 6 | 192);
                    else if (55296 == (64512 & A) && v + 1 < r.Length && 56320 == (64512 & r[v + 1]))
                    {
                        A = 65536 + ((1023 & A) << 10) + (1023 & r[++v]);
                        S.Add(A >> 18 | 240);
                        S.Add(A >> 12 & 63 | 128);
                    }
                    else
                    {
                        S.Add(A >> 12 | 224);
                        S.Add(A >> 6 & 63 | 128);
                    }

                    S.Add(63 & A | 128);
                }
            }

            const string F = "+-a^+6";
            const string D = "+-3^+b+-f";
            long p = m;

            for (var b = 0; b < S.Count; b++)
            {
                p += S[b];
                p = Vi(p, F);
            }

            p = Vi(p, D);
            p ^= s;
            if (0 > p)
                p = (2147483647 & p) + 2147483648;

            p %= (long)1e6;

            return p.ToString(CultureInfo.InvariantCulture) + "." + (p ^ m).ToString(CultureInfo.InvariantCulture);
        }
    }
}

