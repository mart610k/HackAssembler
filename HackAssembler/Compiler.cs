using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HackAssembler
{
    public class Compiler
    {
        public Dictionary<string, int> UserDefinedLabels { get; private set; }

        public Dictionary<string, int> PreDefinedAddressLabels { get; private set; }

        public Dictionary<string, int> AddressLabels { get; set; }

        int CurrentCustomAddress = 16;

        public Compiler()
        {
            UserDefinedLabels = new Dictionary<string, int>();
            AddressLabels = new Dictionary<string, int>();
            PreDefinedAddressLabels = new Dictionary<string, int>();
            RegisterDeterminedAddressValues();
        }

        /// <summary>
        /// Primaily meant for testing to set the Dictionary into the the right state
        /// </summary>
        /// <param name="predeterminedLabels"></param>
        public Compiler(Dictionary<string, int> predeterminedLabels)
        {
            UserDefinedLabels = predeterminedLabels;
            AddressLabels = new Dictionary<string, int>();
            PreDefinedAddressLabels = new Dictionary<string, int>();
            RegisterDeterminedAddressValues();
        }

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
        public string CompileSingleLineCode(string input)
        {
            string instructionBytes;

            input = StripSinglelineComment(input);
            if(input.Length == 0)
            {
                return "";
            }

            if (input.StartsWith("@"))
            {
                instructionBytes = EncodeAddressLine(input);
            }
            else
            {
                instructionBytes = EncodeCInstruction(input);
            }

            return instructionBytes;
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
            string controlBitInstructions = "101010";

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
        /// Parses the labels and strips the labels out from the assembly code saving labels and ROM addresses as a dictionary
        /// </summary>
        /// <param name="codeLines">the codelines to process</param>
        /// <returns>codelines input with the labels stripped out</returns>
        public List<string> ParseLabelsInCode(List<string> codeLines)
        {
            List<string> returningCode = new List<string>();
            int currentROMAddress = 0;
            for (int i = 0; i < codeLines.Count; i++)
            {
                if (codeLines[i].StartsWith("("))
                {
                    string parsedLabelName = codeLines[i].Trim(new char[] { '(', ')' });
                    UserDefinedLabels.Add(parsedLabelName, currentROMAddress);
                }
                else
                {
                    currentROMAddress++;
                    returningCode.Add(codeLines[i]);
                }
            }
            return returningCode;
        }

        /// <summary>
        /// Converts a Label address into a real numeric address
        /// </summary>
        /// <param name="input">line to convert</param>
        /// <returns>The address based instruction</returns>
        public string ConvertAddressLineLabelToAddress(string input)
        {
            if (!input.StartsWith("@"))
            {
                return input;
            }
            if (!Regex.IsMatch(input, "@^[0-9]*$"))
            {
                string label = input.Substring(1);
                if (UserDefinedLabels.ContainsKey(label))
                {
                    return  "@" +  UserDefinedLabels[label];
                }
            }

            return input;
        }

        /// <summary>
        /// Strips all comments, whitespaces and empty lines from the incoming code leaving only the assmby code 
        /// </summary>
        /// <param name="codeLines">The code lines to strip</param>
        /// <returns>Resulting list over what instructions should be run through.</returns>
        public List<string> StripCommentsAndEmptyLines(List<string> codeLines)
        {
            List<string> remainingLines = new List<string>();
            for (int i = 0; i < codeLines.Count; i++)
            {
                string strippedComment = StripSinglelineComment(codeLines[i]);
                strippedComment = strippedComment.Trim();

                if(strippedComment.Length != 0)
                {
                    remainingLines.Add(strippedComment);
                }
            }

            return remainingLines;
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

        public void RegisterAddressLabel(string input)
        {
            int lowestIntFound = 0;

            if(input.StartsWith("@")){
                input = input.Substring(1);
                if (!AddressLabels.ContainsKey(input))
                {

                    List<int> values = AddressLabels.Values.ToList();
                    values.Sort();

                    foreach (int item in values)
                    {
                        if(lowestIntFound <= item || (lowestIntFound <= item && lowestIntFound == item - 1))
                        {
                            lowestIntFound = item;
                        }
                        else
                        {
                            break;
                        }
                    }
                    AddressLabels.Add(input, lowestIntFound);
                }

            }
            


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

        private void RegisterDeterminedAddressValues()
        {

            PreDefinedAddressLabels.Add("R0", 0);
            PreDefinedAddressLabels.Add("R1", 1);
            PreDefinedAddressLabels.Add("R2", 2);
            PreDefinedAddressLabels.Add("R3", 3);
            PreDefinedAddressLabels.Add("R4", 4);
            PreDefinedAddressLabels.Add("R5", 5);
            PreDefinedAddressLabels.Add("R6", 6);
            PreDefinedAddressLabels.Add("R7", 7);
            PreDefinedAddressLabels.Add("R8", 8);
            PreDefinedAddressLabels.Add("R9", 9);
            PreDefinedAddressLabels.Add("R10", 10);
            PreDefinedAddressLabels.Add("R11", 11);
            PreDefinedAddressLabels.Add("R12", 12);
            PreDefinedAddressLabels.Add("R13", 13);
            PreDefinedAddressLabels.Add("R14", 14);
            PreDefinedAddressLabels.Add("R15", 15);
            PreDefinedAddressLabels.Add("SP", 0);
            PreDefinedAddressLabels.Add("LCL", 1);
            PreDefinedAddressLabels.Add("ARG", 2);
            PreDefinedAddressLabels.Add("THIS", 3);
            PreDefinedAddressLabels.Add("THAT", 4);
            PreDefinedAddressLabels.Add("SCREEN", 16384);
            PreDefinedAddressLabels.Add("KEYBOARD", 24576);

        }

        public string ConvertAddressVariablesToAddress(string v)
        {
            if (v.StartsWith("@"))
            {
                string temp = v.Substring(1);
                if(!Regex.IsMatch(temp, "^[0-9]*$"))
                {

                    if (PreDefinedAddressLabels.ContainsKey(temp))
                    {
                        return "@" + PreDefinedAddressLabels[temp];
                    }
                    return "@" + AddressLabels[temp];
                }
            }
            return v;
        }

        public void RegisterCustomAddressLabel(string v)
        {
            if (v.StartsWith("@"))
            {
                v = v.Substring(1);
                if (!Regex.IsMatch(v, "^[0-9]*$"))
                {


                    if (!PreDefinedAddressLabels.ContainsKey(v))
                    {
                        if (!AddressLabels.ContainsKey(v))
                        {
                            AddressLabels.Add(v,CurrentCustomAddress);
                            CurrentCustomAddress++;
                        }
                    }
                }
            }
        
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

        public List<string> CompileCode(List<string> codeLines)
        {
            codeLines = StripCommentsAndEmptyLines(codeLines);

            codeLines = ParseLabelsInCode(codeLines);

            codeLines = ConvertAllAddressLineLabelsToAddresses(codeLines);

            codeLines = ConvertAllAddressLabelsToAddressPointers(codeLines);
           
            return ConvertRawCodeToBinary(codeLines);
        }

        private List<string> ConvertAllAddressLineLabelsToAddresses(List<string> codeLines)
        {
            List<string> convertedLines = new List<string>();


            for (int i = 0; i < codeLines.Count; i++)
            {
                convertedLines.Add(ConvertAddressLineLabelToAddress(codeLines[i]));
            }

            return convertedLines;
        }

        private List<string> ConvertAllAddressLabelsToAddressPointers(List<string> codeLines)
        {
            List<string> convertedLines = new List<string>();

            for (int i = 0; i < codeLines.Count; i++)
            {
                RegisterCustomAddressLabel(codeLines[i]);
            }

            for (int i = 0; i < codeLines.Count; i++)
            {
                convertedLines.Add(ConvertAddressVariablesToAddress(codeLines[i]));
            }
            return convertedLines;
        }

        private List<string> ConvertRawCodeToBinary(List<string> codeLines)
        {
            List<string> convertedLines = new List<string>();

            for (int i = 0; i < codeLines.Count; i++)
            {
                convertedLines.Add(CompileSingleLineCode(codeLines[i]));
            }

            return convertedLines;
        }
    }
}
