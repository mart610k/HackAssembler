using NUnit.Framework;
using HackAssembler;
using System.Collections.Generic;

namespace HackAssemblerTest
{
    public class CompilerTests
    {

        [TestCase("//TestComment")]
        [TestCase("// Another comment")]
        [TestCase("//Comment//WitinComment")]
        [TestCase("")]
        public void CommentsStrippedFullLine(string input)
        {
            Compiler compiler = new Compiler();

            Assert.AreEqual(0, compiler.StripSinglelineComment(input).Length);
        }

        [TestCase("@1 //StripOnlyThisPart", "@1")]
        [TestCase("D=A //Comment here", "D=A")]
        public void CommentStripOnlyComment(string input, string expected)
        {
            Compiler compiler = new Compiler();

            Assert.AreEqual(expected, compiler.StripSinglelineComment(input));
        }

        [TestCase("@1", "0000000000000001")]
        [TestCase("@0", "0000000000000000")]
        [TestCase("@32767", "0111111111111111")]
        [TestCase("@16", "0000000000010000")]
        public void ConvertAddressesSuccessfull(string input, string expected)
        {
            Compiler compiler = new Compiler();

            Assert.AreEqual(expected, compiler.EncodeAddressLine(input));
        }

        [TestCase("@32768")]
        [TestCase("@32999")]
        [TestCase("@-1")]
        public void TryingToConvertIlligalAddressThrowsException(string input)
        {
            Compiler compiler = new Compiler();

            Assert.Throws<MemoryOutOfRangeException>(() => compiler.EncodeAddressLine(input));
        }

        [TestCase("M=0", "000")]
        [TestCase("M=0;JGT", "001")]
        [TestCase("M=0;JEQ", "010")]
        [TestCase("M=0;JGE", "011")]
        [TestCase("M=0;JLT", "100")]
        [TestCase("M=0;JNE", "101")]
        [TestCase("M=0;JLE", "110")]
        [TestCase("M=0;JMP", "111")]
        public void ConvertingJumpCInstruction(string input, string expectedbytes)
        {

            Compiler compiler = new Compiler();

            string returned = compiler.EncodeCInstruction(input);

            if (returned.Length != 16)
            {
                Assert.Fail("output was not 16 characters long");
            }

            string byteCodes = returned.Substring(13);

            Assert.AreEqual(expectedbytes, byteCodes);
        }

        [TestCase("0", "000")]
        [TestCase("M=0", "001")]
        [TestCase("D=0", "010")]
        [TestCase("MD=0", "011")]
        [TestCase("A=0", "100")]
        [TestCase("AM=0", "101")]
        [TestCase("AD=0", "110")]
        [TestCase("AMD=0", "111")]
        public void ConvertDestinationCInstructionTest(string input, string expectedbytes)
        {
            Compiler compiler = new Compiler();

            string returned = compiler.EncodeCInstruction(input);

            if (returned.Length != 16)
            {
                Assert.Fail("output was not 16 characters long");
            }

            string byteCodes = returned.Substring(10, 3);

            Assert.AreEqual(expectedbytes, byteCodes);
        }

        [TestCase("0=0", "0", "101010")]
        [TestCase("0=1", "0", "111111")]
        [TestCase("0=-1", "0", "111010")]
        [TestCase("0=D", "0", "001100")]
        [TestCase("D", "0", "001100")]
        [TestCase("0=A", "0", "110000")]
        [TestCase("0=M", "1", "110000")]
        [TestCase("0=!D", "0", "001101")]
        [TestCase("0=!A", "0", "110001")]
        [TestCase("0=!M", "1", "110001")]
        [TestCase("0=-D", "0", "001111")]
        [TestCase("0=-A", "0", "110011")]
        [TestCase("0=-M", "1", "110011")]
        [TestCase("0=D+1", "0", "011111")]
        [TestCase("0=A+1", "0", "110111")]
        [TestCase("0=M+1", "1", "110111")]
        [TestCase("0=D-1", "0", "001110")]
        [TestCase("0=A-1", "0", "110010")]
        [TestCase("0=M-1", "1", "110010")]
        [TestCase("0=D+A", "0", "000010")]
        [TestCase("0=D+M", "1", "000010")]
        [TestCase("0=D-A", "0", "010011")]
        [TestCase("0=D-M", "1", "010011")]
        [TestCase("0=A-D", "0", "000111")]
        [TestCase("0=M-D", "1", "000111")]
        [TestCase("0=D&A", "0", "000000")]
        [TestCase("0=D&M", "1", "000000")]
        [TestCase("0=D|A", "0", "010101")]
        [TestCase("0=D|M", "1", "010101")]
        public void CheckABitInstructionCode(string instruction, string expectedABit, string expectedInstruction)
        {
            Compiler compiler = new Compiler();

            string encodedEnstruction = compiler.EncodeCInstruction(instruction);

            if (encodedEnstruction.Length != 16)
            {
                Assert.Fail("output was not 16 characters long");
            }

            string actualABit = encodedEnstruction.Substring(3, 1);

            string actualInstruction = encodedEnstruction.Substring(4, 6);

            Assert.AreEqual(expectedABit, actualABit);
            Assert.AreEqual(expectedInstruction, actualInstruction);

        }


