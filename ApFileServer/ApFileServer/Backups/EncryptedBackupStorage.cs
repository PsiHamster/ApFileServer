using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ApFileServerModel.Documents;

namespace ApFileServer.Backups
{
    public class EncryptedBackupStorage : IBackupStorage
    {
        public EncryptedBackupStorage(IBackupStorage storage, string encryptionPassword)
        {
            this.storage = storage;
            this.encryptionPassword = encryptionPassword;
        }

        public Task<DocumentInfo[]> GetAllDocumentsInfosAsync()
        {
            return storage.GetAllDocumentsInfosAsync();
        }

        public Task<Document> LoadDocumentAsync(DocumentInfo info)
        {
            return storage.LoadDocumentAsync(info);
        }

        public Task<Document[]> LoadDocumentsAsync(DocumentInfo[] infos)
        {
            return storage.LoadDocumentsAsync(infos);
        }

        public Task<bool> SaveDocumentAsync(Document document)
        {
            return storage.SaveDocumentAsync(document);
        }

        public Task RemoveDocumentAsync(DocumentInfo info)
        {
            return storage.RemoveDocumentAsync(info);
        }

        public string Id => storage.Id;

        public int FreeSpace => storage.FreeSpace;

        public int FilesCount => storage.FilesCount;

        public bool CanWriteMore => storage.CanWriteMore;

        private Document Decrypt(Document document)
        {
            return new Document()
            {
                Info = document.Info,
                File = SHA512.
            };
        }

        private Document Encrypt(Document document)
        {

        }

        private readonly IBackupStorage storage;
        private readonly string encryptionPassword;
    }
}
