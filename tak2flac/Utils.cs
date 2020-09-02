using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace tak2flac
{
    public static class Utils
    {
        public static void AddOrModify(this Dictionary<string, string> d, string key, string value)
        {
            if (d.ContainsKey(key))
                d[key] = value;
            else
                d.Add(key, value);
        }

        public static string RemoveQuotes(this string s)
        {
            s = s.Remove(0, 1);
            return s.Remove(s.Length - 1, 1);
        }

        public static string[] SplitKeepQuotes(this string s, string delimiter = " ")
        {
            return Regex.Split(s, delimiter + "(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
        }

        public static string PatchFilePath(this string s) {
            //return new FileInfo(s).FullName;
            return Path.GetFullPath(s);
        }

        public static string RemoveUnnecessarySpaces(this string s)
        {
            if (!s.StartsWith(" "))
                return s;
            int startPoint = 0; 
            for(int i = 0;i < s.Length; i++)
            {
                if (s[i] == ' ')
                    startPoint = i;
                else
                {
                    startPoint++;
                    break;
                }
            }
            return s.Substring(startPoint, s.Length - startPoint);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/5132890/c-sharp-replace-bytes-in-byte
        /// </summary>
        /// <param name="src"></param>
        /// <param name="find"></param>
        /// <returns></returns>
        public static int FindBytes(byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;
            // handle the complete source array
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else if (src[i] == find[0])
                {
                    matchIndex = 1;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/5132890/c-sharp-replace-bytes-in-byte
        /// </summary>
        /// <param name="src"></param>
        /// <param name="search"></param>
        /// <param name="repl"></param>
        /// <returns></returns>
        public static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl)
        {
            byte[] dst = null;
            int index = FindBytes(src, search);
            if (index >= 0)
            {
                dst = new byte[src.Length - search.Length + repl.Length];
                // before found array
                Buffer.BlockCopy(src, 0, dst, 0, index);
                // repl copy
                Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
                // rest of src array
                Buffer.BlockCopy(
                    src,
                    index + search.Length,
                    dst,
                    index + repl.Length,
                    src.Length - (index + search.Length));
            }
            return dst;
        } 

        public static void Replace(this byte[] src, byte[] search, byte[] repl)
        {
            ReplaceBytes(src, search, repl);
        }

        public static TimeSpan ParseTimeSpan(this string s)
        {
            if (TryParseTimeSpanString(s, out var result))
                return result;
            else
                return TimeSpan.Zero;
        }

        public static bool TryParseTimeSpanString(string value, out TimeSpan result)
        {
            var m = 0;
            var s = 0;
            var t = 0;

            var end = value.Length;
            var i = 0;
            for (; i < end; i++)
            {
                var v = value[i] - '0';
                if (v >= 0 && v <= 9)
                    m = m * 10 + v;
                else if (value[i] == ':')
                {
                    i++;
                    break;
                }
                else if (char.IsWhiteSpace(value, i))
                {
                    continue;
                }
                else
                {
                    goto ERROR;
                }
            }

            for (; i < end; i++)
            {
                var v = value[i] - '0';
                if (v >= 0 && v <= 9)
                    s = s * 10 + v;
                else if (value[i] == ':')
                {
                    i++;
                    break;
                }
                else if (char.IsWhiteSpace(value, i))
                {
                    continue;
                }
                else
                {
                    goto ERROR;
                }
            }

            var weight = (int)(TICKS_PER_SECOND / 10);
            for (; i < end; i++)
            {
                var v = value[i] - '0';
                if (v >= 0 && v <= 9)
                {
                    t += weight * v;
                    weight /= 10;
                }
                else if (char.IsWhiteSpace(value, i))
                {
                    continue;
                }
                else
                {
                    goto ERROR;
                }
            };
            result = new TimeSpan(t + TICKS_PER_SECOND * s + TICKS_PER_MINUTE * m);
            return true;

        ERROR:
            result = default;
            return false;
        }

        public const long TICKS_PER_MINUTE = TICKS_PER_SECOND * 60;
        public const long TICKS_PER_SECOND = TICKS_PER_MILLISECOND * 1000;
        public const long TICKS_PER_MILLISECOND = 10_000;

    }
}
