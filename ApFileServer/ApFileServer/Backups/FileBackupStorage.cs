using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApFileServerModel.Documents;
using Newtonsoft.Json;
using NLog;

namespace ApFileServer.Backups
{
    public class FileBackupStorage : IBackupStorage
    {
        public FileBackupStorage(string filePath, ILogger log)
        {
            this.filePath = filePath;
            this.log = log;
        }

        public async Task<DocumentInfo[]> GetAllDocumentsInfosAsync()
        {
            return GetCachedIndexFile().Keys.ToArray();
        }

        public async Task<Document> LoadDocumentAsync(DocumentInfo info)
        {
            var e = GetCachedIndexFile();

            var filePath = e[info];

            using (var file = File.OpenRead(filePath))
            {
                var bytes = new byte[file.Length];
                var readed = await file.ReadAsync(bytes, 0, (int)file.Length);

                return new Document()
                {
                    File = bytes,
                    Info = info,
                };
            }
        }

        public async Task RemoveDocumentAsync(DocumentInfo info)
        {
            var index = GetCachedIndexFile();
            File.Delete(index[info]);
            index.Remove(info);
            SaveIndexFile(index);
        }

        public Task<Document[]> LoadDocumentsAsync(DocumentInfo[] infos)
        {
            return Task.WhenAll(infos.Select(async x => await LoadDocumentAsync(x)).ToArray());
        }

        public async Task<bool> SaveDocumentAsync(Document document)
        {
            var index = GetCachedIndexFile();

            var path = GenerateFilePath(document);
            Directory.CreateDirectory(path);

            index[document.Info] = Path.Combine(path, document.Info.Id);
            
            using (var file = File.OpenWrite(index[document.Info]))
            {
                await file.WriteAsync(document.File, 0, document.File.Length);
            }

            SaveIndexFile(index);
            return true;
        }

        private string GenerateFilePath(Document document)
        {
            return Path.Combine(filePath, document.Info.Gallery);
        }

        private Dictionary<DocumentInfo, string> GetCachedIndexFile()
        {
            if (DateTime.UtcNow > cachedEndTime)
            {
                cachedEndTime = DateTime.UtcNow.AddMinutes(10);
                indexData = LoadFromIndexFile();
            }
            
            return indexData;
        }

        private Dictionary<DocumentInfo, string> indexData = new Dictionary<DocumentInfo, string>();
        private DateTime cachedEndTime;

        private Dictionary<DocumentInfo, string> LoadFromIndexFile()
        {
            if (!File.Exists(filePath))
            {
                return new Dictionary<DocumentInfo, string>();
            }

            using (var file = File.OpenRead(Path.Combine(filePath, "index.json")))
            {
                using (var stream = new StreamReader(file))
                {
                    return JsonConvert.DeserializeObject<Dictionary<DocumentInfo, string>>(stream.ReadToEnd());
                }
            }
        }

        private void SaveIndexFile(Dictionary<DocumentInfo, string> toSave)
        {
            using (var file = File.OpenWrite(Path.Combine(filePath, "index.json"))) 
            {
                using (var stream = new StreamWriter(file))
                {
                    stream.Write(JsonConvert.SerializeObject(toSave));
                }
            }
        }

        public string Id => filePath;

        // TODO
        public int FreeSpace => 10000000;
        public int FilesCount => GetCachedIndexFile().Count;
        public bool CanWriteMore => true;


        private readonly string filePath;
        private readonly ILogger log;
    }
}
