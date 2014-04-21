using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IP4Adressen
{
    class Program
    {
        static void Main(string[] args)
        {
            const string pattern = @"\b\d{3}[.]\d{3}[.]\d{3}[.]\d{3}\b"; // IPv4 &Subnet Pattern
            while (true)
            {
                Console.Clear();
                Console.WriteLine("IP4 Adressen Rechner:");

                Console.WriteLine("IP Adresse eingeben: xxx.xxx.xxx.xxx");
                Console.Write("Eingabe: ");
                var inputIp = Console.ReadLine();
               // var inputIp = "192.168.100.117";

                Console.WriteLine("Netzmaske eingeben: xxx.xxx.xxx.xxx");
                Console.Write("Eingabe: ");
                var inputSubnet = Console.ReadLine();
               // var inputSubnet = "255.255.255.224";
               
                var matchIp = Regex.Match(inputIp, pattern);
                var matchSubnet = Regex.Match(inputSubnet, pattern);
                if (matchIp.Success && matchSubnet.Success)
                {
                    // Console.WriteLine("input is correct");

                    var ip = CreateBitArray(inputIp);
                    var subnet = CreateBitArray(inputSubnet);

                    var netzId = GetNetId(ip, subnet);
                    var broadcast = GetBroadcast(ip, subnet);
                    var firstHost = GetHost(netzId.Select(ConvertToInt).ToArray(),Hostaddress.Fist);
                    var lastHost = GetHost(broadcast.Select(ConvertToInt).ToArray(),Hostaddress.Last);


                    Console.Clear();
                    Console.WriteLine(string.Format("{0,-25}{1}", "IPv4-Adresse:", inputIp));
                    Console.WriteLine(string.Format("{0,-25}{1}", "Netzmaske: ", inputSubnet));
                    Console.WriteLine(string.Format("{0,-25}{1}", "NetzId: ", MakeDottedNotation(netzId.Select(ConvertToInt).ToArray())));
                    Console.WriteLine(string.Format("{0,-25}{1}", "Boradcast: ", MakeDottedNotation(broadcast.Select(ConvertToInt).ToArray())));
                    Console.WriteLine(string.Format("{0,-25}{1}", "kleinste Hostadresse: ", MakeDottedNotation(firstHost.ToArray())));
                    Console.WriteLine(string.Format("{0,-25}{1}", "Größte Hostadresse: ", MakeDottedNotation(lastHost.ToArray())));
                    Console.WriteLine("\nWeiter mit belibiger Taste..");
                    Console.ReadKey();

                   
                }
                else
                {
                    Console.WriteLine("Eingabe ist nicht Korrekt!");
                    
                    
                }

            }
        }

        private static int[] GetHost(int[] values, Hostaddress host)
        {
            var clone = values.Clone() as int[];
            
            if (host == Hostaddress.Fist)
                clone[3]++;
            if (host == Hostaddress.Last)
                clone[3]--;
            return clone;
        }

        /// <summary>
        /// Wandelt den Input String in ein bitArray um 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static bool[][] CreateBitArray(string input)
        {
           var substring = input.Split('.');
           var adr = new bool[4][];

           for (var i = 0; i < substring.Length; i++)
           {
               var tmp = Convert.ToInt32(substring[i]);
               var b = new BitArray(new[] { tmp });
               var bits = new bool[b.Count];
               b.CopyTo(bits, 0);
               adr[i] = TrimArray(bits).ToArray();
           }
            return adr;
        }

        /// <summary>
        ///  Trimmt das Array mit der länge 32 auf die Länge 8 (default)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        static IEnumerable<bool> TrimArray(IEnumerable<bool> value, int len = 8)
        {
            var ctn = 0;
            foreach (var b in value)
            {
                if (ctn < len)
                    yield return b;
                ctn ++;
            }
        }

        /// <summary>
        ///  Gibt die Netz Id wieder
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="subnet"></param>
        /// <returns></returns>
        static bool[][] GetNetId(bool[][] ip, bool[][] subnet)
        {
            var result = new bool[ip.Length][];
            for (var i = 0; i < ip.Length; i++)
            {
                result[i] = new bool[ip[i].Length];
                for (int j = 0; j < ip[i].Length; j++)
                {
                    result[i][j] = ip[i][j] && subnet[i][j];
                }
            }
            return result;
        }

       
        /// <summary>
        /// Gibt die Broadcast Addresse wieder
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="subnet"></param>
        /// <returns></returns>
        static bool[][] GetBroadcast(bool[][] ip, bool[][] subnet)
        {
            
            var inv = Inverse(subnet);
            var result = new bool[ip.Length][];
            for (var i = 0; i < ip.Length; i++)
            {
                result[i] = new bool[ip[i].Length];
                for (int j = 0; j < ip[i].Length; j++)
                {
                    result[i][j] = ip[i][j] && subnet[i][j];
                    if (!subnet[i][j])
                        result[i][j] = ip[i][j] || inv[i][j];
                }
            }
            return result;
        }

        /// <summary>
        /// Invertiert die Adresse
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        static bool[][] Inverse(bool[][] values)
        {
            var result = new bool[values.Length][];
            for (var i = 0; i < values.Length; i++)
            {
                result[i] = new bool[values[i].Length];
                for (int j = 0; j < values[i].Length; j++)
                {
                    result[i][j] = !values[i][j];
                }
            }
            return result;
        }

        /// <summary>
        /// Convertiert ein bool[] in int 
        /// bsp: 1010(BIN) = 1*8 + 0*4 + 1*2 + 0*1 = 10(DEZ)
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        static int ConvertToInt(bool[] values)
        {
            var res = 0;
            var multiplyer = 1;
            foreach (var t in values)
            {
                res += t ? multiplyer: 0;
                multiplyer *= 2;
            }
            return res;
        }

       /// <summary>
       /// Baut aus einem Array von Intager eine gültige Adresse
       /// bsp: 192.168.100.117
       /// </summary>
       /// <param name="values"></param>
       /// <returns></returns>
        static string MakeDottedNotation(int[] values)
        {
            var sb = new StringBuilder();
            foreach (var value in values)
            {
                sb.Append(PrefixZero(value));
                sb.Append(".");
            }
            sb.Remove(sb.Length - 1, 1); // entfert den Letzen punkt!
            return sb.ToString();
        }

        /// <summary>
        /// Stellt eine bis zwei Nullen bei Werten und 10 bzw 100 voran. 3 --> 003 , 58 --> 058
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static string PrefixZero(int value)
        {
            if (value < 10)
                return "00" + value;
            if (value < 100)
                return "0" + value;
            return Convert.ToString(value);
        }
    }

}
