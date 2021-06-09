using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FIleHasher
{
	class Program
	{
		private static BackgroundWorker bgWorker;

		static void Main()
		{
			// set the background worker events
			bgWorker = new BackgroundWorker();
			bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
			bgWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
			bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
			bgWorker.WorkerReportsProgress = true;

			// set the console content
			Console.Title = "File Hasher";

			// create a variable to store the path of the file to compute the hash of
			string filePath = string.Empty;

			// prompt the user to enter a file path
			Console.WriteLine("Please enter the path for the file you'd like to hash:");

			// get the user input
			filePath = Console.ReadLine();

			if (filePath == "this" || filePath == "." || filePath == "")
				filePath = Application.ExecutablePath;

			// run the background worker to compute the file's hash
			bgWorker.RunWorkerAsync(filePath);

			// let the user know the hash is being computed
			Console.WriteLine("\nComputing hash...\n");

			Console.ReadLine();
			Console.Clear();
		}

		private static void bgWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			string filePath = e.Argument.ToString();

			byte[] buffer;
			int bytesRead;
			long size;
			long totalBytesRead = 0;

			using (Stream file = File.OpenRead(filePath))
			{
				size = file.Length;

				using (HashAlgorithm hasher = MD5.Create())
				{
					do
					{
						buffer = new byte[4096];
						bytesRead = file.Read(buffer, 0, buffer.Length);
						totalBytesRead += bytesRead;

						hasher.TransformBlock(buffer, 0, bytesRead, null, 0);
						bgWorker.ReportProgress((int)((double)totalBytesRead / size * 100));
					}
					while (bytesRead != 0);

					hasher.TransformFinalBlock(buffer, 0, 0);
					e.Result = MakeHashString(hasher.Hash);
				}
			}
		}
		private static void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			Console.Title = string.Format("File Hasher - {0}%", e.ProgressPercentage);
		}
		private static void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Console.Write(e.Result.ToString());
		}

		private static string MakeHashString(byte[] hashBytes)
		{
			StringBuilder hash = new StringBuilder(32);

			foreach (byte b in hashBytes)
				hash.Append(b.ToString("X2").ToLower());

			return hash.ToString();
		}
	}
}
