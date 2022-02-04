using System;
using System.Collections.Generic;
using System.IO;

namespace HackAssembler
{
    class Program
    {
        static void Main(string[] args)
        {
            Compiler compiler = new Compiler();
            Console.WriteLine("Hello World!");
            ReadAssemblyAndConvertToBinaryFile(@"C:\Notes\nand2tetris\projects\06\pong\Pong.asm", @"C:\Notes\nand2tetris\projects\06\pong\Pong.hack");


        }
        static void ReadAssemblyAndConvertToBinaryFile(string sourceFile, string destFile)
        {
            Compiler compiler = new Compiler();

            List<string> resultBinary = compiler.CompileCode(FetchLinesFromFile(sourceFile));

            WriteLinesToFile(destFile, resultBinary);

        }


        static List<string> FetchLinesFromFile(string filepath)
        {
            List<string> codeLines = new List<string>();
            using (FileStream fs = File.OpenRead(filepath))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    while (!reader.EndOfStream)
                    {
                        codeLines.Add(reader.ReadLine());
                    }
                }
            }
            return codeLines;
        }

        static void WriteLinesToFile(string fullpath,List<string> linesToWrite)
        {
            using (FileStream fs = File.OpenWrite(fullpath))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    for (int i = 0; i < linesToWrite.Count; i++)
                    {
                        writer.WriteLine(linesToWrite[i]);
                    }
                }
            }
        }
    }
}
