namespace MY_CSC_PROJECT.ViewModels
{
    public class RolePermissionVM
    {
        public string StageName { get; set; }

        public bool CanAccess { get; set; }
        public bool CanForward { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanExport { get; set; }
        public bool CanRemove { get; set; }
    }
}
