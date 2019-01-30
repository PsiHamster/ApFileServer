using System;
using System.Collections.Generic;
using System.Text;
using ApFileServerModel.Documents;

namespace ApFileServerModel.Backups
{
    public class BackupDocumentInfo
    {
        public string[] StoragesSavedIn { get; set; }
        public DocumentInfo Document;
    }
}
