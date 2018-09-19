using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Requests.ModuleDiestel
{
    public class Encriptacion
    {
        public static string PXEncryptFX(string sInput, string sKey)
        {
            var inputLength = sInput.Length;
            var keyLength = sKey.Length;
            var keyValueCharArray = new int[keyLength];
            var inputValueCharArray = new int[inputLength];


            var num6 = 0; //Verificar nombre
            for (int i = 0; i < keyLength; i++)
            {
                int charCode = Strings.AscW(sKey.Substring(i, 1));
                keyValueCharArray[i] = charCode;
                num6 += (charCode * (i + 1)) % 9; //Verificar fomula
            }


            sInput= Reverse(sInput);
            var num11 = 0; //Igual que el 6 
            for (int i = 0; i < inputLength; i++)
            {
                int charCode = Strings.AscW(sInput.Substring(i, 1));
                inputValueCharArray[i] = charCode;
                num11 += (charCode * (i + 1)) % 9; //Igual que el 6
            }


            var Number = (num11 + num6) % 143; //??
            var suffixValue = Number.ToString("X"); //Conversion.Hex()
            if (suffixValue.Length == 1) suffixValue = "0" + suffixValue;
            var num12 = (Strings.AscW(sKey.Substring(0, 1)) + Strings.AscW(sKey.Substring(keyLength - 1, 1)) + keyLength) % 9; //Verificar nombre
            if (num12 == 0) num12 = 20;

            //Primera encriptacion en base a codigo1 y posiciondelKey
            var num13 = (num6 + Number) % keyLength;
            for (int i = 0; i < inputLength; i++)
            {
                int charCode = inputValueCharArray[i] + num12 + keyValueCharArray[num13];
                inputValueCharArray[i] = charCode <= 254 ? charCode : charCode - 254;
                if (num13 < (keyLength - 1)) ++num13; else num13 = 0;
            }

            //Segunda encriptacion en base a secretNumber
            for (int i = 0; i < inputLength; i++)
            {
                int charCode = inputValueCharArray[i] + Number;
                inputValueCharArray[i] = charCode <= 254 ? charCode : charCode - 254;
            }

            //Nuevos codigos a hexadecimal
            string encodeString = "";
            for (int i = 0; i < inputLength; i++)
            {
                string hexValue = inputValueCharArray[i].ToString("X");
                if (hexValue.Length == 1)
                    hexValue = "0" + hexValue;
                encodeString += hexValue;
            }
            return Reverse(suffixValue + encodeString);
        }

        private static string Reverse(string v)
        {
            char[] chars = v.ToCharArray();
            string result = "";

            for (int i = 0, j = v.Length - 1; i < v.Length; i++, j--)
            {
                result += chars[j];
            }

            return result;
        }

        public static string PXDecryptFX(string sInput, string sKey)
        {
            var inputLength = (sInput.Length / 2) - 1;
            var keyLength = sKey.Length;
            var keyValueCharArray = new int[keyLength];
            var inputValueCharArray = new int[inputLength];

            var num6 = 0;
            for (int i = 0; i < keyLength; i++)
            {
                int charCode = Strings.AscW(sKey.Substring(i, 1));
                keyValueCharArray[i] = charCode;
                num6 += (charCode * (i + 1)) % 9;
            }

            var num8 = (Strings.AscW(sKey.Substring(0, 1)) + Strings.AscW(sKey.Substring(keyLength - 1, 1)) + keyLength) % 9;
            if (num8 == 0) num8 = 20;

            sInput = Reverse(sInput);
            var suffixCode = int.Parse(sInput.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            sInput = sInput.Substring(2, sInput.Length - 2);

            for (int i = 0; i < inputLength; i++)
            {
                int charCode = int.Parse(sInput.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                inputValueCharArray[i] = charCode;
            }

            var num14 = (num6 + suffixCode) % keyLength;
            for (int i = 0; i < inputLength; i++)
            {
                int charCode = checked((int)(inputValueCharArray[i] - num8 - keyValueCharArray[num14]));
                inputValueCharArray[i] = charCode >= 1 ? charCode : 254 + charCode;
                if (num14 < keyLength - 1) ++num14; else num14 = 0;
            }


            for (int i = 0; i < inputLength; i++)
            {
                int charCode = inputValueCharArray[i] - suffixCode;
                inputValueCharArray[i] = charCode >= 1 ? charCode : 254 + charCode;
            }

            string decodeString = "";
            for (int i = 0; i < inputLength; i++)
            {
                decodeString += Strings.ChrW(inputValueCharArray[i]);
            }
            return Reverse(decodeString);
        }
    }
}
