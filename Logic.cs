using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BoxArchiveGUI
{
    public class Logic
    {

        public const string START = "STaRT";

        public const string SEPARATOR = "SEParaTOR";


        public static void BoxCans(string destFile, CompressionLevel cl)
        {
            if (Directory.Exists("canned") && Directory.Exists("input"))
            {
                ZipFile.CreateFromDirectory(new DirectoryInfo("canned").FullName, "Archive.ZIP", cl, false);
                byte[] data = File.ReadAllBytes("Archive.ZIP");

                File.Delete("Archive.ZIP");

                Directory.Delete("input", true);
                Directory.Delete("canned", true);

                Array.Reverse(data);

                File.WriteAllBytes(destFile, data);
            }
        }

        public static void UnboxBoxFile(string sourceFile)
        {
            if (!File.Exists(sourceFile))
            {
                ErrorRoutine("Неверный путь к архиву.");
            }

            DirectoryInfo dir;

            if (!Directory.Exists("unboxed"))
            {
                dir = Directory.CreateDirectory("unboxed");
                dir.Attributes |= FileAttributes.Hidden;
            }
            else
            {
                Directory.Delete("uncanned", true);
                dir = Directory.CreateDirectory("uncanned");
                dir.Attributes |= FileAttributes.Hidden;
            }

            byte[] data = File.ReadAllBytes(sourceFile);

            Array.Reverse(data);

            File.Create($"{sourceFile}.ZIP").Dispose();
            File.WriteAllBytes($"{sourceFile}.ZIP", data);

            ZipFile.ExtractToDirectory($"{sourceFile}.ZIP", dir.FullName);

            if (File.Exists($"{sourceFile}.ZIP")) { File.Delete($"{sourceFile}.ZIP"); }
        }

        public static void OpenCans()
        {
            DirectoryInfo dir;

            if (!Directory.Exists("uncanned"))
            {
                dir = Directory.CreateDirectory("uncanned");
                dir.Attributes |= FileAttributes.Hidden;
            }
            else
            {
                Directory.Delete("uncanned", true);
                dir = Directory.CreateDirectory("uncanned");
                dir.Attributes |= FileAttributes.Hidden;
            }

            if (Directory.Exists("unboxed") && Directory.Exists("uncanned"))
            {
                foreach (FileInfo i in new DirectoryInfo("unboxed").EnumerateFiles())
                {
                    DecompressFile(Path.Combine("unboxed", i.Name), Path.Combine("uncanned", $"{i.Name}.INT"));
                }
            }

        }

        public static void UnpackInts()
        {
            DirectoryInfo dir;

            if (!Directory.Exists("output"))
            {
                dir = Directory.CreateDirectory("output");
            }
            else
            {
                Directory.Delete("output", true);
                dir = Directory.CreateDirectory("output");
            }

            byte[][] data = new byte[][] { };

            foreach (FileInfo file in new DirectoryInfo("uncanned").EnumerateFiles().ToArray())
            {
                ArrayPush(ref data, File.ReadAllBytes(file.FullName));
            }

            UnpackWrapper(data, new DirectoryInfo("output"));
        }

        public static void PackFilesToInt(string[] files, int startPoint)
        {
            try
            {
                List<FileInfo> infos = new List<FileInfo>();

                for (int i = startPoint; i < files.Length; i++)
                {
                    if (File.Exists(files[i]))
                    {
                        infos.Add(new FileInfo(files[i]));
                    }
                }

                byte[][] data = PrepareData(infos.ToArray());

                for (int i = 0; i < data.Length; i++)
                {
                    File.WriteAllBytes(Path.Combine("input", $"{i}.INT"), data[i]);
                }
            }
            catch
            {
                if (Directory.Exists("input")) { Directory.Delete("input", true); }

                if (Directory.Exists("canned")) { Directory.Delete("canned", true); }

            }
        }



        public static void CanFiles()
        {

            if (Directory.Exists("input") && Directory.Exists("canned"))
            {
                foreach (FileInfo i in new DirectoryInfo("input").EnumerateFiles())
                {
                    CompressFile(Path.Combine("input", i.Name), Path.Combine("canned", $"{i.Name}.CAN"));
                }
            }

        }

        public static void ErrorRoutine(string message)
        {
            if (Directory.Exists("unboxed")) { Directory.Delete("unboxed", true); }
            if (Directory.Exists("uncanned")) { Directory.Delete("uncanned", true); }

            if (Directory.Exists("input")) { Directory.Delete("input", true); }
            if (Directory.Exists("canned")) { Directory.Delete("canned", true); }


            Environment.Exit(0);
        }

        public static void ErrorRoutine()
        {
            if (Directory.Exists("unboxed")) { Directory.Delete("unboxed", true); }
            if (Directory.Exists("uncanned")) { Directory.Delete("uncanned", true); }

            if (Directory.Exists("input")) { Directory.Delete("input", true); }
            if (Directory.Exists("canned")) { Directory.Delete("canned", true); }
        }

        public static void ArrayPush<T>(ref T[] table, T value)
        {
            Array.Resize(ref table, table.Length + 1);
            table[table.Length - 1] = value;
        }

        public static byte[][] PrepareData(FileInfo[] files)
        {



            byte[][] result = new byte[][] { };

            List<List<byte>> preparedData = new List<List<byte>>();

            byte[] rule = Encoding.GetEncoding(1251).GetBytes(SEPARATOR);

            byte[] start = Encoding.GetEncoding(1251).GetBytes(START);



            if ((files.Length % 2) == 0)
            {

                int j = 0;

                for (int i = 0; i < (files.Length / 2); i++)
                {
                    List<byte> dataToPrepare = new List<byte>();
                    List<byte> header = new List<byte>();

                    j++;

                    string origName0 = files[(i == 0) ? (j - 1) : (j + (i - 1))].Name;
                    string origName1 = files[(i == 0) ? j : (j + i)].Name;

                    string name0 = Regex.Replace(origName0, " ", string.Empty);
                    string name1 = Regex.Replace(origName1, " ", string.Empty);

                    header.AddRange(Encoding.GetEncoding(1251).GetBytes($"{0} " + name0 + "\t").Concat(Encoding.GetEncoding(1251).GetBytes($"{1} " + name1 + "\t")).Cast<byte>());

                    header.AddRange(start);

                    dataToPrepare.AddRange(header);

                   

                    dataToPrepare.AddRange(File.ReadAllBytes(files[(i == 0) ? (j - 1) : (j + (i - 1))].FullName).
                        Concat(rule).
                        Concat(File.ReadAllBytes(files[(i == 0) ? j : (j + i)].FullName).
                        Concat(rule)));

                    string debug1 = Encoding.GetEncoding(1251).GetString(dataToPrepare.ToArray());

                    dataToPrepare.Reverse();


                    preparedData.Add(dataToPrepare);


                }

                for (int i = 0; i < preparedData.Count; i++)
                {
                    ArrayPush(ref result, preparedData[i].ToArray());
                }
            }
            else if (files.Length == 1)
            {
                EncodingInfo[] infos = Encoding.GetEncodings();

                List<byte> finalDataToPrepare = new List<byte>();
                List<byte> finalHeader = new List<byte>();

                string origName = files[0].Name;
                string name0 = Regex.Replace(origName, " ", string.Empty);

                finalHeader.AddRange(Encoding.GetEncoding(1251).GetBytes($"{0} " + name0 + "\t").Cast<byte>());

                finalHeader.AddRange(start);

                finalDataToPrepare.AddRange(finalHeader);

                finalDataToPrepare.AddRange(File.ReadAllBytes(files[0].FullName).Concat(rule).ToList());

                string debug1 = Encoding.GetEncoding(1251).GetString(finalDataToPrepare.ToArray());

                finalDataToPrepare.Reverse();

                preparedData.Add(finalDataToPrepare);

                for (int i = 0; i < preparedData.Count; i++)
                {
                    ArrayPush(ref result, preparedData[i].ToArray());
                }
            }
            else
            {
                int j = 0;

                for (int i = 0; i < (files.Length / 2); i++)
                {

                    List<byte> dataToPrepare = new List<byte>();
                    List<byte> header = new List<byte>();

                    j++;

                    string origName0 = files[(i == 0) ? (j - 1) : (j + (i - 1))].Name;
                    string origName1 = files[(i == 0) ? j : (j + i)].Name;

                    string name0 = Regex.Replace(origName0, " ", string.Empty);
                    string name1 = Regex.Replace(origName1, " ", string.Empty);

                    header.AddRange(Encoding.GetEncoding(1251).GetBytes($"{0} " + name0 + "\t").Concat(Encoding.GetEncoding(1251).GetBytes($"{1} " + name1 + "\t")).Cast<byte>());


                    header.AddRange(start);

                    dataToPrepare.AddRange(header);

                    dataToPrepare.AddRange(File.ReadAllBytes(files[(i == 0) ? (j - 1) : (j + (i - 1))].FullName).
                        Concat(rule).
                        Concat(File.ReadAllBytes(files[(i == 0) ? j : (j + i)].FullName).
                        Concat(rule)));



                    string debug1 = Encoding.GetEncoding(1251).GetString(dataToPrepare.ToArray());

                    dataToPrepare.Reverse();


                    preparedData.Add(dataToPrepare);

                }

                List<byte> finalDataToPrepare = new List<byte>();
                List<byte> finalHeader = new List<byte>();

                
                finalHeader.AddRange(Encoding.GetEncoding(1251).GetBytes($"{0} " + files[files.Length - 1].Name + "\t").Cast<byte>());


                finalHeader.AddRange(start);

                finalDataToPrepare.AddRange(finalHeader);

                finalDataToPrepare.AddRange(File.ReadAllBytes(files[files.Length - 1].FullName).Concat(rule).ToList());

                finalDataToPrepare.Reverse();

                preparedData.Add(finalDataToPrepare);

                for (int i = 0; i < preparedData.Count; i++)
                {
                    ArrayPush(ref result, preparedData[i].ToArray());
                }
            }



            return result;

        }





        public static void CompressFile(string filename, string destfile)
        {
            using (FileStream originalFileStream = File.Open(filename, FileMode.Open))
            {
                using (FileStream compressedFileStream = File.Create(destfile))
                {
                    using (var compressor = new GZipStream(compressedFileStream, CompressionLevel.Optimal))
                    {
                        originalFileStream.CopyTo(compressor);
                    }
                }
            }
        }

        public static void DecompressFile(string filename, string destfile)
        {
            using (FileStream compressedFileStream = File.Open(filename, FileMode.Open))
            {
                using (FileStream outputFileStream = File.Create(destfile))
                {
                    using (var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                    {
                        decompressor.CopyTo(outputFileStream);
                    }
                }
            }
        }

        public static void UnpackWrapper(byte[][] intFiles, DirectoryInfo dir)
        {
            for (int i = 0; i < intFiles.Length; i++)
            {
                UnpackFiles(intFiles[i], dir);
            }

        }

        public static void UnpackFiles(byte[] data, DirectoryInfo dir)
        {


            string prevDir = Directory.GetCurrentDirectory();



            Regex headerSplitter = new Regex(START);
            Regex fileSplitter = new Regex(SEPARATOR);

            Array.Reverse(data);

            string toParse = Encoding.GetEncoding(1251).GetString(data);

            string[] parsed1 = headerSplitter.Split(toParse);

            string[] parsed2 = fileSplitter.Split(parsed1[1]);

            string[] parsedHeader = parsed1[0].Split('\t').Where(f => f != string.Empty).ToArray();

            

            Dictionary<int, string> namesAndIndexes = new Dictionary<int, string>();

            for (int i = 0; i < parsedHeader.Length; i++)
            {
                string[] current = parsedHeader[i].Split(' ');


                if (!namesAndIndexes.ContainsKey(int.Parse(current[0])))
                {
                    namesAndIndexes.Add(int.Parse(current[0]), current[1]);
                }

            }


            byte[][] parsedData = new byte[][] { };

            Directory.SetCurrentDirectory(prevDir);


            for (int i = 0; i < parsed2.Length; i++)
            {
                Array.Resize(ref parsedData, parsedData.Length + 1);
                parsedData[i] = Encoding.GetEncoding(1251).GetBytes(parsed2[i]);
            }

            parsedData = parsedData.Where(f => f.Length > 0).ToArray();

            for (int i = 0; i < parsedData.Length; i++)
            {
                if (parsedData.Length == namesAndIndexes.Keys.ToArray().Length)
                {
                    if (!Directory.Exists("output")) { Directory.CreateDirectory("output"); }
                    File.WriteAllBytes(Path.Combine(dir.Name, namesAndIndexes[i]), parsedData[i]);
                }

            }

            Directory.SetCurrentDirectory(prevDir);
        }
    }
}
