using NUnit.Framework;
using HackAssembler;

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

            Assert.AreEqual(0,compiler.StripSinglelineComment(input).Length);
        }
        
        [TestCase("@1 //StripOnlyThisPart","@1")]
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
        [TestCase("M=0;JMP","111")]
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

        [TestCase("0","000")]
        [TestCase("M=0","001")]
        [TestCase("D=0","010")]
        [TestCase("MD=0","011")]
        [TestCase("A=0","100")]
        [TestCase("AM=0","101")]
        [TestCase("AD=0","110")]
        [TestCase("AMD=0","111")]
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

        [TestCase("0=0", "0","101010")]
        [TestCase("0=1", "0","111111")]
        [TestCase("0=-1", "0","111010")]
        [TestCase("0=D", "0","001100")]
        [TestCase("0=A", "0","110000")]
        [TestCase("0=M","1","110000")]
        [TestCase("0=!D", "0", "001101")]
        [TestCase("0=!A", "0","110001")]
        [TestCase("0=!M", "1","110001")]
        [TestCase("0=-D", "0", "001111")]
        [TestCase("0=-A", "0","110011")]
        [TestCase("0=-M", "1","110011")]
        [TestCase("0=D+1", "0", "011111")]
        [TestCase("0=A+1", "0","110111")]
        [TestCase("0=M+1", "1","110111")]
        [TestCase("0=D-1", "0", "001110")]
        [TestCase("0=A-1", "0","110010")]
        [TestCase("0=M-1", "1","110010")]
        [TestCase("0=D+A", "0","000010")]
        [TestCase("0=D+M", "1","000010")]
        [TestCase("0=D-A", "0","010011")]
        [TestCase("0=D-M", "1","010011")]
        [TestCase("0=A-D", "0","000111")]
        [TestCase("0=M-D", "1","000111")]
        [TestCase("0=D&A", "0","000000")]
        [TestCase("0=D&M", "1","000000")]
        [TestCase("0=D|A", "0","010101")]
        [TestCase("0=D|M", "1","010101")]
        public void CheckABitInstructionCode(string instruction, string expectedABit, string expectedInstruction)
        {
            Compiler compiler = new Compiler();

            string encodedEnstruction = compiler.EncodeCInstruction(instruction);

            if (encodedEnstruction.Length != 16)
            {
                Assert.Fail("output was not 16 characters long");
            }

            string actualABit = encodedEnstruction.Substring(3,1);

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

        

    }
}