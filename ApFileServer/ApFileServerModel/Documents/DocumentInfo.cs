using System;
using System.Collections.Generic;
using System.Text;

namespace ApFileServerModel.Documents
{
    public class DocumentInfo
    {
        public StorageType StorageType { get; set; }
        public DateTime DateLoaded { get; set; }
        public string Gallery { get; set; }
        public string Id { get; set; }

        public int Size { get; set; }


        private bool Equals(DocumentInfo info)
        {
            return
                StorageType == info.StorageType &&
                DateLoaded.Equals(info.DateLoaded) &&
                Gallery == info.Gallery &&
                Id == info.Id &&
                Size == info.Size;
        }

        public override bool Equals(object obj)
        {
            if (obj is DocumentInfo documentInfo)
                return Equals(documentInfo);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"Document gallery: [{Gallery}] id: [{Id}] dateLoaded: [{DateLoaded}] storageType: [{StorageType}]";
        }
    }
}
