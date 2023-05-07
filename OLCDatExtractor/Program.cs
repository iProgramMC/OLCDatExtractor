using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OLCDatExtractor
{
    public class sEntry
    {
        public uint nID;
        public uint nFileSize;
        public uint nFileOffset;
        public byte[] data;
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("OLCDat Extractor - (c) 2018-2019 javidx9, (c) 2019 iProgramInCpp");
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: olcextract.exe [filename].olcdat");
                return;
            }
            try
            {
                Stream ifs = File.OpenRead(args[0]);

                Dictionary<string, sEntry> mapFiles = new Dictionary<string, sEntry>();

                // 1. Read Map
                uint nMapEntries;
                byte[] c = new byte[sizeof(uint)];
                ifs.Read(c, 0, sizeof(uint));
                nMapEntries = BitConverter.ToUInt32(c, 0);
                for(uint i = 0; i < nMapEntries; i++)
                {
                    c = new byte[sizeof(uint)];
                    int nFilePathSize = 0;
                    ifs.Read(c, 0, sizeof(int));
                    nFilePathSize = BitConverter.ToInt32(c, 0);

                    c = new byte[nFilePathSize];
                    ifs.Read(c, 0, nFilePathSize);
                    
                    string sFileName = Encoding.ASCII.GetString(c);

                    sEntry e = new sEntry();

                    c = new byte[sizeof(uint)];
                    ifs.Read(c, 0, sizeof(uint));
                    e.nID = BitConverter.ToUInt32(c, 0);

                    c = new byte[sizeof(uint)];
                    ifs.Read(c, 0, sizeof(uint));
                    e.nFileSize = BitConverter.ToUInt32(c, 0);

                    c = new byte[sizeof(uint)];
                    ifs.Read(c, 0, sizeof(uint));
                    e.nFileOffset = BitConverter.ToUInt32(c, 0);

                    mapFiles[sFileName] = e;
                }

                // 2. Read Data
                foreach(var e in mapFiles)
                {
                    e.Value.data = new byte[e.Value.nFileSize];
                    ifs.Seek(e.Value.nFileOffset, SeekOrigin.Begin);
                    ifs.Read(e.Value.data, 0, (int)e.Value.nFileSize);
                }

                // 3. Extract Data
                foreach (var e in mapFiles)
                {
                    string filename = e.Key;
                    string safeFilename = filename.Split('\\', '/').Last();
                    if (!Directory.Exists("output")) Directory.CreateDirectory("output");
                    File.WriteAllBytes("output/" + safeFilename, e.Value.data);
                }
                Console.WriteLine("Done!");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
