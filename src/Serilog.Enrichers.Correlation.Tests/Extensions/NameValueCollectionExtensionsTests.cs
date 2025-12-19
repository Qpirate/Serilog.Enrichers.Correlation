using NUnit.Framework;
using NUnit.Framework.Legacy;
using Serilog.Enrichers.Correlation.Extensions;
using System.Collections.Specialized;

namespace Serilog.Tests.Extensions
{
	[TestFixture]
	[Parallelizable]
	public class NameValueCollectionExtensionsTests
	{
		[Test]
		public void TryGetValue_ReturnsTrue_WhenKeyIsFound()
		{
			NameValueCollection collection = new NameValueCollection { { "MyKey", "MyValue" } };

			ClassicAssert.IsTrue(collection.TryGetValue("MyKey", out _));
		}

		[Test]
		public void TryGetValue_ReturnsFalse_WhenKeyIsNotFound()
		{
			NameValueCollection collection = new NameValueCollection { { "MyKey", "MyValue" } };

			ClassicAssert.IsFalse(collection.TryGetValue("BadKey", out _));
		}

		[Test]
		public void TryGetValue_SetsValues_WhenKeyIsFound()
		{
			NameValueCollection collection = new NameValueCollection { { "MyKey", "MyValue" } };

			collection.TryGetValue("MyKey", out System.Collections.Generic.IEnumerable<string> values);

			ClassicAssert.AreEqual(new[] { "MyValue" }, values);
		}

		[Test]
		public void TryGetValue_SetsValuesToNull_WhenKeyIsNotFound()
		{
			NameValueCollection collection = new NameValueCollection { { "MyKey", "MyValue" } };

			collection.TryGetValue("BadKey", out System.Collections.Generic.IEnumerable<string> values);

			ClassicAssert.IsNull(values);
		}
	}
}
