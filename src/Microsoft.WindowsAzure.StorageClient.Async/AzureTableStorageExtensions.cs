//-----------------------------------------------------------------------
// <copyright file="AzureTableStorageExtensions.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.WindowsAzure.StorageClient {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Microsoft.WindowsAzure.Storage;
	using Microsoft.WindowsAzure.Storage.Table;
	using Microsoft.WindowsAzure.Storage.Table.DataServices;

	public static class AzureTableStorageExtensions {
		/// <summary>
		/// Creates the specified table.
		/// </summary>
		/// <param name="client">The table storage client.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>A task that completes when the async operation is finished.</returns>
		public static Task CreateTableAsync(this CloudTable client, string tableName, CancellationToken cancellationToken = default(CancellationToken)) {
			return Task.Factory.FromAsync(
				(cb, state) => ((CloudTable)state).BeginCreate(cb, state).WithCancellation(cancellationToken),
				ar => ((CloudTable)ar.AsyncState).EndCreate(ar),
				client);
		}

		/// <summary>
		/// Creates the specified table if it does not already exist.
		/// </summary>
		/// <param name="client">The table storage client.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>A task that completes when the async operation is finished.</returns>
		public static Task CreateIfNotExistsAsync(this CloudTable client, CancellationToken cancellationToken = default(CancellationToken)) {
			return Task.Factory.FromAsync(
				(cb, state) => ((CloudTable)state).BeginCreateIfNotExists(cb, state).WithCancellation(cancellationToken),
				ar => ((CloudTable)ar.AsyncState).EndCreateIfNotExists(ar),
				client);
		}

		public static Task SaveChangesAsync(this TableServiceContext tableServiceContext) {
			return Task.Factory.FromAsync(
				(cb, state) => ((TableServiceContext)state).BeginSaveChanges(cb, state),
				ar => ((TableServiceContext)ar.AsyncState).EndSaveChanges(ar),
				tableServiceContext);
		}

		public static async Task<ReadOnlyCollection<T>> ExecuteQuerySegmentedAsync<T>(this CloudTable table, TableQuery<T> query, IProgress<List<DynamicTableEntity>> progress = null)
			where T : ITableEntity, new() {
			TableQuerySegment<DynamicTableEntity> resultSegment;
			TableContinuationToken continuation = null;
			var results = new List<T>();
			do {
				resultSegment = await Task.Factory.FromAsync(
					(cb, state) => (IAsyncResult)table.BeginExecuteQuerySegmented(query, continuation, cb, state),
					ar => table.EndExecuteQuerySegmented(ar),
					null);
				results.AddRange(resultSegment.Results.Cast<T>());
				if (progress != null) {
					progress.Report(resultSegment.Results);
				}

				continuation = resultSegment.ContinuationToken;
			} while (continuation != null);

			return new ReadOnlyCollection<T>(results);
		}

		public static async Task<ReadOnlyCollection<T>> ExecuteSegmentedAsync<T>(this TableServiceQuery<T> query, IProgress<List<T>> progress = null, CancellationToken cancellationToken = default(CancellationToken)) {
			TableQuerySegment<T> resultSegment;
			TableContinuationToken continuation = null;
			var results = new List<T>();
			do {
				resultSegment = await Task.Factory.FromAsync(
					(cb, state) => query.BeginExecuteSegmented(continuation, cb, state).WithCancellation(cancellationToken),
					ar => query.EndExecuteSegmented(ar),
					null);
				results.AddRange(resultSegment.Results);
				if (progress != null) {
					progress.Report(resultSegment.Results);
				}

				continuation = resultSegment.ContinuationToken;
			} while (continuation != null);

			return new ReadOnlyCollection<T>(results);
		}
	}
}
