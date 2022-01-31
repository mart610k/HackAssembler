using System;
using System.IO;

namespace HackAssembler
{
    class Program
    {
        static void Main(string[] args)
        {
            Compiler compiler = new Compiler();
            Console.WriteLine("Hello World!");
            FileStream fileStream = File.OpenRead("C:\\Notes\\nand2tetris\\projects\\06\\add\\Add.asm");
            StreamReader streamReader = new StreamReader(fileStream);
            while (!streamReader.EndOfStream)
            {
                string result = compiler.CompileCode(streamReader.ReadLine());
                if (result.Length != 0)
                {
                    Console.WriteLine(result);

                }
            }
        }
    }

   public class Compiler
    {
        public string StripSinglelineComment(string input)
        {
            int index = input.IndexOf("//");
            if(index != -1)
            {
                input = input.Substring(0, index).Trim();
            }
            return input;
        }
        public string CompileCode(string input)
        {
            input =  StripSinglelineComment(input);


            return input;
        }

        public string EncodeAddressLine(string input)
        {

        }

    }
}
