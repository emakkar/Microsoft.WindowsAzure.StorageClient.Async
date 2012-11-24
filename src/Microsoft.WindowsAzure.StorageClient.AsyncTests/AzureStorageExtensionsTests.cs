namespace Microsoft.WindowsAzure.StorageClient.AsyncTests {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Microsoft.WindowsAzure.Storage;
	using Microsoft.WindowsAzure.Storage.Blob;
	using NUnit.Framework;

	[TestFixture]
	public class AzureStorageExtensionsTests {
		private string testContainerName;
		private CloudStorageAccount account;
		private CloudBlobClient blobClient;
		private CloudBlobContainer blobContainer;

		[SetUp]
		public void Initialize() {
			this.account = CloudStorageAccount.DevelopmentStorageAccount;

			this.blobClient = this.account.CreateCloudBlobClient();
			this.testContainerName = "unittests" + Guid.NewGuid().ToString();
			this.blobContainer = this.blobClient.GetContainerReference(this.testContainerName);
			this.blobContainer.CreateIfNotExistAsync().GetAwaiter().GetResult();
		}

		[TearDown]
		public void TearDown() {
			this.blobContainer.Delete();
		}

		[Test]
		public void CreateIfNotExistAsyncAsync() {
			Assert.IsFalse(this.blobContainer.CreateIfNotExistAsync().GetAwaiter().GetResult());
		}

		[Test]
		public void UploadFromStreamAndDownloadToStreamAsync() {
			const string payload = "Some message";
			string blobName = GetRandomBlobName();
			var blob = this.blobContainer.GetBlockBlobReference(blobName);

			var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
			blob.UploadFromStreamAsync(sourceStream).Wait();

			var targetStream = new MemoryStream();
			blob.DownloadToStreamAsync(targetStream).Wait();

			CollectionAssert.AreEqual(sourceStream.ToArray(), targetStream.ToArray());
		}

		[Test]
		public void ListBlobsSegmentedAsync() {
			var progress = new Progress<IEnumerable<IListBlobItem>>(
				results => {
				});
			this.blobContainer.ListBlobsSegmentedAsync(progress).GetAwaiter().GetResult();
		}

		[Test]
		public void DeleteAsync() {
			var blob = this.CreateTestBlobAsync().Result;
			blob.DeleteAsync().GetAwaiter().GetResult();
		}

		[Test]
		public void DeleteIfExistsAsync() {
			var blob = this.CreateTestBlobAsync().Result;
			Assert.That(blob.DeleteIfExistsAsync().GetAwaiter().GetResult(), Is.True);
			Assert.That(blob.DeleteIfExistsAsync().GetAwaiter().GetResult(), Is.False);
		}

		[Test]
		public void SetMetadataAsyncAndFetchAttributesAsync() {
			var blob = this.CreateTestBlobAsync().Result;
			blob.Metadata["someKey"] = "someValue";
			blob.SetMetadataAsync().GetAwaiter().GetResult();

			blob = this.blobContainer.GetBlockBlobReference(blob.Name);
			blob.FetchAttributesAsync().GetAwaiter().GetResult();
			Assert.That(blob.Metadata["someKey"], Is.EqualTo("someValue"));
		}

		[Test]
		public void SetPermissionsAsync() {
			this.blobContainer.SetPermissionsAsync(new BlobContainerPermissions()).Wait();
		}

		private static string GetRandomBlobName() {
			var random = new Random();
			var buffer = new byte[8];
			random.NextBytes(buffer);
			string name = Convert.ToBase64String(buffer);
			return name;
		}

		private static string ConfigSetter(string configName) {
			string value = ConfigurationManager.AppSettings[configName];
			if (String.IsNullOrEmpty(value)) {
				value = ConfigurationManager.ConnectionStrings[configName].ConnectionString;
			}

			return value;
		}

		private async Task<ICloudBlob> CreateTestBlobAsync() {
			const string payload = "Some message";
			string blobName = GetRandomBlobName();
			var blob = this.blobContainer.GetBlockBlobReference(blobName);
			var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
			await blob.UploadFromStreamAsync(sourceStream);
			return blob;
		}
	}
}
