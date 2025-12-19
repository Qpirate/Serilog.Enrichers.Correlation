using Serilog.Configuration;
using Serilog.Enrichers;
using System;

namespace Serilog
{
	public static class CorrelationIdLoggerConfigurationExtensions
	{
		public static LoggerConfiguration WithCorrelationId(this LoggerEnrichmentConfiguration enrichmentConfiguration)
		{
			return enrichmentConfiguration == null
				? throw new ArgumentNullException(nameof(enrichmentConfiguration))
				: enrichmentConfiguration.With<CorrelationIdEnricher>();
		}

		public static LoggerConfiguration WithCorrelationId(this LoggerEnrichmentConfiguration enrichmentConfiguration, string headerKey)
		{
			return enrichmentConfiguration == null
				? throw new ArgumentNullException(nameof(enrichmentConfiguration))
				: enrichmentConfiguration.With(new CorrelationIdEnricher(headerKey));
		}

		public static LoggerConfiguration WithCorrelationIdHeader(
			this LoggerEnrichmentConfiguration enrichmentConfiguration,
			string headerKey = "x-correlation-id")
		{
			return enrichmentConfiguration == null
				? throw new ArgumentNullException(nameof(enrichmentConfiguration))
				: enrichmentConfiguration.With(new CorrelationIdHeaderEnricher(headerKey));
		}
	}
}
