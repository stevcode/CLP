using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CLP.Models;

namespace TaggingTests
{
    [TestClass]
    public class TaggingTests
    {
        [TestMethod]
        public void InstantiateTags()
        {
            TagType correctness = CorrectnessTagType.Instance;
            TagType strategy = ArrayStrategyTagType.Instance;
        }
    }
}
