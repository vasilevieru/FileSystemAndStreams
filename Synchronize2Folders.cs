using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemAndStreams
{
    class Synchronize2Folders
    {
        const long mb = 1000000;

        //static async Task Main(string[] args)
        //{
        //    string dir1 = @"D:\directory1", dir2 = @"D:\directory2";

        //    await CopyAsync(dir1, dir2);

        //    Console.WriteLine("Finished");
        //    RemoveFilesFromDir2(dir1, dir2);
        //    RemoveFoldersFromDir2(dir1, dir2);
        //    Console.ReadKey();
        //}

        public static void RemoveFilesFromDir2(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);            

            foreach (FileInfo fi2 in diTarget.GetFiles("*", SearchOption.AllDirectories))
            {
                bool exist = false;
                foreach (FileInfo fi1 in diSource.GetFiles("*", SearchOption.AllDirectories))
                {
                    var path = fi2.FullName.Replace(targetDirectory, "");
                    if (File.Exists(diSource + path))
                        exist = true;
                }
                if (!exist)
                {
                    File.Delete(fi2.FullName);
                }
            }            
        }

        public static void RemoveFoldersFromDir2(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            foreach (DirectoryInfo di2 in diTarget.GetDirectories("*", SearchOption.AllDirectories))
            {
                bool exist = false;
                foreach (DirectoryInfo di1 in diSource.GetDirectories("*", SearchOption.AllDirectories))
                {
                    var path = di2.FullName.Replace(targetDirectory, "");
                    if (Directory.Exists(diSource + path))
                        exist = true;
                }
                if (!exist)
                {
                    Directory.Delete(di2.FullName);
                }
            }
        }

        public static async Task CopyAsync(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            await CopyFileAsync(diSource, diTarget);
        }

        public static async Task CopyFileAsync(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
        {
            Directory.CreateDirectory(destinationDirectory.FullName);

            foreach (FileInfo fi in sourceDirectory.GetFiles().Where(x => !x.Attributes.HasFlag(FileAttributes.System)))
            {
                var sourcePath = fi.FullName;
                var destinationPath = destinationDirectory + @"\" + fi.Name;

                if (File.Exists(destinationPath))
                {
                    FileInfo first = new FileInfo(sourcePath);
                    FileInfo second = new FileInfo(destinationPath);

                    if (FilesAreEqual_Hash(first, second))
                        continue;
                }

                if (fi.Length < mb)
                {
                    await CreateOrReplace(fi, destinationPath);
                }
            }

            foreach (DirectoryInfo diSourceSubDir in sourceDirectory.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    destinationDirectory.CreateSubdirectory(diSourceSubDir.Name);
                await CopyFileAsync(diSourceSubDir, nextTargetSubDir);
            }
        }

        private static async Task CreateOrReplace(FileInfo fi, string destinationPath)
        {
            using (FileStream source = fi.OpenRead())
            using (FileStream destination = File.Create(destinationPath))
            {
                await source.CopyToAsync(destination);
            }
        }

        static bool FilesAreEqual_Hash(FileInfo first, FileInfo second)
        {
            using (var readFirst = first.OpenRead())
            using (var readSecond = second.OpenRead())
            {
                byte[] firstHash = MD5.Create().ComputeHash(readFirst);
                byte[] secondHash = MD5.Create().ComputeHash(readSecond);

                for (int i = 0; i < firstHash.Length; i++)
                {
                    if (firstHash[i] != secondHash[i])
                        return false;
                }
            }
            return true;
        }

    }
}
