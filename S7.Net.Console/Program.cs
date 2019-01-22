using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S7.Net.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 188; i < 255; i++)
            {
                using (var plc = new Plc(CpuType.S71500, $"192.168.2.{i}", 0, 1))
                {
                    if (plc.IsAvailable)
                    {
                        System.Console.WriteLine($"192.168.2.{i} OK");
                        break;
                    }
                    else
                    {
                        System.Console.WriteLine($"192.168.2.{i} NG");
                    }
                }
                
            }
            System.Console.ReadKey();
        }
    }
}
