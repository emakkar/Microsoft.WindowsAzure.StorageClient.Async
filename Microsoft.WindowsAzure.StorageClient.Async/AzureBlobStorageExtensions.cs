//-----------------------------------------------------------------------
// <copyright file="AzureBlobStorageExtensions.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.WindowsAzure.StorageClient {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Threading.Tasks;
#if NET40
	using ReadOnlyListOfBlobItem = System.Collections.ObjectModel.ReadOnlyCollection<IListBlobItem>;
#else
	using ReadOnlyListOfBlobItem = System.Collections.Generic.IReadOnlyList<IListBlobItem>;
#endif

	public static class AzureBlobStorageExtensions {
		public static Task<bool> CreateIfNotExistAsync(this CloudBlobContainer container) {
			return Task.Factory.FromAsync(
				(cb, state) => ((CloudBlobContainer)state).BeginCreateIfNotExist(cb, state),
				ar => ((CloudBlobContainer)ar.AsyncState).EndCreateIfNotExist(ar),
				container);
		}

		public static async Task<ReadOnlyListOfBlobItem> ListBlobsSegmentedAsync(this CloudBlobContainer container, int pageSize, BlobRequestOptions options = null, IProgress<IEnumerable<IListBlobItem>> progress = null) {
			options = options ?? new BlobRequestOptions();
			var results = new List<IListBlobItem>();
			ResultContinuation continuation = null;
			ResultSegment<IListBlobItem> segment;
			do {
				segment = await Task.Factory.FromAsync(
					(cb, state) => container.BeginListBlobsSegmented(pageSize, continuation, options, cb, state),
					ar => container.EndListBlobsSegmented(ar),
					null);
				if (progress != null) {
					progress.Report(segment.Results);
				}
				results.AddRange(segment.Results);
				continuation = segment.ContinuationToken;
			} while (segment.HasMoreResults);

			return new ReadOnlyCollection<IListBlobItem>(results);
		}

		public static async Task<ReadOnlyListOfBlobItem> ListBlobsSegmentedAsync(this CloudBlobDirectory directory, int pageSize, BlobRequestOptions options = null, IProgress<IEnumerable<IListBlobItem>> progress = null) {
			options = options ?? new BlobRequestOptions();
			var results = new List<IListBlobItem>();
			ResultContinuation continuation = null;
			ResultSegment<IListBlobItem> segment;
			do {
				segment = await Task.Factory.FromAsync(
					(cb, state) => directory.BeginListBlobsSegmented(pageSize, continuation, options, cb, state),
					ar => directory.EndListBlobsSegmented(ar),
					null);
				if (progress != null) {
					progress.Report(segment.Results);
				}
				results.AddRange(segment.Results);
			} while (segment.HasMoreResults);

			return new ReadOnlyCollection<IListBlobItem>(results);
		}

		public static Task DownloadToStreamAsync(this CloudBlob blob, Stream stream) {
			return Task.Factory.FromAsync(
				(cb, state) => ((Tuple<CloudBlob, Stream>)state).Item1.BeginDownloadToStream(((Tuple<CloudBlob, Stream>)state).Item2, cb, state),
				ar => ((Tuple<CloudBlob, Stream>)ar.AsyncState).Item1.EndDownloadToStream(ar),
				Tuple.Create(blob, stream));
		}

		public static Task UploadFromStreamAsync(this CloudBlob blob, Stream stream) {
			return Task.Factory.FromAsync(
				(cb, state) => ((Tuple<CloudBlob, Stream>)state).Item1.BeginUploadFromStream(((Tuple<CloudBlob, Stream>)state).Item2, cb, state),
				ar => ((Tuple<CloudBlob, Stream>)ar.AsyncState).Item1.EndUploadFromStream(ar),
				Tuple.Create(blob, stream));
		}

		public static Task DeleteAsync(this CloudBlob blob) {
			return Task.Factory.FromAsync(
				(cb, state) => ((CloudBlob)state).BeginDelete(cb, state),
				ar => ((CloudBlob)ar.AsyncState).EndDelete(ar),
				blob);
		}

		public static Task<bool> DeleteIfExistsAsync(this CloudBlob blob) {
			return Task.Factory.FromAsync(
				(cb, state) => ((CloudBlob)state).BeginDeleteIfExists(cb, state),
				ar => ((CloudBlob)ar.AsyncState).EndDeleteIfExists(ar),
				blob);
		}

		public static Task SetMetadataAsync(this CloudBlob blob) {
			return Task.Factory.FromAsync(
				(cb, state) => ((CloudBlob)state).BeginSetMetadata(cb, state),
				ar => ((CloudBlob)ar.AsyncState).EndSetMetadata(ar),
				blob);
		}

		public static Task FetchAttributesAsync(this CloudBlob blob) {
			return Task.Factory.FromAsync(
				(cb, state) => ((CloudBlob)state).BeginFetchAttributes(cb, state),
				ar => ((CloudBlob)ar.AsyncState).EndFetchAttributes(ar),
				blob);
		}

		public static Task SetPermissionsAsync(this CloudBlobContainer container, BlobContainerPermissions permissions, BlobRequestOptions options = null) {
			return Task.Factory.FromAsync(
				(cb, state) => container.BeginSetPermissions(permissions, options, cb, state),
				ar => container.EndSetPermissions(ar),
				null);
		}
	}
}
