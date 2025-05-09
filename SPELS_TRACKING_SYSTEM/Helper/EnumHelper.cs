using SPELS_TRACKING_SYSTEM.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SPELS_TRACKING_SYSTEM.Helper
{
    public class EnumHelper
    {
        public static string GetEnumDisplayName(SubmissionType submissionType)
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

        public static string GetEnumDisplayName(GenderType genderType)
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

        public static string GetEnumDisplayName(StatusType statusType)
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
    }
}
