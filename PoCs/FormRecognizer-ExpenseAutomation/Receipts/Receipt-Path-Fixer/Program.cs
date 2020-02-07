using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace fixer
{
	class Program
	{
		private static string _pathRaw = @"./files/raw";
		private static string _pathLabeled = @"./files/labeled";
		private static string _pathNonLabeled = @"./files/non-labeled";

		static void Main(string[] args)
		{
			if (!Directory.Exists(_pathLabeled))
				Directory.CreateDirectory(_pathLabeled);
			if (!Directory.Exists(_pathNonLabeled))
				Directory.CreateDirectory(_pathNonLabeled);

			var folderPaths = Directory.GetDirectories(_pathRaw, "*", SearchOption.TopDirectoryOnly);

			// Process each source file folder - assume folders and files in raw path contain ONLY files to be used for Form Recognizer training
			foreach (string folderPath in folderPaths)
			{
				string rawFolderName = Path.GetFileName(folderPath);
				string fixedFolderName = rawFolderName.ToLowerInvariant().Replace(". ", "-").Replace(".", "-").Replace(" ", "-");
				string fixedFolderPath = Path.Combine(_pathLabeled, fixedFolderName);

				// Get files in this folder
				var filePaths = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);

				// Keep it to folders where we have at least 5 files, per Form Recognizer custom model training guideline
				bool haveEnoughFiles = (filePaths.Length >= 5);

				if (haveEnoughFiles)
				{
					foreach (string filePath in filePaths)
					{
						// Clean up leaf folder name
						string fixedFilePath = filePath.Replace(folderPath, fixedFolderPath);

						// Clean up filename - add folder name to it for non-labeled since those have to go into the same folder
						string rawFileName = Path.GetFileName(filePath);
						string fixedFileName = fixedFolderName + "-" + rawFileName.ToLowerInvariant().Replace(" ", "-");

						// Fixed path
						fixedFilePath = fixedFilePath.Replace(rawFileName, fixedFileName);

						string fixedFileFolderPath = Path.GetDirectoryName(fixedFilePath);

						if (!Directory.Exists(fixedFileFolderPath))
							Directory.CreateDirectory(fixedFileFolderPath);

						// Copy raw file to labeled file (=> folder and file names fixed to lower case and spaces replaced with dashes)
						File.Copy(filePath, fixedFilePath);

						// Non-labeled path prep
						string fixedFilePathNonLabeled = Path.Combine(_pathNonLabeled, fixedFileName);

						// Copy raw file to non-labeled file
						File.Copy(filePath, fixedFilePathNonLabeled);
					}
				}
			}
		}
	}
}
