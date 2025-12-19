using NUnit.Framework;
using Serilog.Configuration;
using Serilog.Tests.Support;
using System;

namespace Serilog.Tests
{
	[TestFixture]
	[Parallelizable]
	public class CorrelationIdLoggerConfigurationExtensionsTests
	{
		[Test]
		public void WithCorrelationId_ThenLoggerIsCalled_ShouldNotThrowException()
		{
			Core.Logger logger = new LoggerConfiguration()
				.Enrich.WithCorrelationId()
				.WriteTo.Sink(new DelegateSink.DelegatingSink(e => { }))
				.CreateLogger();

			Assert.DoesNotThrow(() => logger.Information("LOG"));
		}

		[Test]
		public void WithCorrelationId_WhenLoggerEnrichmentConfigurationIsNull_ShouldThrowArgumentNullException()
		{
			LoggerEnrichmentConfiguration configuration = null;
			Assert.Throws<ArgumentNullException>(() => configuration.WithCorrelationId());
		}

		[Test]
		public void WithCorrelationIdHeader_ThenLoggerIsCalled_ShouldNotThrowException()
		{
			Core.Logger logger = new LoggerConfiguration()
				.Enrich.WithCorrelationIdHeader()
				.WriteTo.Sink(new DelegateSink.DelegatingSink(e => { }))
				.CreateLogger();

			Assert.DoesNotThrow(() => logger.Information("LOG"));
		}

		[Test]
		public void WithCorrelationIdHeader_WhenLoggerEnrichmentConfigurationIsNull_ShouldThrowArgumentNullException()
		{
			LoggerEnrichmentConfiguration configuration = null;
			Assert.Throws<ArgumentNullException>(() => configuration.WithCorrelationIdHeader());
		}
	}
}
