using Cactus.Fileserver.ImageResizer;
using ImageResizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cactus.Fileserver.Tests
{
    [TestClass]
    public class InstructionsExtensionsTest
    {
        [TestMethod]
        public void InstarctionsJoinTest()
        {
            var incomeInstruction = new Instructions
            {
                Alpha = 25,
                Blur = .5
            };

            var defInstructions = new Instructions
            {
                Alpha = 50,
                BorderColor = "red"
            };

            var mandatoryInstructions = new Instructions
            {
                Contrast = 25,
                Blur = .8
            };

            incomeInstruction.Join(defInstructions);
            Assert.AreEqual(25,incomeInstruction.Alpha);
            Assert.AreEqual(.5, incomeInstruction.Blur);
            Assert.AreEqual("red", incomeInstruction.BorderColor);

            incomeInstruction.Join(mandatoryInstructions,true);
            Assert.AreEqual(25, incomeInstruction.Alpha);
            Assert.AreEqual(.8, incomeInstruction.Blur);
            Assert.AreEqual("red", incomeInstruction.BorderColor);
            Assert.AreEqual(25, incomeInstruction.Contrast);
        }
    }
}
