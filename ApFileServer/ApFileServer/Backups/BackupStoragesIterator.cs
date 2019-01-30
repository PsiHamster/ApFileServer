using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApFileServerModel.Backups;

namespace ApFileServer.Backups
{
    public class BackupStoragesIterator
    {
        public void Add(BackupStorageInformation storage)
        {
            lock (lockObj)
            {
                storages = storages.Append(storage).ToArray();
            }
        }

        public BackupStorageInformation[] GetStoragesToSave(BackupDocumentInfo documentToSave, int count)
        {
            lock (lockObj)
            {
                var startIndex = index;
                var result = new List<BackupStorageInformation>();
                while (index != startIndex && result.Count < count)
                {
                    if (!documentToSave.StoragesSavedIn.Contains(storages[index].StorageInterface.Id))
                    {
                        result.Add(storages[index]);
                    }

                    IncIndex();
                }

                return result.ToArray();
            }
        }

        public BackupStorageInformation GetStorageToLoad(BackupDocumentInfo documentToLoad)
        {
            lock (lockObj)
            {
                var startIndex = index;
                while (index != startIndex)
                {
                    if (documentToLoad.StoragesSavedIn.Contains(storages[index].StorageInterface.Id))
                    {
                        return storages[index];
                    }

                    IncIndex();
                }

                return null;
            }
        }

        public BackupStorageInformation GetById(string id)
        {
            lock (lockObj)
            {
                return storages.FirstOrDefault(x => x.StorageInterface.Id == id);
            }
        }

        private int IncIndex()
        {
            return index = (index + 1) >= storages.Length ? 0 : index + 1;
        }

        private int index = 0;
        private object lockObj = new object();

        private BackupStorageInformation[] storages = { };
    }
}
