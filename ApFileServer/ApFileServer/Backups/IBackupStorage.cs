using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApFileServerModel.Documents;

namespace ApFileServer.Backups
{
    public interface IBackupStorage
    {
        Task<DocumentInfo[]> GetAllDocumentsInfosAsync();
        Task<Document> LoadDocumentAsync(DocumentInfo info);
        Task<Document[]> LoadDocumentsAsync(DocumentInfo[] infos);
        Task<bool> SaveDocumentAsync(Document document);
        Task RemoveDocumentAsync(DocumentInfo info);

        string Id { get; }
        int FreeSpace { get; }
        int FilesCount { get; }
        bool CanWriteMore { get; }
    }
}
