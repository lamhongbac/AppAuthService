using MSA.Shared.DataTypes;

namespace AuthService.Shared.BusinessObjects
{
    /// <summary>
    /// 1 role la tap hop cac quyen truy cap den cac object,
    /// moi quyen object duoc dinh nghia trong ObjectRight,
    /// 1  role chi co quyen tren tap hop app object thuoc cung 1 application, 
    /// khong co quyen tren app object thuoc application khac
    /// </summary>
    public class AppRole: BaseObject
    {
        public int ApplicationId { get; set; }
        public AppRole() { }
       
        
        public List<ObjectRight> ObjectRights { get; set; } = new List<ObjectRight>();

    }
}
