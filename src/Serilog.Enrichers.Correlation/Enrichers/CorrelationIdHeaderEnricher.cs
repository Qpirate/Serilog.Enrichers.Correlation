using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Linq;

namespace Serilog.Enrichers
{
	public class CorrelationIdHeaderEnricher : ILogEventEnricher
	{
		private const string CorrelationIdPropertyName = "CorrelationId";
		private readonly string _headerKey;
		private readonly IHttpContextAccessor _contextAccessor;

		public CorrelationIdHeaderEnricher(string headerKey) : this(headerKey, new HttpContextAccessor())
		{
		}

		public CorrelationIdHeaderEnricher(string headerKey, IHttpContextAccessor contextAccessor)
		{
			_headerKey = headerKey;
			_contextAccessor = contextAccessor;
		}

		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			if (_contextAccessor.HttpContext == null)
				return;

			string correlationId = GetCorrelationId(logEvent);

			logEvent.AddOrUpdateProperty(new LogEventProperty(CorrelationIdPropertyName, new ScalarValue(correlationId)));
		}

		private string GetCorrelationId(LogEvent logEvent)
		{
			string header = string.Empty;

			if (_contextAccessor.HttpContext.Request.Headers.TryGetValue(_headerKey, out Microsoft.Extensions.Primitives.StringValues values))
			{
				header = values.FirstOrDefault();
			}
			else if (_contextAccessor.HttpContext.Response.Headers.TryGetValue(_headerKey, out values))
			{
				header = values.FirstOrDefault();
			}

			string key = $"{logEvent.TraceId}-{logEvent.SpanId}";

			string correlationId = string.IsNullOrEmpty(header)
									? key
									: header;

			if (!_contextAccessor.HttpContext.Response.Headers.ContainsKey(_headerKey) && !_contextAccessor.HttpContext.Response.Headers.IsReadOnly)
			{
				_contextAccessor.HttpContext.Response.Headers.Add(_headerKey, correlationId);
			}

			return correlationId;
		}
	}
}
