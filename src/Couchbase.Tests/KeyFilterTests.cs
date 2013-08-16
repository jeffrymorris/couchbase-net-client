using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Couchbase.Diagnostics;
using NUnit.Framework;

namespace Couchbase.Tests
{
    [TestFixture]
    public class KeyFilterTests
    {
        [Test]
        public void Test_That_ShouldTrace_Returns_True_When_Data_And_Message_Are_Same()
        {
            const string pattern = "10-contentid";
            const string message = pattern;
            var filter = new KeyFilter(pattern);
            Assert.IsTrue(filter.ShouldTrace(new TraceEventCache(), "source", TraceEventType.Information, 1, message, null, null, null));
        }

        [Test]
        public void Test_That_ShouldTrace_Returns_False_When_Data_And_Message_Are_Not_Same()
        {
            const string pattern = "10-contentid";
            const string message = "11-contentid";
            var filter = new KeyFilter(pattern);
            Assert.IsFalse(filter.ShouldTrace(new TraceEventCache(), "source", TraceEventType.Information, 1, message, null, null, null));
        }

        [Test]
        public void Test_That_ShouldTrace_Returns_True_When_Key_Matches_Regex()
        {
            const string pattern = "^([0-9]*-[a-zA-Z]*)+$";
            const string message = "11-contentid";
            var filter = new KeyFilter(pattern);
            Assert.IsTrue(filter.ShouldTrace(new TraceEventCache(), "source", TraceEventType.Information, 1, message, null, null, null));
        }

        [Test]
        public void Test_That_ShouldTrace_Returns_False_When_Key_Does_Not_Match_Regex()
        {
            const string pattern = "^([0-9]*-[a-zA-Z]*)+$";
            const string message = "11contentid";
            var filter = new KeyFilter(pattern);
            Assert.IsFalse(filter.ShouldTrace(new TraceEventCache(), "source", TraceEventType.Information, 1, message, null, null, null));
        }

        [Test]
        public void Test_That_ShouldTrace_Returns_False_When_Key_Does_Not_Match_Regex2()
        {
            const string pattern = "^([0-9]*-[a-zA-Z]*)+$";
            const string message = "aa-contentid";
            var filter = new KeyFilter(pattern);
            Assert.IsFalse(filter.ShouldTrace(new TraceEventCache(), "source", TraceEventType.Information, 1, message, null, null, null));
        }

        [Test]
        public void Test_That_ShouldTrace_Returns_False_When_Key_Does_Not_Match_Regex3()
        {
            const string pattern = "^([0-9]*-[a-zA-Z]*)+$";
            const string message = "contentid-11";
            var filter = new KeyFilter(pattern);
            Assert.IsFalse(filter.ShouldTrace(new TraceEventCache(), "source", TraceEventType.Information, 1, message, null, null, null));
        }

        [Test]
        public void Test_That_ShouldTrace_Throws_Not_Supported_Exception_When_TraceEventType_Is_Not_Information()
        {
            const TraceEventType notSupportedTraceEventType = TraceEventType.Error;
            const string pattern = "10-contentid";
            const string message = pattern;
            var filter = new KeyFilter(pattern);
            Assert.Throws<NotSupportedException>(() => filter.ShouldTrace(new TraceEventCache(), "source", notSupportedTraceEventType, 1, message, null, null, null));
        }
    }
}