        [TestCase("A=M")]
        [TestCase("A=-1")]
        [TestCase("M=0")]
        [TestCase("AMD=1")]
        [TestCase("D=M+1")]
        [TestCase("M=M+1")]
        [TestCase("M=!A")]
        [TestCase("0=D")]

        public void StartingCEnstructionsPrefixedBits(string input)
        {
            Compiler compiler = new Compiler();

            string encodedEnstruction = compiler.EncodeCInstruction(input);

            if (encodedEnstruction.Length != 16)
            {
                Assert.Fail("output was not 16 characters long");
            }

            string startingbits = encodedEnstruction.Substring(0, 3);

            Assert.AreEqual("111", startingbits);
        }


        [TestCase("A=1 //Comment", "1110111111100000")]
        [TestCase("@2 //DefaultLocation Memory", "0000000000000010")]
        [TestCase("@100 //Very Important Location", "0000000001100100")]
        [TestCase("AMD=0", "1110101010111000")]
        [TestCase("@162", "0000000010100010")]
        [TestCase("0;JMP", "1110101010000111")]
        [TestCase("//EMPTYLine should return no bits", "")]
        [TestCase("", "")]

        public void SampleInstructionsEncodedCorrectlySomeComments(string input, string expectedBits)
        {
            Compiler compiler = new Compiler();

            string actualByteCode = compiler.CompileSingleLineCode(input);

            Assert.AreEqual(expectedBits, actualByteCode);
        }

        [Test]
        public void RemoveAllEmptyLinesAndComments()
        {
            List<string> codeLines = new List<string>() { "         //Starting comment", "//Another starting comment", "", "          @0 //AddressLine With comment", "D=M-1", "@510 //Address location for stats", "//This will not do much but is a comment", "A=M", "0; JMP //End of the program" };
            List<string> expectedLines = new List<string>() { "@0", "D=M-1", "@510", "A=M", "0; JMP" };

            Compiler compiler = new Compiler();


            List<string> actualLines = compiler.StripCommentsAndEmptyLines(codeLines);


            Assert.AreEqual(expectedLines, actualLines);

        }

        [Test]
        public void ParseLabelsFromAssembly()
        {
            List<string> codeLines = new List<string>() { "@0", "(START)", "D=M-1", "@510", "A=M", "@START", "A; JEQ", "@2", "(DOSOMETHING)", "@41", "A=A-1; JGT ", "(END)", "@END", "0; JMP" };
            List<string> expectedLines = new List<string>() { "@0", "D=M-1", "@510", "A=M", "@START", "A; JEQ", "@2", "@41", "A=A-1; JGT ", "@END", "0; JMP" };

            Dictionary<string, int> expectedlabels = new Dictionary<string, int>() { ["START"] = 1, ["DOSOMETHING"] = 7, ["END"] = 9 };

            Compiler compiler = new Compiler();

            List<string> actualLines = compiler.ParseLabelsInCode(codeLines);

            Assert.AreEqual(expectedlabels, compiler.UserDefinedLabels);
            Assert.AreEqual(expectedLines, actualLines);

        }

