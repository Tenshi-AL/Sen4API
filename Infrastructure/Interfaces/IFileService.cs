using Infrastructure.DTO;
using Minio.DataModel;
using Minio.DataModel.Encryption;
using Minio.DataModel.Response;

namespace Infrastructure.Interfaces;

public interface IFileService
{
    Task<string?> GetObjectUrl(string objectName, string bucketName);
    Task<MemoryStream?>GetObject(string objectName, string bucketName);
    Task RemoveObject(string objectName, string bucketName);
    Task<List<Item>> ListProjectsObject(string? name, string projectId, string? taskId,  string? prefix = null, bool recursive = true, bool versions = false);
    Task<PutObjectResponse> PutObject(FileWriteDTO fileWriteDto, Dictionary<string, string?> metaData, IProgress<ProgressReport>? progress = null, IServerSideEncryption? sse = null);
}