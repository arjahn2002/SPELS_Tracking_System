using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SPELS_TRACKING_SYSTEM.Models
{
    public class DocumentHistory
    {
        [Key]
        public int HistoryID { get; set; }

        public int DocumentID { get; set; }

        public string ActionType { get; set; }  // e.g., "Created", "Edited", "Forwarded"

        public string ActedBy { get; set; }

        public DateTime Timestamp { get; set; }

        [ForeignKey("DocumentID")]
        public Document Document { get; set; }
    }
}
