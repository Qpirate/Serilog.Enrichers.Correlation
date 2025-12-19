using Serilog.Core;
using Serilog.Events;
using System.Threading;

namespace Serilog.Enrichers
{
    public class CorrelationIdEnricher : ILogEventEnricher
    {

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private string _correlationId = null;
        private const string CorrelationIdPropertyName = "CorrelationId";

        public CorrelationIdEnricher()
        {
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
            finally
            {
                _semaphore.Release();
            }
        }

        private string GetCorrelationId()
        {
            _correlationId ??= System.Guid.NewGuid().ToString();
            return _correlationId;
        }
    }
}
