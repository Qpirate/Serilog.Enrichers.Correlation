using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Linq;
using System.Threading;

namespace Serilog.Enrichers
{
	public class CorrelationIdEnricher : ILogEventEnricher
	{

		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private string _correlationId = null;
		private readonly string _headerKey;
		private readonly IHttpContextAccessor _contextAccessor;
		private const string CorrelationIdPropertyName = "CorrelationId";

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

				string correlationId = GetCorrelationId();

				LogEventProperty correlationIdProperty = new(CorrelationIdPropertyName, new ScalarValue(correlationId));

				logEvent.AddOrUpdateProperty(correlationIdProperty);
			}
			catch (Exception ex)
			{
				LogEventProperty exceptionProperty = new(CorrelationIdPropertyName, new ScalarValue(ex));
				logEvent.AddOrUpdateProperty(exceptionProperty);
			}
			finally
			{
				_semaphore.Release();
			}
		}

		private string GetCorrelationId()
		{

			if (_contextAccessor?.HttpContext is not null && _contextAccessor.HttpContext.Request.Headers.TryGetValue(_headerKey, out Microsoft.Extensions.Primitives.StringValues strings))
			{
				_correlationId ??= strings.FirstOrDefault();
			}


			if (string.IsNullOrWhiteSpace(_correlationId))
			{
				_correlationId = Guid.NewGuid().ToString();
			}
			return _correlationId;
		}
	}
}
