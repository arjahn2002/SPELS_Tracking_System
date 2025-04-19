using MY_CSC_PROJECT.Models;
using System.ComponentModel.DataAnnotations;

namespace MY_CSC_PROJECT.ViewModels
{
    public class DocumentVM
    {
        public List<Document> Documents { get; set; }

        public Document Document { get; set; } = new Document();

    }
}
