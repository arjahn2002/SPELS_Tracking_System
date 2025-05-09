using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace SPELS_TRACKING_SYSTEM.Models
{
    public class Document
    {
        [Key]
        public int DocumentID { get; set; }

        [Required(ErrorMessage = "Please select a type of submission.")]
        [Display(Name = "Type of Submission")]
        public int SubmissionType { get; set; }

        public string SubmissionTypeName => GetEnumDisplayName((SubmissionType)SubmissionType);

        public string GetEnumDisplayName(SubmissionType submissionType)
        {
            var type = submissionType.GetType();
            var memberInfo = type.GetMember(submissionType.ToString());

            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
                return attributes?.Name ?? submissionType.ToString();
            }

            return submissionType.ToString();
        }

        [Required(ErrorMessage = "Please select an other fo.")]
        [Display(Name = "Other FO's")]
        public int OtherFOsID { get; set; }

        [ForeignKey("OtherFOsID")]
        public OtherFOs? OtherFOs { get; set; }

        [Required(ErrorMessage = "Please enter a last name.")]
        [Display(Name = "Last Name")]
        public string Lastname { get; set; }

        [Required(ErrorMessage = "Please enter a first name.")]
        [Display(Name = "First Name")]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Please enter a middle name.")]
        [Display(Name = "Middle Name")]
        public string Middlename { get; set; }

        public string? Suffix { get; set; }

        [Required(ErrorMessage = "Please select a gender.")]
        public int GenderType { get; set; }

        public string GenderTypeName => GetEnumDisplayName((GenderType)GenderType);

        public string GetEnumDisplayName(GenderType genderType)
        {
            var type = genderType.GetType();
            var memberInfo = type.GetMember(genderType.ToString());

            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
                return attributes?.Name ?? genderType.ToString();
            }

            return genderType.ToString();
        }

        [Required(ErrorMessage = "Please enter an address.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please select a province.")]
        public int ProvinceID { get; set; }

        [ForeignKey("ProvinceID")]
        public Province? Province { get; set; }

        [Required(ErrorMessage = "Please select a position.")]
        public int PositionID { get; set; }

        [ForeignKey("PositionID")]
        public Position Position { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Please provide a date of birth.")]
        [Display(Name = "Date of Birth")]
        public DateTime DateofBirth { get; set; }

        [Required(ErrorMessage = "Please enter a place of birth.")]
        [Display(Name = "Place of Birth")]
        public string PlaceofBirth { get; set; }

        [Required(ErrorMessage = "Please select an special eligibility.")]
        [Display(Name = "Special Eligibility")]

        public int SpecialEligibilityID { get; set; }

        [ForeignKey("SpecialEligibilityID")]
        public SpecialEligibility? SpecialEligibility { get; set; }

        [Required(ErrorMessage = "Please select a type of eligibility.")]
        [Display(Name = "Type of Eligibility")]
        public string TypeofEligibility {  get; set; }

        [Required(ErrorMessage = "Please enter an other eligibility.")]
        [Display(Name = "Other Eligibility")]
        public string OtherEligibility { get; set; }

        [Required(ErrorMessage = "Please enter a school name.")]
        public string School { get; set; }

        [Required(ErrorMessage = "Please enter a remarks.")]
        public string Remarks { get; set; }

        public int StatusType { get; set; }

        public string StatusTypeName => GetEnumDisplayName((StatusType)StatusType);

        public string GetEnumDisplayName(StatusType statusType)
        {
            var type = statusType.GetType();
            var memberInfo = type.GetMember(statusType.ToString());

            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
                return attributes?.Name ?? statusType.ToString();
            }

            return statusType.ToString();
        }

        public DateTime DateEntered { get; set; }

        [Required(ErrorMessage = "Please enter the date received.")]
        [DataType(DataType.Date)]
        public DateTime DateReceived { get; set; }

        public virtual ICollection<ReleasingStage>? ReleasingStages { get; set; }
    }
}
