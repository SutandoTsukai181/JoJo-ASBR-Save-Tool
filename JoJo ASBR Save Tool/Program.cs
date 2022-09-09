namespace JoJoASBRSaveTool
{
    using System.IO;
    using System.Reflection;
    using CommandLine;

    public class Program
    {
        [Verb("dec", HelpText = "Decrypt a save file.")]
        public class Decrypt
        {
            [Value(0, MetaName = "input", Required = true, HelpText = "Input save file path.")]
            public string InputPath { get; set; }

            [Value(1, MetaName = "output", Required = true, HelpText = "Output file path.")]
            public string OutputPath { get; set; }

            [Option("alt", Required = false, Default = false, HelpText = "Alternate save type. For files other than \"JOJOASB.S\" (\"BattleRecord\", etc.)")]
            public bool AltType { get; set; }
        }

        [Verb("enc", HelpText = "Encrypt a save file.")]
        public class Encrypt
        {
            [Value(0, MetaName = "input", Required = true, HelpText = "Input save file path.")]
            public string InputPath { get; set; }

            [Value(1, MetaName = "output", Required = true, HelpText = "Output file path.")]
            public string OutputPath { get; set; }

            [Option("alt", Required = false, HelpText = "Alternate save type. For files other than \"JOJOASB.S\" (\"BattleRecord\", etc.)")]
            public bool AltType { get; set; }
        }
        private static void WriteHeader()
        {
            Console.WriteLine(CommandLine.Text.HeadingInfo.Default);
            Console.WriteLine(CommandLine.Text.CopyrightInfo.Default);
            Console.WriteLine();
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Decrypt, Encrypt>(args)
                   .WithParsed<Decrypt>(opts =>
                   {
                       WriteHeader();

                       if (!File.Exists(opts.InputPath))
                       {
                           Console.WriteLine($"ERROR: \"{opts.InputPath}\" not found!!!!");
                           return;
                       }

                       Console.WriteLine($"Decrypting \"{opts.InputPath}\"...");

                       var inputFile = File.ReadAllBytes(opts.InputPath);

                       var seed = opts.AltType ? 0xB8 : 0x3F6CA177;
                       var outputFile = DecryptData(inputFile, (uint)inputFile.Length, (uint)seed);

                       var dataLength = outputFile.Length - 4;
                       if (opts.AltType)
                       {
                           dataLength += 4;

                           outputFile[dataLength - 1] = 0;
                           outputFile[dataLength - 2] = 0;
                           outputFile[dataLength - 3] = 0;
                           outputFile[dataLength - 4] = 0;
                       }

                       Console.WriteLine($"Writing \"{opts.OutputPath}\"...");
                       using (FileStream stream = new FileStream(opts.OutputPath, FileMode.Create))
                       {
                           stream.Write(outputFile, 0, dataLength);
                       }
                   })
                   .WithParsed<Encrypt>(opts =>
                   {
                       WriteHeader();

                       if (!File.Exists(opts.InputPath))
                       {
                           Console.WriteLine($"ERROR: \"{opts.InputPath}\" not found!!!!");
                           return;
                       }

                       Console.WriteLine($"Calculating hash for \"{opts.InputPath}\"...");

                       var inputFile = File.ReadAllBytes(opts.InputPath);
                       var hash = CalculateHash(inputFile, (uint)inputFile.Length);

                       byte[] outputFile;
                       if (opts.AltType)
                       {
                           outputFile = inputFile;
                       }
                       else
                       {
                           outputFile = new byte[inputFile.Length + 4];
                           Array.Copy(inputFile, outputFile, inputFile.Length);
                       }

                       outputFile[^1] = (byte)(hash >> 24);
                       outputFile[^2] = (byte)(hash >> 16);
                       outputFile[^3] = (byte)(hash >> 8);
                       outputFile[^4] = (byte)hash;

                       Console.WriteLine($"Encrypting \"{opts.InputPath}\"...");

                       var seed = opts.AltType ? 0xB8 : 0x3F6CA177;
                       outputFile = DecryptData(outputFile, (uint)outputFile.Length, (uint)seed);

                       Console.WriteLine($"Writing \"{opts.OutputPath}\"...");
                       using (FileStream stream = new FileStream(opts.OutputPath, FileMode.Create))
                       {
                           stream.Write(outputFile, 0, outputFile.Length);
                       }
                   });

            Console.WriteLine("DONE!");
        }

        public static byte[] DecryptData(byte[] data, uint size, uint seed)
        {
            var pos = 0;

            uint v1 = seed;
            v1 = v1 / 32 ^ v1 * 0x1da597;
            uint v2 = v1 / 32 + 0x85c9c2 ^ v1 * 0x1da597;
            uint v3 = v2 / 32 + 0x10b9384 ^ v2 * 0x1da597;
            uint v4 = v3 / 32 + 0x1915d46 ^ v3 * 0x1da597;

            do
            {
                v1 = v1 * 2048 ^ v1;
                uint v5 = v4 ^ ((v4 / 2048 ^ v1) / 256) ^ v1;
                var v6Arr = new byte[] { (byte)v5, (byte)(v5 >> 8), (byte)(v5 >> 16), (byte)(v5 >> 24) };

                uint v7 = Math.Min(4, size);
                for (var i = 0; i < v7; i++)
                {
                    data[pos + i] = (byte)(data[pos + i] ^ v6Arr[i]);
                }

                size -= v7;
                pos += 4;

                v1 = v2;
                v2 = v3;
                v3 = v4;
                v4 = v5;
            } while (size > 0);

            return data;
        }

        public static uint CalculateHash(byte[] data, uint size)
        {
            var hashTable = new uint[256];

            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.hash_table.bin"))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                for (int i = 0; i < hashTable.Length; i++)
                {
                    hashTable[i] = reader.ReadUInt32();
                }
            }

            uint result = 0xFFFFFFFF;
            for (int i = 0; i < size; i++)
            {
                uint hashIndex = data[i] ^ (result >> 0x18);
                result = hashTable[Math.Clamp((int)hashIndex, 0, 255)] ^ (result << 8);
            }

            return ~result;
        }
    }
}
