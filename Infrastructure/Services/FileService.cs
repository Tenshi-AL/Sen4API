using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.Encryption;
using Minio.DataModel.Response;

namespace Infrastructure.Services;

/// <summary>
/// FileService
/// </summary>
/// <param name="minioClient">MinIO instance</param>
public sealed class FileService(IMinioClient minioClient, ILogger<FileService> logger): IFileService
{
    /// <summary>
    /// Check bucket is exists
    /// </summary>
    /// <param name="name">Bucket name</param>
    private async Task MakeBucketIfNotExists(string name)
    {
        var bucket = new BucketExistsArgs()
            .WithBucket(name);
        if (!await minioClient.BucketExistsAsync(bucket))
        {
            var makeBucketArgs = new MakeBucketArgs()
                .WithBucket(name);
            await minioClient.MakeBucketAsync(makeBucketArgs);
        }
    }

    public async Task<string?> GetObjectUrl(string objectName, string bucketName)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(1000);
        
        return await minioClient.PresignedGetObjectAsync(args);
    }
    
    /// <summary>
    /// Download file
    /// </summary>
    /// <param name="objectName">File name</param>
    /// <param name="bucketName">Bucket name</param>
    /// <returns>File memory stream</returns>
    public async Task<MemoryStream?> GetObject(string objectName, string bucketName)
    {
        try
        {
            var memoryStream = new MemoryStream();

            var getArguments = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(async (stream, cancellationToken) =>
                {
                    await stream.CopyToAsync(memoryStream, cancellationToken);
                });

            await minioClient.GetObjectAsync(getArguments);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return null;
        }
    }
    
    /// <summary>
    /// Remove file
    /// </summary>
    /// <param name="objectName">File name</param>
    /// <param name="bucketName">Bucket name</param>
    public async Task RemoveObject(string objectName, string bucketName)
    {
        var removeArguments = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);

        await minioClient.RemoveObjectAsync(removeArguments);
    }   
    
    /// <summary>
    /// Put file in storage
    /// </summary>
    /// <param name="fileWriteDto">File write model</param>
    /// <param name="progress"></param>
    /// <param name="sse"></param>
    /// <param name="metaData"></param>
    /// <returns>Return PutObjectResponse.</returns>
    public async Task<PutObjectResponse> PutObject(FileWriteDTO fileWriteDto, Dictionary<string, string?> metaData, IProgress<ProgressReport>? progress = null, IServerSideEncryption? sse = null)
    {
        await MakeBucketIfNotExists(fileWriteDto.ProjectId.ToString());
        var file = fileWriteDto.File;
        
        //upload file
        var filestream = fileWriteDto.File.OpenReadStream();
        
        var putArguments = new PutObjectArgs()
            .WithBucket(fileWriteDto.ProjectId.ToString())
            .WithObject(file.FileName)
            .WithStreamData(filestream)
            .WithObjectSize(filestream.Length)
            .WithContentType("application/octet-stream")
            .WithHeaders(metaData)
            .WithProgress(progress)
            .WithServerSideEncryption(sse);

        return await minioClient.PutObjectAsync(putArguments);
    }

    /// <summary>
    /// Get list files
    /// </summary>
    /// <param name="projectId">Project id</param>
    /// <param name="taskId">Task id</param>
    /// <param name="prefix"></param>
    /// <param name="recursive"></param>
    /// <param name="versions"></param>
    /// <returns></returns>
    public async Task<List<Item>> ListProjectsObject(string? name, string? projectId, string? taskId, string? prefix = null, bool recursive = true, bool versions = false)
    {
        await MakeBucketIfNotExists(projectId);
        
        var metaData = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            { "Project", projectId },
        };
        if (taskId is not null)
            metaData.Add("Task", taskId);
        
        var listArgs = new ListObjectsArgs()
            .WithBucket(projectId)
            .WithPrefix(prefix)
            .WithRecursive(recursive)
            .WithIncludeUserMetadata(true)
            .WithHeaders(metaData)
            .WithVersions(versions);
        
        var items = new List<Item>();
        await foreach (var item in minioClient.ListObjectsEnumAsync(listArgs))
            items.Add(item);

        if (name is not null)
            items = items.Where(p => p.UserMetadata.TryGetValue("Name", out string? itemName) && itemName == name).ToList();
        if (projectId is not null)
            items = items.Where(p => p.UserMetadata.TryGetValue("Project", out string? project) && project == projectId).ToList();
        if (taskId is not null)
            items = items.Where(p => p.UserMetadata.TryGetValue("Task", out string? task) && task == taskId).ToList();
        
        return items;
    }
}