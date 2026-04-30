
using MSA.Shared;

namespace AuthService.Shared.BusinessObjects
{
    /// <summary>
    /// d?i di?n cho 1 application trong h? th?ng, 
    /// m?i application s? có 1 appkey duy nh?t, 
    /// appkey này s? du?c s? d?ng d? xác th?c khi g?i API liên quan d?n application dó,
    /// </summary>
   
    public class AppObject : BaseObject
    {
        public int ApplicationId { get; set; }
        public AppObject() { }

    }
}
