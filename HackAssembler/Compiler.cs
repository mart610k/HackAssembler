using System;

namespace HackAssembler
{
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

        /// <summary>
        /// Main entry point for processing all compling the code into a binary format for each line
        /// </summary>
        /// <param name="input">the instruction being sent in</param>
        /// <returns>the binary code it results in</returns>
        public string CompileCode(string input)
        {
            input =  StripSinglelineComment(input);


            return input;
        }


        /// <summary>
        /// Encodes the Address into an address range the machine code accepts
        /// </summary>
        /// <param name="input">address liked code</param>
        /// <returns>address in binary format</returns>
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

        /// <summary>
        /// Encode the given Input into machine code
        /// </summary>
        /// <param name="input">the statement to convert</param>
        /// <returns>returning machine code</returns>
        public string EncodeCInstruction(string input)
        {
            string ainstructionBinary = "0";
            string controlBitInstructions = "000000";

            string destcodeBinary = "000";

            int equalsignLocation = input.IndexOf("=");

            if(equalsignLocation != -1)
            {
                string destinations = input.Substring(0, equalsignLocation).ToLower();

                destcodeBinary = DecodeDestBits(destinations);

                string action = input.Substring(equalsignLocation + 1).ToLower();

                if (action.Contains("m"))
                {
                    ainstructionBinary = "1";
                }

                controlBitInstructions = DecodeControlBitsFromAction(action);
            }

            string bytestring = 
                ainstructionBinary + 
                controlBitInstructions +  
                destcodeBinary + 
                DecodeJumpInstruction(input);

            return bytestring.PadLeft(16,'1');
        }

        /// <summary>
        /// Decode the bits that are related to where the result should be stored
        /// </summary>
        /// <param name="destinations">input string containing the save location(s)</param>
        /// <returns>the bits required to save the result</returns>
        private string DecodeDestBits(string destinations)
        {
            byte destinationBytes = 0;

            if (destinations.Contains("a"))
            {
                destinationBytes = (byte)(destinationBytes + 4);
            }

            if (destinations.Contains("d"))
            {
                destinationBytes = (byte)(destinationBytes + 2);
            }

            if (destinations.Contains("m"))
            {
                destinationBytes = (byte)(destinationBytes + 1);

            }

            return Convert.ToString(destinationBytes, 2).PadLeft(3, '0');
        }

        /// <summary>
        /// Decodes the control bits to result in what action should be taken on the incoming data
        /// </summary>
        /// <param name="action">the action part is everything after the "=" sign</param>
        /// <returns>bits that controls that action</returns>
        private string DecodeControlBitsFromAction(string action)
        {

            string controlBitInstructions = "000000";

            switch (action)
            {
                case "0":
                    controlBitInstructions = "101010";
                    break;
                case "1":
                    controlBitInstructions = "111111";
                    break;
                case "-1":
                    controlBitInstructions = "111010";
                    break;
                case "d":
                    controlBitInstructions = "001100";
                    break;
                case "a":
                case "m":
                    controlBitInstructions = "110000";
                    break;
                case "!d":
                    controlBitInstructions = "001101";
                    break;
                case "!a":
                case "!m":
                    controlBitInstructions = "110001";
                    break;
                case "-d":
                    controlBitInstructions = "001111";
                    break;
                case "-a":
                case "-m":
                    controlBitInstructions = "110011";
                    break;
                case "d+1":
                    controlBitInstructions = "011111";
                    break;
                case "a+1":
                case "m+1":
                    controlBitInstructions = "110111";
                    break;
                case "d-1":
                    controlBitInstructions = "001110";
                    break;
                case "a-1":
                case "m-1":
                    controlBitInstructions = "110010";
                    break;
                case "d+a":
                case "d+m":
                    controlBitInstructions = "000010";
                    break;
                case "d-a":
                case "d-m":
                    controlBitInstructions = "010011";
                    break;
                case "a-d":
                case "m-d":
                    controlBitInstructions = "000111";
                    break;
                case "d&a":
                case "d&m":
                    controlBitInstructions = "000000";
                    break;
                case "d|a":
                case "d|m":
                    controlBitInstructions = "010101";
                    break;
            }

            return controlBitInstructions;
        }

        /// <summary>
        /// Decodes the assembly jump instruction
        /// </summary>
        /// <param name="input">the full input from the C instruction</param>
        /// <returns>bits related to jump instructions</returns>
        private string DecodeJumpInstruction(string input)
        {
            string jumpCodeBinary = "000";
            int simicolonIndex = input.IndexOf(";");

            if (simicolonIndex != -1)
            {
                string jumpInstruction = input.Substring(simicolonIndex + 1).Trim().ToLower();

                switch (jumpInstruction)
                {
                    case "jgt":
                        jumpCodeBinary = "001";
                        break;
                    case "jeq":
                        jumpCodeBinary = "010";
                        break;
                    case "jge":
                        jumpCodeBinary = "011";
                        break;
                    case "jlt":
                        jumpCodeBinary = "100";
                        break;
                    case "jne":
                        jumpCodeBinary = "101";
                        break;
                    case "jle":
                        jumpCodeBinary = "110";
                        break;
                    case "jmp":
                        jumpCodeBinary = "111";
                        break;
                    default:
                        jumpCodeBinary = "000";
                        break;
                }
            }

            return jumpCodeBinary;
        }
    }
}
