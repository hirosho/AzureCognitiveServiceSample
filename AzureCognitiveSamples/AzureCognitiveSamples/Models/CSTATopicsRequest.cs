using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveSamples.Models
{
    public class CSTATopicsRequest
    {
        public List<string> stopWords { get; }
        public List<string> topicsToExclude { get; }
        public List<CSTADocument> documents { get; }

        public CSTATopicsRequest()
        {
            stopWords = new List<string>();
            topicsToExclude = new List<string>();
            documents = new List<CSTADocument>();
        }
    }
}
