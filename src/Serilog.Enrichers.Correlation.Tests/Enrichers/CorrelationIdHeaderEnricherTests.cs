using FakeItEasy;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Tests.Support;

namespace Serilog.Tests.Enrichers
{
    [TestFixture]
    [Parallelizable]
    public class CorrelationIdHeaderEnricherTests
    {
        private const string HeaderKey = "x-correlation-id";

        [SetUp]
        public void SetUp()
        {
            _httpContextAccessor = A.Fake<IHttpContextAccessor>();
            _enricher = new CorrelationIdHeaderEnricher(HeaderKey, _httpContextAccessor);
        }

        private IHttpContextAccessor _httpContextAccessor;
        private CorrelationIdHeaderEnricher _enricher;

        [Test]
        public void When_CorrelationIdNotInHeader_Should_CreateCorrelationIdProperty()
        {
            A.CallTo(() => _httpContextAccessor.HttpContext)
                .Returns(new DefaultHttpContext());

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
        public void When_CorrelationIdIsInHeader_Should_ExtractCorrelationIdFromHeader()
        {
            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers[HeaderKey] = "my-correlation-id";

            A.CallTo(() => _httpContextAccessor.HttpContext)
                .Returns(httpContext);

            LogEvent evt = null;
            Core.Logger log = new LoggerConfiguration()
                .Enrich.With(_enricher)
                .WriteTo.Sink(new DelegateSink.DelegatingSink(e => evt = e))
                .CreateLogger();


            log.Information(@"Has a CorrelationId property");

            ClassicAssert.NotNull(evt);
            ClassicAssert.IsTrue(evt.Properties.ContainsKey("CorrelationId"));
            ClassicAssert.AreEqual(evt.Properties["CorrelationId"].LiteralValue(), "my-correlation-id");
        }

        [Test]
        public void When_CurrentHttpContextIsNull_ShouldNot_CreateCorrelationIdProperty()
        {
            A.CallTo(() => _httpContextAccessor.HttpContext)
                .Returns(null);

            LogEvent evt = null;
            Core.Logger log = new LoggerConfiguration()
                .Enrich.With(_enricher)
                .WriteTo.Sink(new DelegateSink.DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information(@"Does not have a CorrelationId property");

            ClassicAssert.NotNull(evt);
            ClassicAssert.IsFalse(evt.Properties.ContainsKey("CorrelationId"));
        }

        [Test]
        public void When_MultipleLoggingCallsMade_Should_KeepUsingCreatedCorrelationIdProperty()
        {
            DefaultHttpContext httpContext = new();

            A.CallTo(() => _httpContextAccessor.HttpContext)
                .Returns(httpContext);

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
    }
}