        [TestCase("@LABEL", "@4")]
        [TestCase("@START", "@25")]
        [TestCase("@END", "@61")]
        [TestCase("@BEGINING", "@91")]
        [TestCase("@COMPARE", "@81")]
        [TestCase("@NEGATE", "@14")]
        [TestCase("@BABAISYOU", "@50")]
        [TestCase("@LABELNOTEXISING", "@LABELNOTEXISING")]
        public void ParseLabelAddressesAndConvertThem(string input, string expected)
        {
            Compiler compiler = new Compiler(new Dictionary<string, int>() { ["LABEL"] = 4, ["START"] = 25, ["END"] = 61, ["BEGINING"] = 91, ["COMPARE"] = 81, ["NEGATE"] = 14, ["BABAISYOU"] = 50 });

            string actual = compiler.ConvertAddressLineLabelToAddress(input);

            Assert.AreEqual(expected, actual);

        }


        [TestCase("@Address", "Address")]
        [TestCase("@Test", "Test")]
        [TestCase("@Mess", "Mess")]
        [TestCase("@i", "i")]
        [TestCase("@ADWE", "ADWE")]
        [TestCase("@Teama", "Teama")]
        [TestCase("@Scorpio", "Scorpio")]
        public void RegisterSingleAddressAsValue(string input, string expected)
        {
            Compiler compiler = new Compiler();

            compiler.RegisterAddressLabel(input);

            Dictionary<string, int> actual = compiler.AddressLabels;

            Assert.True(actual.ContainsKey(expected));
        }


        [Test]
        public void CheckAllExpectedDeterminedAddresValues()
        {
            Compiler compiler = new Compiler();


            Dictionary<string, int> preDeterminedAddressValues = new Dictionary<string, int>();
            preDeterminedAddressValues.Add("R0", 0);
            preDeterminedAddressValues.Add("R1", 1);
            preDeterminedAddressValues.Add("R2", 2);
            preDeterminedAddressValues.Add("R3", 3);
            preDeterminedAddressValues.Add("R4", 4);
            preDeterminedAddressValues.Add("R5", 5);
            preDeterminedAddressValues.Add("R6", 6);
            preDeterminedAddressValues.Add("R7", 7);
            preDeterminedAddressValues.Add("R8", 8);
            preDeterminedAddressValues.Add("R9", 9);
            preDeterminedAddressValues.Add("R10", 10);
            preDeterminedAddressValues.Add("R11", 11);
            preDeterminedAddressValues.Add("R12", 12);
            preDeterminedAddressValues.Add("R13", 13);
            preDeterminedAddressValues.Add("R14", 14);
            preDeterminedAddressValues.Add("R15", 15);
            preDeterminedAddressValues.Add("SP", 0);
            preDeterminedAddressValues.Add("LCL", 1);
            preDeterminedAddressValues.Add("ARG", 2);
            preDeterminedAddressValues.Add("THIS", 3);
            preDeterminedAddressValues.Add("THAT", 4);
            preDeterminedAddressValues.Add("SCREEN", 16384);
            preDeterminedAddressValues.Add("KEYBOARD", 24576);

            foreach (string key in preDeterminedAddressValues.Keys)
            {

                Assert.AreEqual(preDeterminedAddressValues[key], compiler.PreDefinedAddressLabels[key]);
            }
        }

        [TestCase("R0", 0)]
        [TestCase("R1", 1)]
        [TestCase("R2", 2)]
        [TestCase("R3", 3)]
        [TestCase("R4", 4)]
        [TestCase("R5", 5)]
        [TestCase("R6", 6)]
        [TestCase("R7", 7)]
        [TestCase("R8", 8)]
        [TestCase("R9", 9)]
        [TestCase("R10", 10)]
        [TestCase("R11", 11)]
        [TestCase("R12", 12)]
        [TestCase("R13", 13)]
        [TestCase("R14", 14)]
        [TestCase("R15", 15)]
        [TestCase("SP", 0)]
        [TestCase("LCL", 1)]
        [TestCase("ARG", 2)]
        [TestCase("THIS", 3)]
        [TestCase("THAT", 4)]
        [TestCase("SCREEN", 16384)]
        [TestCase("KEYBOARD", 24576)]
        public void TestIndivdualPredeterminedAddresses(string name, int expectedAddress)
        {
            Compiler compiler = new Compiler();

            Assert.AreEqual(expectedAddress, compiler.PreDefinedAddressLabels[name]);
        }


