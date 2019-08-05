using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace app
{
	class Program
	{
		static void Main(string[] args)
		{
			Process().Wait();

			Console.WriteLine("Done!");
			Console.ReadKey();
		}

		static async Task Process()
		{
			// Disqualified approach 1...
			//Approach1 a1 = new Approach1();
			//Task t1 = a1.ProcessAsync();

			Approach2 a2 = new Approach2();
			await a2.ProcessAsync();

			//Task.WaitAll(t1, t2);
		}

		private static string GetKey()
		{
			return Console.ReadKey().Key.ToString().Last().ToString().ToUpper();
		}
	}
}
