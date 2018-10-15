using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.ExtensionMethods
{
    public static class ExtSKU
    {
        public static string[] SeparateSku(this string SKU)
        {
            string[] values = new string[2]; 

            string[] s = SKU.Split('-');
            for (int i = 0; i < s.Length - 1; i++)
            {
                if (s[0].Length == 2)
                {
                    values[0] = s[0];
                    if (s[1].Length == 13)
                    {
                        values[1] = s[1];
                        break;
                    }
                }
            }

            return values;
        }
    }
}