        [Test]
        public void IngoreAddressedEncoded()
        {
            List<string> codeWithAddresses = new List<string>()
            {
                "@91",
                "@22",
                "@51",
                "@42",
                "@11"
            };

            Compiler compiler = new Compiler();

            for (int i = 0; i < codeWithAddresses.Count; i++)
            {
                compiler.RegisterCustomAddressLabel(codeWithAddresses[i]);
            }

            Assert.False(compiler.AddressLabels.ContainsKey("91"));
            Assert.False(compiler.AddressLabels.ContainsKey("22"));
            Assert.False(compiler.AddressLabels.ContainsKey("51"));
            Assert.False(compiler.AddressLabels.ContainsKey("42"));
            Assert.False(compiler.AddressLabels.ContainsKey("11"));
        } 


        [Test]
        public void RegisterMultipleAddressesConfirmLocations()
        {
            List<string> codeWithAddresses = new List<string>()
            {
                "@i",
                "@Test",
                "@SYMBOL",
                "@TEST",
                "@R0"
            };

            Compiler compiler = new Compiler();

            for (int i = 0; i < codeWithAddresses.Count; i++)
            {
                compiler.RegisterCustomAddressLabel(codeWithAddresses[i]);
            }


            Assert.AreEqual(16, compiler.AddressLabels["i"]);
            Assert.AreEqual(17, compiler.AddressLabels["Test"]);
            Assert.AreEqual(18, compiler.AddressLabels["SYMBOL"]);
            Assert.AreEqual(19, compiler.AddressLabels["TEST"]);
            Assert.False(compiler.AddressLabels.ContainsKey("R0"));

        }

        [Test]
        public void EncodeCustomAddressIgnoreAddressesEncoded()
        {
            List<string> codeWithAddresses = new List<string>()
            {
                "@42",
                "@12",
                "@42",
                "@52",
                "@12",
                "@16384",
                "D=-1",
                "A=0"
            };
            Compiler compiler = new Compiler();

            for (int i = 0; i < codeWithAddresses.Count; i++)
            {
                compiler.RegisterCustomAddressLabel(codeWithAddresses[i]);
            }

            List<string> expectedAddress = new List<string>()
            {
                "@42",
                "@12",
                "@42",
                "@52",
                "@12",
                "@16384",
                "D=-1",
                "A=0"
            };

            for (int i = 0; i < expectedAddress.Count; i++)
            {
                Assert.AreEqual(expectedAddress[i], compiler.ConvertAddressVariablesToAddress(codeWithAddresses[i]));
            }
        }

        [Test]
        public void EncodeCustomAddresses()
        {
            List<string> codeWithAddresses = new List<string>()
            {
                "@i",
                "@Test",
                "@SYMBOL",
                "@TEST",
                "@R0",
                "@SCREEN",
                "D=-1",
                "A=0"
            };
            Compiler compiler = new Compiler();

            for (int i = 0; i < codeWithAddresses.Count; i++)
            {
                compiler.RegisterCustomAddressLabel(codeWithAddresses[i]);
            }

            List<string> expectedAddress = new List<string>()
            {
                "@16",
                "@17",
                "@18",
                "@19",
                "@0",
                "@16384",
                "D=-1",
                "A=0"
            };

            for (int i = 0; i < expectedAddress.Count; i++)
            {
                Assert.AreEqual(expectedAddress[i], compiler.ConvertAddressVariablesToAddress(codeWithAddresses[i]));
            }
        }



        [Test]
        public void CompileFullCodeWithCodeLabels()
        {
            List<string> codeLines = new List<string>()
            {
                "            ",
                "       //comment",
                "@AddressTest //Test",
                "AMD=0 // Comment",
                "//AMD=-1",
                "@SCREEN",
                "0; JMP",
                "D=-1",
                "A=0",
                "@TESTValue",
                "(LABELTEST) //test",
                "@52",
                "@LABELTEST"
            };

            List<string> expectedBinaryCode = new List<string>()
            {
                "0000000000010000",
                "1110101010111000",
                "0100000000000000",
                "1110101010000111",
                "1110111010010000",
                "1110101010100000",
                "0000000000010001",
                "0000000000110100",
                "0000000000000111"
            };


            Compiler compiler = new Compiler();

            List<string> result = compiler.CompileCode(codeLines);

            Assert.AreEqual(expectedBinaryCode.Count, result.Count);

            for (int i = 0; i < expectedBinaryCode.Count; i++)
            {
                Assert.AreEqual(expectedBinaryCode[i], result[i]);
            }
        }
    }
}