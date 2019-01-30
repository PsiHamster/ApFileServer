using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApFileServer.Exceptions;
using ApFileServerModel.Backups;
using ApFileServerModel.Documents;
using NLog;

namespace ApFileServer.Backups
{
    public interface IBackupController
    {
        Task RegisterStorageAsync(IBackupStorage storage);
        void Start();

        DocumentInfo[] GetDocumentInfos();
        DocumentInfo[] GetDocumentInfos(string gallery);
        Task<Document> LoadDocumentAsync(string id);
        Task SaveDocumentAsync(Document document);
    }

    public class BackupController : IBackupController, IDisposable
    {
        public BackupController(int replicationFactor, ILogger log)
        {
            if (replicationFactor <= 0)
                throw new ArgumentException("Replication factor need to be > 0");
            this.replicationFactor = replicationFactor;
            this.log = log;
        }

        public void Start()
        {
            isWorking = true;
            workThread = new Thread(WorkCycle);
            workThread.Start();
        }

        public DocumentInfo[] GetDocumentInfos()
        {
            return documents.Values.Select(x => x.Document).ToArray();
        }

        public DocumentInfo[] GetDocumentInfos(string gallery)
        {
            return documents.Values.Select(x => x.Document).Where(x => x.Gallery == gallery).ToArray();
        }

        public Task<Document> LoadDocumentAsync(string id)
        {
            return storages
                .GetStorageToLoad(documents[id])
                .StorageInterface
                .LoadDocumentAsync(documents[id].Document);
        }

        public Task SaveDocumentAsync(Document document)
        {
            return SaveDocumentToStorages(document);
        }
        
        public async Task RegisterStorageAsync(IBackupStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            var documentsInStorage = await storage.GetAllDocumentsInfosAsync();
            lock (lockObject)
            {
                var storageInfo = new BackupStorageInformation()
                {
                    StorageInterface = storage,
                    StoredDocuments = documentsInStorage,
                };
                
                storages.Add(storageInfo);

                foreach (var documentInfo in documentsInStorage)
                {
                    RegisterDocumentInfo(documentInfo);
                    DocumentWasSavedIn(documentInfo, storageInfo);
                }
            }
        }

        private void DocumentWasSavedIn(DocumentInfo documentInfo, BackupStorageInformation storageInfo)
        {
            if (!documents[documentInfo.Id].Document.Equals(documentInfo))
                throw new StorageException($"Documents list is corrupted! Found two different documents with same id! [{documentInfo}] and [{documents[documentInfo.Id].Document}]");

            documents[documentInfo.Id].StoragesSavedIn = documents[documentInfo.Id].StoragesSavedIn
                .Append(storageInfo.StorageInterface.Id).ToArray();
        }

        private BackupDocumentInfo RegisterDocumentInfo(DocumentInfo documentInfo)
        {
            if (!documents.ContainsKey(documentInfo.Id))
            {
                documents[documentInfo.Id] = new BackupDocumentInfo()
                {
                    Document = documentInfo,
                    StoragesSavedIn = { }
                };
            }

            return documents[documentInfo.Id];
        }

        private async Task SaveDocumentToStorages(Document document)
        {
            var backupInfo = RegisterDocumentInfo(document.Info);
            var tasks = storages
                .GetStoragesToSave(backupInfo, replicationFactor)
                .Select(async x => (await x.StorageInterface.SaveDocumentAsync(document), x));

            var results = await Task.WhenAll(tasks);
            foreach (var result in results)
            {
                if (result.Item1)
                {
                    DocumentWasSavedIn(document.Info, result.Item2);
                }
                else
                {
                    log.Error("Error while saving");
                }
            }
        }
        
        public void Dispose()
        {
            if (isWorking)
            {
                isWorking = false;
                workThread.Abort();
            }
        }

        private async void WorkCycle()
        {
            while (true)
            {
                log.Info("Checking backup storage");

                var toRemove = new List<string>();
                foreach (var backupInfo in documents.Values)
                {
                    if (backupInfo.StoragesSavedIn.Length == 0)
                    {
                        log.Error($"Backuped document had been lost. [{backupInfo.Document}]");

                        toRemove.Add(backupInfo.Document.Id);
                    }

                    if (backupInfo.StoragesSavedIn.Length < replicationFactor)
                    {
                        log.Info($"Document [{backupInfo.Document}] have less replication that needed. Adding to storages.");

                        var document = await storages.GetStorageToLoad(backupInfo)
                            .StorageInterface.LoadDocumentAsync(backupInfo.Document);
                        await SaveDocumentToStorages(document);
                    }
                }

                foreach (var removeId in toRemove)
                {
                    documents.Remove(removeId);
                }

                await Task.Delay(10 * 60 * 1000);
            }
        }
        
        private BackupStoragesIterator storages = new BackupStoragesIterator();
        private Dictionary<string, BackupDocumentInfo> documents = new Dictionary<string, BackupDocumentInfo>();
        private readonly int replicationFactor;
        private readonly ILogger log;
        private readonly object lockObject = new object();

        private Thread workThread;
        private bool isWorking;
    }
}
