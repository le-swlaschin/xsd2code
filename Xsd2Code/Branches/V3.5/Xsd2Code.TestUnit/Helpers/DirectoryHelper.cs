using System.IO;

namespace Xsd2Code.TestUnit.Helpers
{
    internal static class DirectoryHelper
    {
        public static void CopyAll(string sourcePath, string targetPath, bool recursive, bool setDefaultAttribute)
        {
            var source = new DirectoryInfo(sourcePath);
            var target = new DirectoryInfo(targetPath);

            if (source.FullName.ToLower() == target.FullName.ToLower())
            {
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                var destFileName = Path.Combine(target.ToString(), fi.Name);
                fi.CopyTo(destFileName, true);
                if (setDefaultAttribute)
                    File.SetAttributes(destFileName, FileAttributes.Normal);
            }

            // Copy each subdirectory using recursion.
            if (recursive)
            {
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir.FullName, nextTargetSubDir.FullName, true, true);
                }
            }
        }

    }
}
