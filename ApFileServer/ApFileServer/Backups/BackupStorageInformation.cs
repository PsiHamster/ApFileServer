using System;
using System.Collections.Generic;
using System.Text;
using ApFileServer.Backups;
using ApFileServerModel.Documents;

namespace ApFileServerModel.Backups
{
    public class BackupStorageInformation
    {
        public IBackupStorage StorageInterface { get; set; }
        public DocumentInfo[] StoredDocuments { get; set; }
    }
}
