using Microsoft.Extensions.DependencyInjection;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Enrichers;
using System;

namespace Serilog
{
	public static class CorrelationIdLoggerConfigurationExtensions
	{

		public static IServiceCollection WithCorrelationId(this IServiceCollection services)
		{
			services.AddSingleton<ILogEventEnricher, CorrelationIdEnricher>();
			return services;
		}

		public static IServiceCollection WithCorrelationIdHeader(this IServiceCollection services, string headerKey = "x-correlation-id")
		{
			services.AddSingleton<ILogEventEnricher>(sp => new CorrelationIdHeaderEnricher(headerKey));
			return services;
		}

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
