using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;

namespace SuperSocket.Common
{
    public static class StringUtil
    {
        public static long ParseLong(string target)
        {
            long result;
            if (!long.TryParse(target, out result))
            {
                result = 0;
            }
            return result;
        }

        public static int ParseInt(string target)
        {
            int result;
            if (!int.TryParse(target, out result))
            {
                result = 0;
            }
            return result;
        }

        public static short ParseShort(string target)
        {
            short result;
            if (!short.TryParse(target, out result))
            {
                result = 0;
            }
            return result;
        }

        public static decimal ParseDecimal(string target)
        {
            decimal result;
            if (!decimal.TryParse(target, out result))
            {
                result = 0;
            }
            return result;
        }

        public static DateTime ParseDateTime(string target)
        {
            DateTime result;
            if (!DateTime.TryParse(target, out result))
            {
                result = DateTime.MinValue;
            }
            return result;
        }

        public static bool ParseBool(string target)
        {
            bool result;

            if (bool.TryParse(target, out result))
                return result;
            else
                return false;
        }

        public static string GetUTF8String(string strReceived, Encoding encoding)
        {

            byte[] data = encoding.GetBytes(strReceived);

            data = Encoding.Convert(encoding, System.Text.Encoding.UTF8, data);

            return Encoding.UTF8.GetString(data);

        }

        public static string RemoveLink(string rawText)
        {
            int pos = rawText.IndexOf("<a");
            if (pos >= 0)
            {
                int posEnd = rawText.IndexOf(">", pos + 2);
                if (posEnd > 0)
                {
                    rawText = rawText.Substring(0, pos) + rawText.Substring(posEnd + 1);
                    posEnd = rawText.IndexOf("</a>");
                    rawText = rawText.Substring(0, posEnd);
                    return rawText.Trim();
                }
            }
            return rawText;
        }


        public static bool ValidatePasswordParameter(ref string param, int maxSize)
        {
            if (param == null)
            {
                return false;
            }

            if (param.Length < 1)
            {
                return false;
            }

            if (maxSize > 0 && (param.Length > maxSize))
            {
                return false;
            }

            return true;
        }

        public static bool ValidateParameter(ref string param, bool checkForNull, bool checkIfEmpty, bool checkForCommas, int maxSize)
        {
            if (param == null)
            {
                return !checkForNull;
            }

            param = param.Trim();
            if ((checkIfEmpty && param.Length < 1) ||
                 (maxSize > 0 && param.Length > maxSize) ||
                 (checkForCommas && param.Contains(",")))
            {
                return false;
            }

            return true;
        }

        public static string EncryptString(string unEncrypted)
        {
            int rollSize = 5;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(unEncrypted);
            string result = System.Convert.ToBase64String(data);

            StringBuilder sb = new StringBuilder();
            int left = result.Length;

            while (left > rollSize * 2)
            {
                sb.Append(result.Substring(result.Length - left + rollSize, rollSize));
                sb.Append(result.Substring(result.Length - left, rollSize));
                left = left - rollSize * 2;
            }

            sb.Append(result.Substring(result.Length - left, left));

            return sb.ToString();
        }

        public static string DecryptString(string unDecrypted)
        {
            int rollSize = 5;
            StringBuilder sb = new StringBuilder();
            int left = unDecrypted.Length;

            while (left > rollSize * 2)
            {
                sb.Append(unDecrypted.Substring(unDecrypted.Length - left + rollSize, rollSize));
                sb.Append(unDecrypted.Substring(unDecrypted.Length - left, rollSize));
                left = left - rollSize * 2;
            }

            sb.Append(unDecrypted.Substring(unDecrypted.Length - left, left));

            return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(sb.ToString()));
        }

        public static string EncryptPassword(string mess)
        {
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            md5Provider.Initialize();
            byte[] data = md5Provider.ComputeHash(Encoding.UTF8.GetBytes(mess));
            md5Provider.Clear();
            return Encoding.UTF8.GetString(data);
        }

        public static bool IsEmailFormat(string email)
        {
            if (string.IsNullOrEmpty(email) || email.Length > 100)
                return false;

            return Regex.IsMatch(email, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }

        /// </summary>
        /// <param name="text">User's Input</param>
        /// <param name="maxLength">Maximum length of input</param>
        /// <returns>The cleaned up version of the input</returns>
        public static string InputText(string text, int maxLength)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            if (text.Length > maxLength)
                text = text.Substring(0, maxLength);
            //text = Regex.Replace(text, "[\\s]{2,}", " ");	//two or more spaces
            //text = Regex.Replace(text, "(<[b|B][r|R]/*>)+|(<[p|P](.|\\n)*?>)", "\n");	//<br>
            //text = Regex.Replace(text, "(\\s*&[n|N][b|B][s|S][p|P];\\s*)+", " ");	//&nbsp;
            //text = Regex.Replace(text, "<(.|\\n)*?>", string.Empty);	//any other tags
            //text = text.Replace("'", "''");
            return text;
        }

        public static string ResolvePath(string path)
        {

            string[] arrPath = path.Split('|');
            path = string.Empty;
            int max = arrPath.Length - 1;

            for (int i = 0; i < arrPath.Length; i++)
            {
                if (i > 0)
                    path = path + " > " + arrPath[i];
                else
                    path = arrPath[i];
            }

            return path;
        }

