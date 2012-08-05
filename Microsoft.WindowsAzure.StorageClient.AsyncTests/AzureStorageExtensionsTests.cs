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

		private static string GetRandomBlobName() {
			return "FooBlob";
		}

		private static void ConfigSetter(string configName, Func<string, bool> configSetter) {
			string value = ConfigurationManager.AppSettings[configName];
			if (String.IsNullOrEmpty(value)) {
				value = ConfigurationManager.ConnectionStrings[configName].ConnectionString;
			}

			configSetter(value);
		}
	}
}
