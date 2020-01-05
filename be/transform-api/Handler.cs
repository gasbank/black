using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System;
using System.IO;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Black
{
    public class Handler
    {
        private readonly AmazonS3Client client = new AmazonS3Client();
        private readonly string bucketName = Environment.GetEnvironmentVariable("SOURCE_BUCKET");
        private readonly string uploadKeyPrefix = Environment.GetEnvironmentVariable("UPLOAD_KEY_PREFIX");

        public void TransformImage(S3EventNotification request)
        {
            Console.Out.WriteLine($"Env: bucketName={bucketName} uploadKeyPrefix={uploadKeyPrefix}");
            Console.Out.WriteLine("Start to transform");
            try
            {
                TransformImageAsync(request).Wait();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine($"Error in transformation: {e.Message}");
            }
            finally
            {
                CleanupTempDirectory();
            }
            Console.Out.WriteLine("End of transform");
        }

        public async Task TransformImageAsync(S3EventNotification request)
        {
            foreach (var each in request.Records)
            {
                await TransformOneAsync(each.S3.Object.Key);
            }
        }

        private async Task TransformOneAsync(string s3Key)
        {
            var fileKey = Path.GetFileName(s3Key);
            Console.Out.WriteLine($"key={s3Key} fileKey={fileKey}");

            var localTempFile = Path.GetTempPath() + fileKey;
            Console.Out.WriteLine($"localTempFile={localTempFile}");
            await DownloadFile(s3Key, localTempFile);

            Console.Out.WriteLine("Do SingleFileTransform");
            new SingleFileTransform()
            {
                Mode = "dit",
                StartFileName = localTempFile,
                MaxColor = 30,
                OutputPathReplaceFrom = "out",
                OutputPathReplaceTo = ""
            }.Run();

            var fileName = Path.GetFileNameWithoutExtension(fileKey);
            foreach (var postfix in new[] { ".bytes", "-OTB-FSNB.png", "-OTB-FSNB-BB-SDF.png" })
            {
                var mimeType = postfix.EndsWith("png") ? "image/png" : "application/octet-stream";
                await UploadFile(Path.Join(Path.GetDirectoryName(localTempFile), fileName + postfix), uploadKeyPrefix + fileName + postfix, mimeType);
            }
        }

        private async Task DownloadFile(string remoteKey, string localFile)
        {

            var s3Request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = remoteKey,
            };

            using (var response = await client.GetObjectAsync(s3Request))
            using (var responseStream = response.ResponseStream)
            using (var writer = File.Create(localFile))
            {
                Console.Out.WriteLine("Download file");
                await responseStream.CopyToAsync(writer);
            }
        }

        private async Task UploadFile(string localFile, string remoteKey, string mimeType)
        {
            Console.Out.WriteLine($"Upload file: localFile={localFile} remoteKey={remoteKey}");
            await client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucketName,
                Key = remoteKey,
                FilePath = localFile,
                ContentType = mimeType
            });
            Console.Out.WriteLine($"Upload completed: {localFile}");
        }

        private void CleanupTempDirectory()
        {
            foreach (var pattern in new[] { "*.bytes", "*.png" })
            {
                foreach (var file in Directory.GetFiles(Path.GetTempPath(), pattern))
                {
                    Console.Out.WriteLine($"Delete file: {file}");
                    DeleteFileAnyway(file);
                }
            }
        }

        private void DeleteFileAnyway(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine($"Cannot delete file: {file} error={e.Message}");
            }
        }
    }
}
