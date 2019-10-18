// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.Utils
{
    public static class Strings
    {
        // https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
        public static string ToBase64(this string text)
        {
            return ToBase64(text, Encoding.UTF8);
        }

        public static string ToBase64(this string text, Encoding encoding)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }

        public static bool TryParseBase64(this string text, out string decodedText)
        {
            return TryParseBase64(text, Encoding.UTF8, out decodedText);
        }

        public static bool TryParseBase64(this string text, Encoding encoding, out string decodedText)
        {
            if (string.IsNullOrEmpty(text))
            {
                decodedText = text;
                return false;
            }

            try
            {
                byte[] textAsBytes = Convert.FromBase64String(text);
                decodedText = encoding.GetString(textAsBytes);
                return true;
            }
            catch (Exception)
            {
                decodedText = null;
                return false;
            }
        }

        // https://stackoverflow.com/questions/1601834/c-implementation-of-or-alternative-to-strcmplogicalw-in-shlwapi-dll
        public class NaturalComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                int lx = x.Length, ly = y.Length;

                for (int mx = 0, my = 0; mx < lx && my < ly; mx++, my++)
                {
                    if (char.IsDigit(x[mx]) && char.IsDigit(y[my]))
                    {
                        long vx = 0, vy = 0;

                        for (; mx < lx && char.IsDigit(x[mx]); mx++)
                            vx = vx * 10 + x[mx] - '0';

                        for (; my < ly && char.IsDigit(y[my]); my++)
                            vy = vy * 10 + y[my] - '0';

                        if (vx != vy)
                            return vx > vy ? 1 : -1;
                    }

                    if (mx < lx && my < ly && x[mx] != y[my])
                        return x[mx] > y[my] ? 1 : -1;
                }

                return lx - ly;
            }
        }
    }
}
