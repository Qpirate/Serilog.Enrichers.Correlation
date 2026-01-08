using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Serilog.Enrichers
{
	public class CorrelationIdEnricher : ILogEventEnricher
	{

		private readonly ConcurrentDictionary<string, string> _correlationIds = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private string _correlationId = null;
		private readonly string _headerKey;
		private readonly IHttpContextAccessor _contextAccessor;

		public CorrelationIdEnricher()
		{
		}

		internal CorrelationIdEnricher(string headerKey)
		{
			_headerKey = headerKey;
			_contextAccessor = new HttpContextAccessor();
		}

		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			_semaphore.Wait();
			try
			{

				string correlationId = GetCorrelationId(logEvent);

				logEvent.AddOrUpdateProperty(new("SpanId", new ScalarValue(logEvent.SpanId)));
				logEvent.AddOrUpdateProperty(new("TraceId", new ScalarValue(logEvent.TraceId)));
				logEvent.AddOrUpdateProperty(new("CorrelationId", new ScalarValue(correlationId)));
			}
			catch (Exception ex)
			{
				logEvent.AddOrUpdateProperty(new("CorrelationId", new ScalarValue(ex)));
			}
			finally
			{
				_semaphore.Release();
			}
		}

		private string GetCorrelationId(LogEvent logEvent)
		{
			string key = $"{logEvent.TraceId}-{logEvent.SpanId}";
			if (key != "-")
			{
				_correlationId = key;
			}

			if (string.IsNullOrWhiteSpace(_correlationId))
			{
				_correlationId = Guid.NewGuid().ToString();
			}

			_correlationIds.TryAdd(key, _correlationId);

			return _correlationId;
		}
	}
}
