using NUnit.Framework;
using NUnit.Framework.Legacy;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Tests.Support;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Serilog.Tests.Enrichers
{
    [TestFixture]
    [Parallelizable]
    public class CorrelationIdEnricherTests
    {
        [SetUp]
        public void SetUp()
        {
            _enricher = new CorrelationIdEnricher();
        }

        private CorrelationIdEnricher _enricher;

        [Test]
        public void When_CurrentHttpContextIsNotNull_Should_CreateCorrelationIdProperty()
        {
            LogEvent evt = null;
            Core.Logger log = new LoggerConfiguration()
                .Enrich.With(_enricher)
                .WriteTo.Sink(new DelegateSink.DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information(@"Has a CorrelationId property");

            ClassicAssert.NotNull(evt);
            ClassicAssert.IsTrue(evt.Properties.ContainsKey("CorrelationId"));
            ClassicAssert.NotNull(evt.Properties["CorrelationId"].LiteralValue());
        }

        [Test]
        public void When_MultipleLoggingCallsMade_Should_KeepUsingCreatedCorrelationIdProperty()
        {
            LogEvent evt = null;
            Core.Logger log = new LoggerConfiguration()
                .Enrich.With(_enricher)
                .WriteTo.Sink(new DelegateSink.DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information(@"Has a CorrelationId property");

            object correlationId = evt.Properties["CorrelationId"].LiteralValue();

            log.Information(@"Here is another event");

            ClassicAssert.AreEqual(correlationId, evt.Properties["CorrelationId"].LiteralValue());
        }


        [Test]
        public async Task When_MultipleLoggingCallsMade_WithAsync_Should_KeepUsingCreatedCorrelationIdProperty()
        {
            List<LogEvent> evt = new List<LogEvent>();
            Core.Logger log = new LoggerConfiguration()
                .Enrich.With(_enricher)
                .WriteTo.Sink(new DelegateSink.DelegatingSink(e => evt.Add(e)))
                .CreateLogger();

            var tasks = Enumerable.Range(0, 10).Select((s) => Task.Run(() =>
            {
                log.Information(@"Has a CorrelationId property: {id}", s);
            }));

            await Task.WhenAll(tasks);

            ClassicAssert.AreEqual(evt.Select(s => s.Properties["CorrelationId"]).Distinct().Count(), 1);

        }
    }
}
