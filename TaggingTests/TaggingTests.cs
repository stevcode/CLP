using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CLP.Models;

namespace TaggingTests
{
    [TestClass]
    public class TaggingTests
    {
        /*[TestMethod]
        public void InstantiateCorrectnessTag()
        {
            //TagType correctness = new CorrectnessTagType();
            TagType correctness = CorrectnessTagType.Instance;
        }*/


        [TestMethod]
        public void InstantiateTags()
        {
            TagType div = ArrayDivisionCorrectnessTagType.Instance;
            TagType horiz = ArrayHorizontalDivisionsTagType.Instance;
            TagType orient = ArrayOrientationTagType.Instance;
            TagType strat = ArrayStrategyTagType.Instance;
            TagType vert = ArrayVerticalDivisionsTagType.Instance;
            TagType correct = CorrectnessTagType.Instance;
            TagType domain = DomainInterpretationTagType.Instance;
            TagType pagedef = PageDefinitionTagType.Instance;
            TagType topic = PageTopicTagType.Instance;
            TagType star = StarredTagType.Instance;
        }
    }
}
