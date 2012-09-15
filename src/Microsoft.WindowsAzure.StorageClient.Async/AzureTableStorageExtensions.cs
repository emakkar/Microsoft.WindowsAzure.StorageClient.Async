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
	using System.Threading.Tasks;

	public static class AzureTableStorageExtensions {
		/// <summary>
		/// Creates the specified table.
		/// </summary>
		/// <param name="client">The table storage client.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>A task that completes when the async operation is finished.</returns>
		public static Task CreateTableAsync(this CloudTableClient client, string tableName) {
			return Task.Factory.FromAsync(
				(cb, state) => ((Tuple<CloudTableClient, string>)state).Item1.BeginCreateTable(((Tuple<CloudTableClient, string>)state).Item2, cb, state),
				ar => ((Tuple<CloudTableClient, string>)ar.AsyncState).Item1.EndCreateTable(ar),
				Tuple.Create(client, tableName));
		}

		/// <summary>
		/// Creates the specified table if it does not already exist.
		/// </summary>
		/// <param name="client">The table storage client.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>A task that completes when the async operation is finished.</returns>
		public static Task CreateTableIfNotExistAsync(this CloudTableClient client, string tableName) {
			return Task.Factory.FromAsync(
				(cb, state) => ((Tuple<CloudTableClient, string>)state).Item1.BeginCreateTableIfNotExist(((Tuple<CloudTableClient, string>)state).Item2, cb, state),
				ar => ((Tuple<CloudTableClient, string>)ar.AsyncState).Item1.EndCreateTableIfNotExist(ar),
				Tuple.Create(client, tableName));
		}

		public static Task SaveChangesAsync(this TableServiceContext tableServiceContext) {
			return Task.Factory.FromAsync(
				(cb, state) => ((TableServiceContext)state).BeginSaveChanges(cb, state),
				ar => ((TableServiceContext)ar.AsyncState).EndSaveChanges(ar),
				tableServiceContext);
		}

		public static async Task<ReadOnlyCollection<T>> ExecuteAsync<T>(this CloudTableQuery<T> query, IProgress<IEnumerable<T>> progress = null) {
			var results = new List<T>();
			ResultSegment<T> resultSegment;
			ResultContinuation continuation = null;
			do {
				resultSegment = await Task.Factory.FromAsync(
					(cb, state) => query.BeginExecuteSegmented(continuation, cb, state),
					ar => query.EndExecuteSegmented(ar),
					null);
				if (progress != null) {
					progress.Report(resultSegment.Results);
				}

				results.AddRange(resultSegment.Results);
				continuation = resultSegment.ContinuationToken;
			} while (resultSegment.HasMoreResults);

			return new ReadOnlyCollection<T>(results);
		}
	}
}
