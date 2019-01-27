using System;
using System.Collections.Generic;
using System.Text;

namespace ApFileServerModel.Documents
{
    public class Document
    {
        public DocumentInfo Info { get; set; }
        public byte[] File { get; set; }
    }
}
