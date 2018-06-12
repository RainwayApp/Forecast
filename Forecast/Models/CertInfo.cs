using System;
using System.Collections.Generic;
using System.Text;

namespace Forecast.Models
{
    /// <summary>
    /// Holds information about the certficate of the host
    /// </summary>
    public class CertInfo
    {

        /// <summary>
        /// Is the certficate valid
        /// </summary>
        public bool CertValid { get; set; }

        /// <summary>
        /// The subject information
        /// </summary>
        public object Subject { get; set; }

        /// <summary>
        /// The issuer information
        /// </summary>
        public object Issuer { get; set; }


        /// <summary>
        /// Breaks a CNID string down into a key,value pairing.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static object DecodeCNID(string input)
        {
            try
            {
                // WARN: lexer, probably horribly inefficient
                var result = new Dictionary<string, string>();
                var term = "";
                var value = "";
                var open = false;
                var skipnext = false;
                var ignore = false;
                var first = false;
                for (int i = 0; i < input.Length; ++i)
                {
                    var ch = input[i];
                    first = false;
                    if (open)
                    {
                        if (!skipnext && ch == '\\')
                        {
                            skipnext = true;
                            first = true;
                        }
                        else if (!skipnext && ch == '"')
                        {
                            ignore = !ignore;
                        }
                        else if (!skipnext && !ignore && ch == ',')
                        {
                            open = false;
                            i += 1;
                            result[term] = value;
                            term = "";
                        }
                        else
                        {
                            value += ch;
                        }
                        
                        if (skipnext && !first)
                        {
                            skipnext = false;
                        }
                    }
                    else
                    {
                        skipnext = false;
                        if (ch == '=')
                        {
                            open = true;
                            value = "";
                        }
                        else
                        {
                            term += ch;
                        }
                    }
                }
                if (term.Length > 0)
                {
                    result[term] = value;
                }
                return result;
            }
            catch
            {
                return input;
            }
        }
    }
}
