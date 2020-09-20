using System.Collections.Generic;

namespace ClaytonsWeb2
{
    public class PreSearchListModel
    {
        public int CategoryId {get; set;}
        public string CategoryLabel {get; set;}
        public List<presearch_list> SearchTerms {get; set;}
    
    }
}