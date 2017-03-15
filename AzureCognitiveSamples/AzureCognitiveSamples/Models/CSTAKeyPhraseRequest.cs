using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveSamples.Models
{
    public class CSTAKeyPhraseDocument
    {
        public string language { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }

    public class CSTAKeyPhraseRequest
    {
        public List<CSTAKeyPhraseDocument> documents { get; }
        public CSTAKeyPhraseRequest()
        {
            documents = new List<CSTAKeyPhraseDocument>();
        }
    }
}
