using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigSharp
{
    internal static class CommandlineParser
    {
        public static List<string> Parse(string sLine)
        {
            var args = new List<string>();

            bool bDone = false;
            bool bInString = false;
            string sToken = "";
            int nPos = 0;
            while (!bDone) {
                bool bIsData = false;
                switch (sLine[nPos]) {
                    case '"':
                        if (!bInString) {
                            bInString = true;
                        } else {
                            bInString = false;
                        }
                        break;
                    case '\0':
                        bDone = true;
                        break;
                    case ' ':
                        if (bInString) {
                            bIsData = true;
                        } else {
                            if (sToken != "") {
                                args.Add(sToken);
                                sToken = "";
                            }
                        }
                        break;
                    default:
                        bIsData = true;
                        break;
                }

                if (!bDone) {
                    if (bIsData) {
                        sToken += sLine[nPos];
                    }
                    nPos++;
                    bDone = (nPos >= sLine.Length);
                }
            }

            if (sToken != "") {
                args.Add(sToken);
                sToken = "";
            }

            return args;
        }
    }
}
