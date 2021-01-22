using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemAndStreams
{
    class RealTimeSync2Dir
    {
        const long mb = 1000000;
        private static readonly string dir1 = @"D:\dir1";
        private static readonly string dir2 = @"D:\dir2";
        static FileSystemWatcher watcher;

        static void Main(string[] args)
        {
            Run(dir1);
            Copy(dir1, dir2);
            RemoveFileFromDir2(dir1, dir2);
            Console.ReadKey();
        }

        public static void Run(string path)
        {
            watcher = new FileSystemWatcher();
            watcher.Path = path;

            watcher.Created += FileSystemWatcher_Created;
            watcher.Renamed += FileSystemWatcher_Renamed;
            watcher.Deleted += FileSystemWatcher_Deleted;
            watcher.Changed += FileSystemWatcher_Changed;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

        }

        private static void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Path.GetExtension(e.FullPath) != String.Empty)
            {
                CopyFile(e.FullPath, e.Name);
                Console.WriteLine($"A new file has been changed {e.Name}");
            }
        }

        private static void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            RenameFile(e.OldName, e.Name);

            Console.WriteLine($"File renamed {e.Name} {e.FullPath}");
        }

        private static void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            DeleteFile(e.Name);
            Console.WriteLine($"File deleted {e.Name}");
        }

        private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (Path.GetExtension(e.FullPath) == String.Empty)
            {
                DirectoryInfo di = new DirectoryInfo(dir2);
                di.CreateSubdirectory(e.Name);

            }
            else
            {
                CopyFile(e.FullPath, e.Name);
                Console.WriteLine($"File created {e.Name} {e.FullPath}");
            }
        }

        public static void RenameFile(string oldName, string newName)
        {
            if (Path.GetExtension(dir2 + @"\" + oldName) != String.Empty)
            {
                File.Move(dir2 + @"\" + oldName, dir2 + @"\" + newName);
            }
            else
                Directory.Move(dir2 + @"\" + oldName, dir2 + @"\" + newName);
        }

        public static void CopyFile(string path, string fileName)
        {
            string s = path.Replace(dir1 + @"\", "");
            Console.WriteLine(s);
            FileInfo fi = new FileInfo(path);

            fi.CopyTo(Path.Combine(path, dir2 + @"\" + s), true);
        }

        public static void DeleteFile(string fileName)
        {
            if (Path.GetExtension(fileName) != String.Empty)
            {
                File.Delete(dir2 + @"\" + fileName);
            }
            else
                Directory.Delete(dir2 + @"\" + fileName);
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
                if (File.Exists(fileDir2))
                {
                    FileInfo first = new FileInfo(fileDir1);
                    FileInfo second = new FileInfo(fileDir2);
                    if (FilesAreEqual_Hash(first, second))
                        continue;
                    else
                    {
                        if (fi.Length < mb)
                        {
                            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                        }
                    }
                }
                else
                    if (fi.Length < mb)
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
