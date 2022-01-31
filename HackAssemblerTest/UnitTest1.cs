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
        public void ConvertAddresses(string input, string expected)
        {
            Compiler compiler = new Compiler();

            Assert.AreEqual(expected, compiler.EncodeAddressLine(input))
        }
    }
}