using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemAndStreams
{
    class Sync2Folders
    {
        const int BYTES_TO_READ = sizeof(Int64);
        const long mb = 1000000;
        //static void Main(string[] args)
        //{
        //    string dir1 = @"D:\dir1", dir2 = @"D:\dir2";
        //    Copy(dir1, dir2);
        //    RemoveFileFromDir2(dir1 ,dir2);
        //    Console.ReadKey();

        //}

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


        public static void RemoveFileFromDir2(string sourceDirectory, string targetDirectory)
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
                    Console.WriteLine("Trying to delete " + fi2.FullName);
                    File.Delete(fi2.FullName);
                }
            }
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                var fileDir1 = fi.FullName;
                var fileDir2 = target + @"\" + fi.Name;
                Console.WriteLine(fileDir2);
                if (File.Exists(fileDir2)) {
                    FileInfo first = new FileInfo(fileDir1);
                    FileInfo second = new FileInfo(fileDir2);
                    if (FilesAreEqual_Hash(first, second))
                        continue;
                    else
                    {
                        if(fi.Length < mb)
                        {
                            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                        }
                    }
                }
                else
                    if(fi.Length < mb)
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }
            }
            
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {

                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
