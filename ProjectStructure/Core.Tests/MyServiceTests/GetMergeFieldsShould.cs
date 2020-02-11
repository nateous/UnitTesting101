using System.Collections.Generic;
using NUnit.Framework;

namespace Awesome
{
    public class GetMergeFieldsShould
    {
        private CommunicationMergeFieldService _service;

        [SetUp]
        public void Setup()
        {
            _service = new CommunicationMergeFieldService(new Mock<IMergeFieldDataService>().Object);
        }

        [Test]
        public void ReturnOneGivenOneMergeField()
        {
            string template = "{{asdf}}";

            int expected = 1;

            int actual = _service.GetMergeFields(template).Count();

            Assert.AreEqual(expected, actual);
        }

        [TestCase("", 0)]
        [TestCase("{{asdf}}", 1)]
        [TestCase("{{asdf}} }}", 1)]
        [TestCase("{{ {{asdf}}", 1)]
        [TestCase("{{{{asdf}}}}", 1)]
        [TestCase("adsfasdf{{asdf}}asdfasdf", 1)]
        [TestCase("{{  asdf}}", 1)]
        [TestCase("{{asdf}} {{qwerweqr}}", 2)]
        [TestCase("{{  asdf}}{{qwerqwer}}", 2)]
        public void ReturnCorrectCountGivenTestTemplates(string template, int expected)
        {
            int actual = _service.GetMergeFields(template).Count();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnCorrectNamesGivenTemplate()
        {
            string template = "{{one}} three {{two}}";
            Dictionary<string, string> actual = _service.GetMergeFields(template);
            CollectionAssert.Contains(actual.Keys, "{{one}}");
            CollectionAssert.Contains(actual.Keys, "{{two}}");
            CollectionAssert.DoesNotContain(actual.Keys, "{{three}}");
        }

        [Test]
        public void NotBlowUpGivenNulls()
        {
            string template = "{{asdf}}";
            var actual = _service.GetMergeFields(template, null);
            CollectionAssert.Contains(actual.Keys, "{{asdf}}");
        }

        [Test]
        public void NotBlowUpGivenNullList()
        {
            var actual = _service.GetMergeFields(null);
            CollectionAssert.IsEmpty(actual);
        }
    }
}
