namespace Microsoft.WindowsAzure.StorageClient {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using Microsoft.WindowsAzure.Storage;

	internal static class InternalExtensions {
		internal static IAsyncResult WithCancellation(this ICancellableAsyncResult asyncResult, CancellationToken cancellationToken = default(CancellationToken)) {
			cancellationToken.Register(ar => ((ICancellableAsyncResult)ar).Cancel(), asyncResult, false);
			return asyncResult;
		}
	}
}
