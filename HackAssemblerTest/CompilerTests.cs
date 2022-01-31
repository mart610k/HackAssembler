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
    }
}