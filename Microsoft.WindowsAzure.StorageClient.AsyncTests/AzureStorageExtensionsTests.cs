namespace Microsoft.WindowsAzure.StorageClient.AsyncTests {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using NUnit.Framework;

	[TestFixture]
	public class AzureStorageExtensionsTests {
		private const string TestContainerName = "unittests";
		private CloudStorageAccount account;
		private CloudBlobClient blobClient;
		private CloudBlobContainer blobContainer;

		[SetUp]
		public void Initialize() {
			CloudStorageAccount.SetConfigurationSettingPublisher(ConfigSetter);
			this.account = CloudStorageAccount.FromConfigurationSetting("StorageConnectionString");
			this.blobClient = this.account.CreateCloudBlobClient();
			this.blobContainer = this.blobClient.GetContainerReference(TestContainerName);
			this.blobContainer.CreateIfNotExistAsync().GetAwaiter().GetResult();
		}

		[Test]
		public void CreateIfNotExistAsyncAsync() {
			Assert.IsFalse(this.blobContainer.CreateIfNotExistAsync().GetAwaiter().GetResult());
		}

		[Test]
		public void UploadFromStreamAndDownloadToStreamAsync() {
			const string payload = "Some message";
			string blobName = GetRandomBlobName();
			var blob = this.blobContainer.GetBlobReference(blobName);
			
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
			this.blobContainer.ListBlobsSegmentedAsync(1, progress).GetAwaiter().GetResult();
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

			blob = this.blobContainer.GetBlobReference(blob.Name);
			blob.FetchAttributesAsync().GetAwaiter().GetResult();
			Assert.That(blob.Metadata["someKey"], Is.EqualTo("someValue"));
		}

		private static string GetRandomBlobName() {
			var random = new Random();
			var buffer = new byte[8];
			random.NextBytes(buffer);
			string name = Convert.ToBase64String(buffer);
			return name;
		}

		private static void ConfigSetter(string configName, Func<string, bool> configSetter) {
			string value = ConfigurationManager.AppSettings[configName];
			if (String.IsNullOrEmpty(value)) {
				value = ConfigurationManager.ConnectionStrings[configName].ConnectionString;
			}

			configSetter(value);
		}

		private async Task<CloudBlob> CreateTestBlobAsync() {
			const string payload = "Some message";
			string blobName = GetRandomBlobName();
			var blob = this.blobContainer.GetBlobReference(blobName);
			var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(payload));
			await blob.UploadFromStreamAsync(sourceStream);
			return blob;
		}
	}
}
