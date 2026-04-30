namespace AuthService.Shared.BusinessObjects
{
    // dinh nghia quyen truy cap cua role voi object
    public class ObjectRight
    {
        public int Id { get; set; }
        public int ObjectId { get; set; }
        public string ObjectNumber { get; set; } = string.Empty;
        public string ObjectName { get; set; } = string.Empty;
        public bool CanList { get; set; }
        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
    }
}
