//-----------------------------------------------------------------------
// <copyright file="AzureStorageExtensions.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.WindowsAzure.StorageClient {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;

	public static class AzureStorageExtensions {
		public static Task<bool> CreateIfNotExistAsync(this CloudBlobContainer container) {
			return Task.Factory.FromAsync(
				(cb, state) => ((CloudBlobContainer)state).BeginCreateIfNotExist(cb, state),
				ar => ((CloudBlobContainer)ar.AsyncState).EndCreateIfNotExist(ar),
				container);
		}

		public static async Task ListBlobsSegmentedAsync(this CloudBlobContainer container, int pageSize, IProgress<IEnumerable<IListBlobItem>> progress) {
			var options = new BlobRequestOptions { BlobListingDetails = BlobListingDetails.Metadata };
			ResultContinuation continuation = null;
			ResultSegment<IListBlobItem> segment;
			do {
				segment = await Task.Factory.FromAsync(
					(cb, state) => container.BeginListBlobsSegmented(pageSize, continuation, options, cb, state),
					ar => container.EndListBlobsSegmented(ar),
					null);
				progress.Report(segment.Results);
			} while (segment.HasMoreResults);
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
