using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemAndStreams
{
    class Program
    {
        static string dir1 = @"D:\dir1", dir2 = @"D:\dir2";
        //static void Main(string[] args)
        //{
        //    Run(dir1);
        //    SyncDirectories(dir1, dir2);

        //    Console.ReadKey();

        //}

        public static void Run(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
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
            CopyFile(e.FullPath, e.Name);
            Console.WriteLine($"A new file has been changed {e.Name}");
        }

        private static void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            DeleteFile(e.Name);
            Console.WriteLine($"File deleted {e.Name}");
        }

        private static void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {

            RenameFile(e.OldName, e.Name);

            Console.WriteLine($"File renamed {e.Name} {e.FullPath}");
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

        private static void SyncDirectories(string dir1, string dir2)
        {
            CreateDirectoriesRecursive(dir1, dir2);
        }
        private static void CreateDirectoriesRecursive(string dir1, string dir2)
        {
            string[] directories1 = Directory.GetDirectories(dir1);
            string[] directories2 = Directory.GetDirectories(dir2);
            string[] filesDir1 = Directory.GetFiles(dir1);
            string[] filesDir2 = Directory.GetFiles(dir2);


            foreach (var folder1 in directories1)
            {
                bool isFound = false;
                foreach (var folder2 in directories2)
                {
                    if (GetFolderName(folder1) == GetFolderName(folder2))
                        isFound = true;
                }
                if (!isFound)
                {
                    Copy(dir1, dir2);
                }
            }
        }

        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }


        public static string GetFolderName(string path)
        {
            return new DirectoryInfo(path).Name;
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
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static void CopyFile(string path, string fileName)
        {
            string s = path.Replace(dir1 + @"\", "");
            FileInfo fi = new FileInfo(path);

            fi.CopyTo(Path.Combine(path, dir2 + @"\" + s), true);
        }

        public static void DeleteFile(string fileName)
        {
            File.Delete(dir2 + @"\" + fileName);
        }

        public static void RenameFile(string oldName, string newName)
        {
            File.Move(dir2 + @"\" + oldName, dir2 + @"\" + newName);
        }

        public static void CompareTwoFiles() { }
    }
}
