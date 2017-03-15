using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveSamples.Models
{
    public class CSTADocument
    {
        public string id { get; set; }
        public string text { get; set; }
    }

    public class CSTADocuments
    {
        public List<CSTADocument> documents { get; }

        public CSTADocuments()
        {
            documents = new List<CSTADocument>();
        }
    }
}
