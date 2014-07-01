using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Finder.Util
{
    public static class EncodingUtil
    {
        /// <summary>
        /// 汉字转换为Unicode编码
        /// </summary>
        /// <param name="str">要编码的汉字字符串</param>
        /// <returns>Unicode编码的的字符串</returns>
        public static string ToUnicode(this string str)
        {
            string r = "";
            char[] chars = str.ToCharArray();
            for (int i = 0; i < str.Length; i++)
            {
                if (!str[i].IsUnicode())
                {
                    r += str[i];
                    continue;
                }
                else
                {
                    byte[] bts = Encoding.Unicode.GetBytes(chars, i, 1);

                    for (int j = 0; j < bts.Length; j += 2)
                    {
                        r += "\\u" + bts[j + 1].ToString("x").PadLeft(2, '0') + bts[j].ToString("x").PadLeft(2, '0');
                    }
                }
                
            }
            
            return r;
        }

        /// <summary>
        /// 将Unicode编码转换为汉字字符串
        /// </summary>
        /// <param name="str">Unicode编码字符串</param>
        /// <returns>汉字字符串</returns>
        public static string ToGB2312(this string str)
        {
            string r = "";
            for (int i = 0; i < str.Length; )
            {
                if (str[i] == '\\')
                {
                    i++;
                    if (str[i] == 'u')
                    {
                        i++;
                        byte[] bts = new byte[2];
                        
                        bts[1] = (byte)int.Parse(str.Substring(i, 2), NumberStyles.HexNumber);
                        bts[0] = (byte)int.Parse(str.Substring(i+2, 2), NumberStyles.HexNumber);
                        i += 4;
                        
                        r += Encoding.Unicode.GetString(bts);
                    }
                    else
                    {
                        r += '\\';
                    }
                }
                else
                {
                    r += str[i++];
                }
            }
            return r;
        }

        public static bool IsUnicode(this char c)
        {
            return c >= 256;
        }
    }
}
