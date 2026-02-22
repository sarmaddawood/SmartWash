using Supabase;

namespace SmartWash.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        string GetPublicUrl(string fileName);
    }

    public class SupabaseStorageService : IStorageService
    {
        private readonly Client _supabaseClient;
        private readonly string _bucketName;

        public SupabaseStorageService(IConfiguration configuration)
        {
            var url = configuration["Supabase:Url"];
            var key = configuration["Supabase:Key"];
            _bucketName = configuration["Supabase:BucketName"] ?? "laundry-media";

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false
            };
            
            _supabaseClient = new Client(url!, key!, options);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            // Read stream into byte array
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);
            var fileData = ms.ToArray();

            // Upload to Supabase Storage
            var storage = _supabaseClient.Storage.From(_bucketName);
            await storage.Upload(fileData, fileName, new Supabase.Storage.FileOptions { ContentType = contentType });

            return GetPublicUrl(fileName);
        }

        public string GetPublicUrl(string fileName)
        {
            return _supabaseClient.Storage.From(_bucketName).GetPublicUrl(fileName);
        }
    }
}
