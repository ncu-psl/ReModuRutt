using System.Text.RegularExpressions;

namespace AnalysisExtension.Tool
{
    public class EscapeTokenSet
    {
        public static string DOUBLE_QUOTATION = "\"";
        public static string BACKSLASH = @"\\";

        public static string[,] PAIR_TOKEN = new string[,] { { "{", "}" }, { "(", ")" } };//, {"[", "]" } };

        public static string[] FindPairToken(string content)
        {//need to check if need change to regex match or not
            string[] pairToken = null;

            int index = 0;
            while(index < (PAIR_TOKEN.Length/2))
            {
                if (content.IndexOf(PAIR_TOKEN[index, 0]) > -1)
                {
                    pairToken = new string[2];
                    pairToken[0] = PAIR_TOKEN[index,0];
                    pairToken[1] = PAIR_TOKEN[index,1];
                    break;
                }

                index++;
            }

            return pairToken;
        }

        public static int GetPairTokenIndex(string content)
        {
            int tokenIndex = -1;
            int index = 0;

            while (index < (PAIR_TOKEN.Length / 2))
            {
                if (Regex.Match(content, Regex.Escape(PAIR_TOKEN[index, 0])).Success)
                {
                    tokenIndex = content.IndexOf(PAIR_TOKEN[index, 0]);
                    break;
                }
                else if (Regex.Match(content, Regex.Escape(PAIR_TOKEN[index, 1])).Success)
                {
                    tokenIndex = content.IndexOf(PAIR_TOKEN[index, 1]);
                    break;
                }
                index++;
            }

            return tokenIndex;
        }

        public static string GetToken(string content)
        {
            string token = null;
            int index = 0;

            while (index < (PAIR_TOKEN.Length / 2))
            {
                if (Regex.Match(content, Regex.Escape(PAIR_TOKEN[index, 0])).Success)
                {
                    token = PAIR_TOKEN[index, 0];
                    break;
                }
                else if (Regex.Match(content, Regex.Escape(PAIR_TOKEN[index, 1])).Success)
                {
                    token = PAIR_TOKEN[index, 1];
                    break;
                }
                index++;
            }

            return token;
        }

        public static string[] GetPairToken(string token)
        {
            string[] tokenPair = null;
            int index = 0;

            /*while (index < (PAIR_TOKEN.Length / 2))
            {
                string pattern = Regex.Escape(PAIR_TOKEN[index, 0]) + "(<content>[.]*)" +"|" + "(<content>[.]*)" + Regex.Escape(PAIR_TOKEN[index, 1]) ;
                MatchCollection matches = Regex.Matches(token, pattern);
                foreach (Match match in matches)
                {
                    if (!match.Groups["content"].Value.Contains("\\s\\t") && !match.Groups["content"].Value.Contains("\\n\\r"))
                    {
                        tokenPair = new string[] { PAIR_TOKEN[index, 0], PAIR_TOKEN[index, 1] };
                        break;
                    }
                }
                index++;
            }*/
            while (index < (PAIR_TOKEN.Length / 2))
            {
                string pattern = Regex.Escape(PAIR_TOKEN[index, 0]) + "|" + Regex.Escape(PAIR_TOKEN[index, 1]);
                if (Regex.Match(token, pattern).Success)
                {
                    tokenPair = new string[] { PAIR_TOKEN[index, 0], PAIR_TOKEN[index, 1] };
                    break;
                }
                index++;
            }

            return tokenPair;
        }

        public static bool IsPairToken(string token)
        {
            bool result = false;
            int index = 0;

            /*while (index < (PAIR_TOKEN.Length / 2))
            {
                string pattern =  Regex.Escape(PAIR_TOKEN[index, 0] ) + "|" + Regex.Escape(PAIR_TOKEN[index, 1]) ;
                MatchCollection matches = Regex.Matches(token, pattern);
                foreach (Match match in matches)
                {
                    if (!match.Groups["content"].Value.Contains("\\s\\t") && !match.Groups["content"].Value.Contains("\\n\\r"))
                    {
                        result = true;
                        break;
                    }
                }
                index++;
            }*/
            while (index < (PAIR_TOKEN.Length / 2))
            {
                if (Regex.Match(Regex.Escape(PAIR_TOKEN[index, 0]), token).Success || Regex.Match(Regex.Escape(PAIR_TOKEN[index, 1]), token).Success)
                {
                    result = true;
                    break;
                }
                index++;
            }

            return result;
        }
    }
}
