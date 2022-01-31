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
        /// <summary>
        /// Strips away single line comments from the code. wether or not they are located in the same line as an instruction or are exclusively for that line.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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
            int index = input.IndexOf("@");

            if (index != -1)
            {
                string substringAddress = input.Substring(index + 1);
                string convertedaddress;
                try
                {
                    //Int might be too big for this as its 16 bits long at most
                    int parsedAddress =  int.Parse(substringAddress);
                    //Addres space is only working within positive space
                    if(parsedAddress > 32767 || parsedAddress < 0)
                    {
                        throw new MemoryOutOfRangeException(string.Format("The address at '{0}' is illigal in type of system",parsedAddress));
                    }

                    convertedaddress = Convert.ToString(parsedAddress, 2).PadLeft(16, '0');
                }
                catch (Exception)
                {

                    throw;
                }
                return convertedaddress;
            }
            return input;
        }
    }
}
