using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Common.Telemetry;
using OpenTelemetry.Trace;
using Serilog;

namespace Common.Azure
{

    public interface IBlobContainer
    {
        string GetName();
        string GetPath(string blobName);
        Task<Stream> GetBlob(string blobName, CancellationToken token = default);
        Task UpsertBlob(string blobName, Stream fileStream, CancellationToken token = default);
        Task UpsertBlob(string blobName, Func<Stream, CancellationToken, Task> copyStream, CancellationToken token = default);
        Task UpsertBlob(string blobName, Stream fileStream, BlobUploadOptions blobUploadOptions, CancellationToken token = default);
        Task DeleteBlob(string blobName, CancellationToken token = default);
        Task DeleteAll(string folder, CancellationToken token = default);
        Uri GenerateSas(string blobName, long accessPeriodMins = 1, BlobSasPermissions permissions = BlobSasPermissions.Read, SasIPRange? ipRange = null);
    }

    class BlobContainer : IBlobContainer
    {
        private static readonly Tracer _tracer = Measure.CreateTracer<BlobContainer>();

        private readonly BlobContainerClient _container;
        private readonly string _directory;

        private static readonly Serilog.ILogger _logger = Log.ForContext<BlobContainer>();

        public BlobContainer(BlobContainerClient container, string directory)
        {
            _container = container;
            _directory = $"{directory}/";
        }

        public string GetName()
        {
            return _container.Name;
        }

        public string GetPath(string blobName)
        {
            return $"{_directory}{blobName}";
        }

        public async Task<Stream> GetBlob(string blobName, CancellationToken token = default)
        {
            var attr = new SpanAttributes();
            attr.Add("blob.path", GetPath(blobName));
            using var span = _tracer.StartActiveSpan($"GET blob.{_container.Name}", SpanKind.Client, initialAttributes: attr);
            var response = await _container.GetBlobClient(GetPath(blobName)).DownloadStreamingAsync(cancellationToken: token);
            return response.Value.Content;
        }

        public async Task DeleteBlob(string blobName, CancellationToken token = default)
        {
            var attr = new SpanAttributes();
            attr.Add("blob.path", GetPath(blobName));
            using var span = _tracer.StartActiveSpan($"DELETE blob.{_container.Name}", SpanKind.Client, initialAttributes: attr);
            await _container.DeleteBlobAsync(GetPath(blobName), cancellationToken: token);
        }

        public async Task DeleteAll(string folder, CancellationToken token = default)
        {
            var attr = new SpanAttributes();
            attr.Add("blob.path", GetPath(folder));
            using var span = _tracer.StartActiveSpan($"ERASE blob.{_container.Name}", SpanKind.Client, initialAttributes: attr);

            var blobs = await _container.GetBlobsAsync(prefix: GetPath(folder), cancellationToken: token)
                                        .ToArrayAsync(cancellationToken: token);
            var tasks = blobs.Select(it => _container.DeleteBlobAsync(it.Name, cancellationToken: token));

            await Task.WhenAll(tasks);
        }

        public async Task UpsertBlob(string blobName, Stream fileStream, CancellationToken token = default)
        {
            var attr = new SpanAttributes();
            attr.Add("blob.path", GetPath(blobName));
            using var span = _tracer.StartActiveSpan($"UPSERT blob.{_container.Name}", SpanKind.Client, initialAttributes: attr);
            await _container.GetBlobClient(blobName).UploadAsync(fileStream, new BlobUploadOptions(), token);
        }

        public async Task UpsertBlob(string blobName, Func<Stream, CancellationToken, Task> copyStream, CancellationToken token = default)
        {
            var attr = new SpanAttributes();
            attr.Add("blob.path", GetPath(blobName));
            using var span = _tracer.StartActiveSpan($"UPSERT blob.{_container.Name}", SpanKind.Client, initialAttributes: attr);
            using var os = await _container.GetBlobClient(blobName).OpenWriteAsync(true, new BlobOpenWriteOptions(), token);
            await copyStream(os, token);
        }

        public async Task UpsertBlob(string blobName, Stream fileStream, BlobUploadOptions blobUploadOptions, CancellationToken token = default)
        {
            var attr = new SpanAttributes();
            attr.Add("blob.path", GetPath(blobName));
            using var span = _tracer.StartActiveSpan($"UPSERT blob.{_container.Name}", SpanKind.Client, initialAttributes: attr);
            await _container.GetBlobClient(blobName).UploadAsync(fileStream, blobUploadOptions, token);
        }

        public Uri GenerateSas(string blobName, long accessPeriodMins = 1, BlobSasPermissions permissions = BlobSasPermissions.Read, SasIPRange? ipRange = null)
        {
            //  https://trailheadtechnology.com/dos-and-donts-for-streaming-file-uploads-to-azure-blob-storage-with-net-mvc/
            //  https://www.petecodes.co.uk/uploading-files-to-azure-blob-storage-using-the-rest-api-and-postman/

            //  https://docs.microsoft.com/en-us/azure/storage/blobs/sas-service-create?tabs=dotnet
            var blobClient = _container.GetBlobClient(GetPath(blobName));

            // Check whether this BlobContainerClient object has been authorized with Shared Key.
            if (!blobClient.CanGenerateSasUri)
                throw new InvalidOperationException($"BlobContainerClient must be authorized with Shared Key credentials to create a service SAS: {GetPath(blobName)}");

            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = _container.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(accessPeriodMins),
                Protocol = _container.Uri.Scheme == "http" ? SasProtocol.HttpsAndHttp : SasProtocol.Https,
            };

            sasBuilder.SetPermissions(permissions);

            if (ipRange != null)
                sasBuilder.IPRange = (SasIPRange)ipRange;

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
            _logger.Debug("SAS URI for blob is: {sas}", sasUri);

            return sasUri;
        }
    }
}