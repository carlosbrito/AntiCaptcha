using ConsoleApp2.Api;
using ConsoleApp2.Helper;
using Microsoft.Playwright;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConsoleApp2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Inicia.Start();
        }
    }
}