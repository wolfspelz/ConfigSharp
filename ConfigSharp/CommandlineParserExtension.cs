using System.Collections.Generic;

namespace ConfigSharp
{
    public static class CommandlineParserExtension
    {
        public static List<string> ParseCommandline(this string s)
        {
            var args = new List<string>();

            bool done = false;
            bool inString = false;
            string token = "";
            int pos = 0;
            while (!done) {
                bool isData = false;
                switch (s[pos]) {
                    case '"':
                        if (!inString) {
                            inString = true;
                        } else {
                            inString = false;
                        }
                        break;
                    case '\0':
                        done = true;
                        break;
                    case ' ':
                        if (inString) {
                            isData = true;
                        } else {
                            if (token != "") {
                                args.Add(token);
                                token = "";
                            }
                        }
                        break;
                    default:
                        isData = true;
                        break;
                }

                if (!done) {
                    if (isData) {
                        token += s[pos];
                    }
                    pos++;
                    done = (pos >= s.Length);
                }
            }

            if (token != "") {
                args.Add(token);
                token = "";
            }

            return args;
        }
    }
}
