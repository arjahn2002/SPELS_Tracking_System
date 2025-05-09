using SPELS_TRACKING_SYSTEM.Models;
using System.ComponentModel.DataAnnotations;

namespace SPELS_TRACKING_SYSTEM.ViewModels
{
    public class DocumentVM
    {
        public List<Document> Documents { get; set; }

        public Document Document { get; set; } = new Document();

    }
}