        public static long GetPageInfoFromUrl(string url, out int page)
        {
            page = 1;

            if (string.IsNullOrEmpty(url))
                return 0;

            if (!url.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                return 0;

            url = url.ToLower();

            int pos = url.LastIndexOf("/");

            if (pos >= 0)
            {
                url = url.Substring(pos + 1);
                pos = url.LastIndexOf(".aspx");
                url = url.Substring(0, pos);
                return ParseLong(url);
            }
            else
                return 0;
        }

        public static long GetPageInfoFromUrl(string url)
        {
            int page = 1;
            return GetPageInfoFromUrl(url, out page);
        }

        public static string GetUrlLink(string url, bool addTag)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }

            if (addTag)
                return string.Format("<a href=\"{0}\">{0}</a>", url);
            else
                return url;
        }

        public static string GetUrlLink(string url)
        {
            return GetUrlLink(url, true);
        }

        public static bool IsImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            Regex reg = new Regex(@"^.+\.(jpg)|(jpeg)|(gif)|(bmp)$", RegexOptions.IgnoreCase);
            return reg.IsMatch(fileName);
        }

        public static bool IsPackageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            Regex reg = new Regex(@"^.+\.(rar)|(zip)$", RegexOptions.IgnoreCase);
            return reg.IsMatch(fileName);
        }

        public static string[] GetStrArray(string message)
        {
            return GetStrArray(message, ',');
        }

        public static string[] GetStrArray(string message, char seperator)
        {
            return message.Split(seperator);
        }

        public static bool IsNumberString(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            Regex reg = new Regex(@"^[0-9]+$", RegexOptions.IgnoreCase);
            return reg.IsMatch(message);
        }

        public static string ReverseSlash(string message, char slash)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            if (slash == '\\')
            {
                return message.Replace('\\', '/');
            }
            else if (slash == '/')
            {
                return message.Replace('/', '\\');
            }
            else
                return message;
        }

        public static string GetParentDirectory(string dir, char seperator)
        {
            if (seperator != '\\')
                seperator = '/';

            if (string.IsNullOrEmpty(dir))
                return string.Empty;

            if (dir[dir.Length - 1] == seperator)
                dir = dir.Substring(0, dir.Length - 1);

            int pos = dir.LastIndexOf(seperator);

            if (pos < 0)
                return string.Empty;
            else
                return dir.Substring(0, pos);
        }

        public static string GetMD5String(string mess)
        {
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            md5Provider.Initialize();
            byte[] data = Encoding.UTF8.GetBytes(mess);
            byte[] result = md5Provider.ComputeHash(data, 0, data.Length);
            md5Provider.Clear();
            return BitConverter.ToString(result).Replace("-", string.Empty);
        }


        public static string GetMD5String(string mess, bool lowerCase)
        {
            if (string.IsNullOrEmpty(mess))
                return string.Empty;

            return GetMD5String(mess.ToLower());
        }


        /// <summary>
        /// Unquotes and unescapes escaped chars specified text. For example "xxx" will become to 'xxx', "escaped quote \"", will become to escaped 'quote "'.
        /// </summary>
        /// <param name="text">Text to unquote.</param>
        /// <returns></returns>
        public static string UnQuoteString(string text)
        {
            int startPosInText = 0;
            int endPosInText = text.Length;

            //--- Trim. We can't use standard string.Trim(), it's slow. ----//
            for (int i = 0; i < endPosInText; i++)
            {
                char c = text[i];
                if (c == ' ' || c == '\t')
                {
                    startPosInText++;
                }
                else
                {
                    break;
                }
            }
            for (int i = endPosInText - 1; i > 0; i--)
            {
                char c = text[i];
                if (c == ' ' || c == '\t')
                {
                    endPosInText--;
                }
                else
                {
                    break;
                }
            }
            //--------------------------------------------------------------//

            // All text trimmed
            if ((endPosInText - startPosInText) <= 0)
            {
                return "";
            }

            // Remove starting and ending quotes.         
            if (text[startPosInText] == '\"')
            {
                startPosInText++;
            }
            if (text[endPosInText - 1] == '\"')
            {
                endPosInText--;
            }

            char[] chars = new char[endPosInText - startPosInText];

            int posInChars = 0;
            bool charIsEscaped = false;
            for (int i = startPosInText; i < endPosInText; i++)
            {
                char c = text[i];

                // Escaping char
                if (!charIsEscaped && c == '\\')
                {
                    charIsEscaped = true;
                }
                // Escaped char
                else if (charIsEscaped)
                {
                    // TODO: replace \n,\r,\t,\v ???
                    chars[posInChars] = c;
                    posInChars++;
                    charIsEscaped = false;
                }
                // Normal char
                else
                {
                    chars[posInChars] = c;
                    posInChars++;
                    charIsEscaped = false;
                }
            }

            return new string(chars, 0, posInChars);
        }

        public static bool IsRelativePath(this string path)
        {
            if (string.IsNullOrEmpty(path))
                return true;

            if (path.IndexOf(':') >= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string GetConfigElementValue(this NameValueConfigurationCollection collection, string key)
        {
            return GetConfigElementValue(collection, string.Empty);
        }

        public static string GetConfigElementValue(this NameValueConfigurationCollection collection, string key, string defaultValue)
        {
            var e = collection[key];

            if (e == null)
                return defaultValue;

            return e.Value;
        }
    }
}
