//<copyright> 3Shape A/S </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CustomAttribute.NCrunchTest
{
    [AttributeUsage(AttributeTargets.Method)]
    [XunitTestCaseDiscoverer(
        "CustomAttribute.NCrunchTest.CustomFactDiscoverer",
        "CustomAttribute.NCrunchTest")]
    public class CustomFactAttribute : FactAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    [XunitTestCaseDiscoverer(
        "CustomAttribute.NCrunchTest.CustomTheoryDiscoverer",
        "CustomAttribute.NCrunchTest")]
    public class CustomTheoryAttribute : TheoryAttribute
    {
    }

    public class CustomFactDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly FactDiscoverer _discoverer;

        public CustomFactDiscoverer(IMessageSink diagnosticMessageSink) =>
            _discoverer = new FactDiscoverer(diagnosticMessageSink);

        public IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute) =>
            _discoverer.Discover(discoveryOptions, testMethod, factAttribute)
                .Select(testCase => new CustomTestCase(testCase));
    }

    public class CustomTheoryDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly TheoryDiscoverer _discoverer;

        public CustomTheoryDiscoverer(IMessageSink diagnosticMessageSink) =>
            _discoverer = new TheoryDiscoverer(diagnosticMessageSink);

        public IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute) =>
            _discoverer.Discover(discoveryOptions, testMethod, factAttribute)
                .Select(testCase => new CustomTestCase(testCase));
    }

    public class CustomTestCase : LongLivedMarshalByRefObject, IXunitTestCase
    {
        private IXunitTestCase _testCase;

        public CustomTestCase(IXunitTestCase testCase) => _testCase = testCase;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", error: true)]
        public CustomTestCase()
        {
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public IMethodInfo Method => _testCase.Method;

        public Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) =>
            _testCase.RunAsync(
                diagnosticMessageSink, messageBus, constructorArguments, aggregator,
                cancellationTokenSource);

        public string DisplayName => _testCase.DisplayName;

        public string SkipReason => _testCase.SkipReason;

        public ISourceInformation SourceInformation
        {
            get => _testCase.SourceInformation;
            set => _testCase.SourceInformation = value;
        }

        public ITestMethod TestMethod => _testCase.TestMethod;

        public int Timeout => _testCase.Timeout;

#pragma warning disable CA1819 // Properties should not return arrays
        public object[] TestMethodArguments => _testCase.TestMethodArguments;
#pragma warning restore CA1819 // Properties should not return arrays

        public Dictionary<string, List<string>> Traits => _testCase.Traits;

        public string UniqueID => _testCase.UniqueID;

        public Exception InitializationException => _testCase.InitializationException;

        public void Deserialize(IXunitSerializationInfo info) => _testCase = info.GetValue<IXunitTestCase>("InnerTestCase");

        public void Serialize(IXunitSerializationInfo info) => info.AddValue("InnerTestCase", _testCase);
    }

    public class XUnitHelpersTest : IAsyncLifetime, IDisposable
    {
        private readonly Thread _thread;
        private readonly SynchronizationContext _context;

        public XUnitHelpersTest()
        {
            _context = SynchronizationContext.Current!;
            _thread = Thread.CurrentThread;
        }

        public Task InitializeAsync()
        {
            Assert.Same(_context, SynchronizationContext.Current);
            Assert.Same(_thread, Thread.CurrentThread);
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Assert.Same(_context, SynchronizationContext.Current);
            Assert.Same(_thread, Thread.CurrentThread);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Assert.Same(_context, SynchronizationContext.Current);
            Assert.Same(_thread, Thread.CurrentThread);
        }

        [CustomFact]
        public void FactTest() => Assert.Same(_thread, Thread.CurrentThread);

        [CustomTheory]
        [InlineData(42)]
        public Task TheoryTest(int testValue)
        {
            Assert.Same(_thread, Thread.CurrentThread);
            Assert.Equal(42, testValue);
            return Task.CompletedTask;
        }
    }
}
