using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Principal;

namespace dfr
{
    class Program
    {
        public static Dictionary<string, string> files = new Dictionary<string, string>();
        public static List<string> duplicates = new List<string>();
        public static List<string> error = new List<string>();
        public static string hashHolder = string.Empty;
        public static bool warning = false;

        /// <summary>
        /// Main Entry point for program. Establishes actions
        /// loop, and admin warning.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            bool running = true;
            while (running)
            {
                //command line
                if (!IsAdministrator() && !warning)
                {
                    Console.WriteLine("=======================================================================================");
                    Console.WriteLine("| WARNING you are not running this app as administrator.                              |");
                    Console.WriteLine("| In order to ensure files can be deleted it is recommended you run as administrator. |");
                    Console.WriteLine("=======================================================================================");
                    warning = true;
                }

                Console.Write(">"); string action = Console.ReadLine();
                Console.WriteLine(execute(action));
                if(action == "exit")
                {
                    running = false;
                }
            }
        }

        /// <summary>
        /// Execution block. Contains switch statement
        /// for execution method. 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string execute(string command)
        {
            string ret = string.Empty;
            switch(command)
            {
                case "scan":
                    {
                        scanFiles();
                        break;
                    }
                case "list":
                    {
                        listFiles();
                        break;
                    }
                case "list-errors":
                    {
                        listErrors();
                        break;
                    }
                case "delete":
                    {
                        deleteFiles();
                        break;
                    }
                case "dir-clear":
                    {
                        clearDir();
                        break;
                    }
                default:
                    {
                        ret = "Unknown command";
                        break;
                    }
            }
            return ret;
        }

        /// <summary>
        /// List Errors
        /// 
        /// Lists all errors in error list.
        /// </summary>
        private static void listErrors()
        {
            foreach (string err in error)
            {
                Console.WriteLine(err);
            }
        }
 
        /// <summary>
        /// Simple function for checking if the app
        /// is being run in admin mode or not. If
        /// not it returns false.
        /// </summary>
        /// <returns>bool</returns>
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Checks if a directory's content is empty or not.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        /// <summary>
        /// Clears all empty directorys from given path recursivly.
        /// </summary>
        private static void clearDir()
        {
            Console.Write("Enter path>"); string path = Console.ReadLine();
            string[] alldirs = Directory.GetDirectories(path);

            foreach (string dirpath in alldirs)
            {
                if(IsDirectoryEmpty(dirpath))
                {
                    try
                    {
                        Directory.Delete(dirpath);
                        Console.WriteLine("DIRECTORY" + dirpath + " DELETED");
                    } 
                    catch(Exception ex)
                    {
                        error.Add(ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Cycle files
        /// This iterates through all the files in the supplied path.
        /// and checks that a record isnt present in the dictionary, 
        /// if not it adds it and moves on. If a record is present,
        /// it compares the subjects hash to the stored files path
        /// if they are identical it adds them to the duplicate list
        /// and moves on.
        /// 
        /// The directorys in the path are then transversed and the 
        /// method is called from inside itself.
        /// </summary>
        /// <param name="path"></param>
        public static void cycleFiles(string path)
        {
            try
            {
                string[] allfiles = Directory.GetFiles(path, "*.*");

                foreach (string filePath in allfiles)
                {
                    if (File.Exists(filePath))
                    {
                        string flName = Path.GetFileName(filePath);
                        try
                        {
                            if (files.ContainsKey(flName))
                            {
                                if (fileHashCheck(files[flName], filePath))
                                {
                                    // Only duplicates are added to the duplicate list and the original remains.
                                    duplicates.Add(filePath);
                                    Console.WriteLine("Duplicate [" + filePath + "][" + hashHolder + "]");
                                }
                            }
                            else
                            {
                                // We know the file name is the same but if its not the same content we will hit 
                                // this block. So we simply ignore the file, as its been checked and its only a
                                // duplicate in name.
                                if (!files.ContainsKey(flName))
                                {
                                    files.Add(flName, filePath);
                                }
                                Console.WriteLine(filePath);
                            }
                        }
                        catch
                        {
                            Console.WriteLine("read error.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Permissions error accessing the files.");
                error.Add(ex.ToString());
            }
           
            try
            {
                string[] alldirs = Directory.GetDirectories(path);

                foreach (string dirpath in alldirs)
                {
                    cycleFiles(dirpath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Permissions error accessing the folder.");
                error.Add(ex.ToString());
            }
        }
 
        /// <summary>
        /// Initialise a file scan. Clears duplicate and files arrays. 
        /// asks for path and then initialises a scan.
        /// </summary>
        public static void scanFiles()
        {
            duplicates.Clear();
            files.Clear();
            Console.Write("Enter File Path>"); string path = Console.ReadLine();
            Console.WriteLine("Working...");
            cycleFiles(path);

            Console.WriteLine("Following Duplicates discovered");
            Console.WriteLine("===============================");
            foreach (string fl in duplicates)
            {
                Console.WriteLine(fl);
            }
        }

        /// <summary>
        /// print out a list of all file path in duplicate List
        /// </summary>
        private static void listFiles()
        {
            foreach (string fl in duplicates)
            {
                Console.WriteLine(fl);
            }
            Console.WriteLine("TOTAL :" + Convert.ToString(duplicates.Count()));
        }

        /// <summary>
        /// Delete Files - iterates through the duplicate List and 
        /// attempts to delete the file. Clearing the list at the end.
        /// </summary>
        private static void deleteFiles()
        {
            Console.Write("This will delete a total of " + Convert.ToString(duplicates.Count) + " files. Are you sure?");
            string confirm = Console.ReadLine();
            if (confirm.ToUpper() == "Y" || confirm.ToUpper() == "YES")
            {
                foreach (string path in duplicates)
                {
                    try
                    {
                        File.Delete(path);
                        Console.WriteLine("DELETED " + path);
                    }
                    catch
                    {
                        Console.WriteLine("Unable to delete. Check permissions.");
                    }
                }
                duplicates.Clear();
            }
            else
            {
                Console.WriteLine("Operation cancelled. No files were deleted.");
            }
        }

        /// <summary>
        /// File hash, generates two hash's from a base and compare file path,
        /// if these hashes are identicial the file is exactly the same, and the
        /// method returns true, if they are not identical then although named
        /// the same the content of the file is different, so it returns false.
        /// </summary>
        /// <param name="baseFile"></param>
        /// <param name="compare"></param>
        /// <returns>bool</returns>
        public static bool fileHashCheck(string baseFile, string compare)
        { 
            bool ret = false;
            // The cryptographic service provider.
            SHA256 Sha256 = SHA256.Create();
            byte[] A_bytes;
            byte[] B_bytes;
            // Compute the base file's hash.
            using (FileStream stream = File.OpenRead(baseFile))
            {
                A_bytes = Sha256.ComputeHash(stream);
            }

            // Compute the compare file's hash.
            using (FileStream stream2 = File.OpenRead(compare))
            {
                B_bytes = Sha256.ComputeHash(stream2);
            }

            string compareA = string.Empty;
            string compareB = string.Empty;
            foreach (byte b in A_bytes) compareA += b.ToString("x2");
            foreach (byte b in B_bytes) compareB += b.ToString("x2");

            if (compareA == compareB)
            {
                ret = true;
                hashHolder = compareA;
            }

            return ret;
        }
    }
}
