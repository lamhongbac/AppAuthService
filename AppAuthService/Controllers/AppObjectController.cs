using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppAuthService.Controllers
{
    /// <summary>
    /// chu y sau khi register Application, da co App-key, buoc này can co appkey trong header cua request, ...
    /// cac nghiep vu lien quan den AppObject trong 1 application, bao gom 
    /// cac chuc nang CRUD tren AppObject, 
    /// va cac chuc nang lien quan den AppObject nhu get list AppObject theo applicationId, get AppObject theo Id, ...
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AppObjectController : ControllerBase
    {
        //1. Create new AppObject 
        //2. Get AppObject by Id
        //3. Get all AppObjects
        //4. Update AppObject
        //5. Delete AppObject
    }
}
