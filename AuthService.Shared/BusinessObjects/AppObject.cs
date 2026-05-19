
using MSA.Shared.DataTypes;

namespace AuthService.Shared.BusinessObjects
{
    /// <summary>
    /// d?i di?n cho 1 application trong h? th?ng, 
    /// m?i application s? c� 1 appkey duy nh?t, 
    /// appkey n�y s? du?c s? d?ng d? x�c th?c khi g?i API li�n quan d?n application d�,
    /// </summary>
   
    public class AppObject : BaseObject
    {
        public int ApplicationId { get; set; }
        public AppObject() { }

    }
}
