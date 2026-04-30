1. Giới thiệu tổng quan
AuthService (AS) là một dịch vụ Microservice độc lập chuyên trách nhiệm lưu trữ và quản lý xác thưc & phân quyền cho hệ sinh thái các ứng dụng của doanh nghiệp.
•	Mục tiêu: Tách rời gánh nặng quản lý phân quyền,ra khỏi ứng dụng, hình thành 1 công thức chung về quản lý phân quyền cho mọi ứng dụng, triển khai SSO cho phép logined 1 lần và sử dụng trên  nhiều ứng dụng.
•	Kiến trúc: Clean Architecture (3 Layers).
•	Công nghệ lõi: .NET 8 Web API,  Mapster. EntityFramework, Microsoft Json Webtoken
•	Database:MS-SQL, Mỗi 1 cty sử dụng 1 ứng dụng AuthService tương ứng 1 DB AuthService
2. Kiến trúc hệ thống (Architecture Stack)
Tầng (Layer)	Công nghệ / Thư viện	Vai trò
GUI / API: AppAuthService	ASP.NET Core API	Tiếp nhận và điều hướng yêu cầu xác thực, phân quyền.
AppAuthService.BLL	Class Library	Xử lý logic nghiệp vụ, ánh xạ dữ liệu, kiểm tra quyền.
AuthService.DAL	EntityFW, DB First	Tương tác CSDL SQL Server (Models, Repositories).

Thư viện dùng chung (Shared & Utils):
AuthService.Shared	DTO, BO, Result	Chia sẻ kiểu dữ liệu nội bộ dự án AuthService (API/BLL/Client).
MSA.Shared	Base Models, Types	Các kiểu dữ liệu nền tảng dùng chung cho mọi ứng dụng sau này.
AppAuth.Utils	JWT Library	Chuyên trách xử lý Token (Access, Refresh), JwtConfig.
MSA.Utils	Helper Libraries	Các hàm tiện ích (String, DateTime, Security) cho mọi hệ thống.
3. Quy trình nghiệp vụ cốt lõi
3.1. Đăng ký Công ty (Company Registration)
•	Input: CompanyName, Company Code.
•	Logic: MSA sinh ra một ApiKey duy nhất (Guid-N) cho mỗi cty
•	Output: Cấp phát ApiKey cho ứng dụng khách.
3.1. Đăng ký ứng dụng (Application Registration)
•	Input: AppName, AppCode. In request header (x-api-key:Api-key)
•	Logic: MSA sinh ra một AppKey duy nhất (Guid-N).Sinh ra các thuộc tính tạo jwt cho từng app khác nhau
•	Output: Cấp phát AppCode ,AppKey (jwt Key) hoạc body request (nếu kg dung Jwt), các thuộc tính liên quan đến jwtConfig  : AppKey ,Issuer, Audience,Expired,RefExpired
•	3.2. CRUD AppObject
•	1 application bao gồm 1 tập hợp các business object hay còn gọi là app object
•	App object có thể là 1view hay 1 sub view, 1 process hay 1 sub process tùy ý, được đn trong ObjectType :EObjectType{View,Process,Data}

3.3. CRUD App Role
•	1 Role trong 1 app bao gồm 1 tập hợp định nghĩa về quyền (List, CRUD) trên từng app object
•	Danh sách quyền được khai báo trong bảng RoleRights

3.3. CRUD User & Role
•	Định nghĩa các thuộc tính chung nhất của 1 user như Id, UserID, FullName,..vv
•	Ánh xạ User và danh sách app role, trong bảng [UserRoles]: 
o	1 User trong 1 app được ánh xạ vào 1 tập hợp role, theo đó sẽ suy ra các quyền trên app objects mà user đó được gán,
o	1 User có thể đc phân quyền trên nhiều app role trong bảng [UserRoles]
3.3. Kiểm tra  xác thực (username/Password)
•	ObjectRight : List, CRUD
•	Kiểm tra user name và mật khẩu:  khi end user login
o	Authenticate (userID và mật khẩu) => Jwt object, Dictionary<string,objectRight> và danh sách quyền cua tung object, cho mục đích lập trình hiển thị giao diện app
o	Jwt Object: Jwt string, rft(refreshtoken) string
KIỂM TRA PHẦN QUYỀN CỦA USER TRÊN 1 APP OBJECTS
•	Kiểm tra phân quyền theo jwt:
o	Authorize (Jwt, AppObject, Right)->bool: kiem tra user được phép truy cập theo quyền chỉ ra tren object hay không=> tra về false nếu kg có quyền
o	RefreshToken (jwt, jft) Thực hiện refreshToken=> khi jwt token hết hạn

Các thuộc tính quan trọng của Businesss Object
Company/ RegKey:
RegKey =Api key nằm trong header x-api-key của request, khi call hàm Api luôn buộc có key này, admin sẽ cung cấp key này cho client bên ngoài ứng dụng, thủ tục đăng ký company=> api-key 
Application/[AppKey]: 
Application: khai báo 1 application trong companyId chỉ ra
AppKey nằm trong body của request (BaseRequest), khi 1 yêu cầu (API) đến function thuộc domain = application , cần truyền appKey=> appID (kg dung AppId trong setting phía client) để xác định tính valid của request
AppObject/ObjectType
là các đối tượng cần quản lý phân quyền trong 1 application, Objecttype là kiểu string chính là string của enum EObjectType trong thư viện 

User
[Pwd]: tránh dung chữ password cho fieldName
[PwdKey]: dung để mã hóa password 1 chiều, mỗi user có thể sinh ra các PwdKey khác nhau
[UserName] [nvarchar](50) NOT NULL,
[Email] [nvarchar](50) NULL,
[Mobile] [varchar](50) NULL,
[CardNum] [varchar](50) NULL,

Có thể dung 1 trong 4 cách trên để login
 

LoginRequest

Đối tượng Login Request
Bên cạnh các thuộc tính cơ bản như Full Name,password
Usertype: có thể là email, mobile, cardNumber hay just UserName (text). User có thể login vào thông qua các hình thức định danh trên, cần chỉ ra loai là enum EUserType (Email, Mobile, UserName, CardNum) để BE thực hiện kiểm chứng
AppID: luôn bắt đầu với 1 app cụ thể, khi cần chuyển app khác thì chỉ cần gọi Hàm SwitchApp(appID)
Đối tượng LoginInfo: có các thuộc tính cần chú ý sau
thông tin trả về client sau khi login thành công, bao gồm, thông tin profile cơ bản của user (fullName, …), danh sách AppObject và quyền trên AppObject 
Chuyển App: cho phép chỉ login 1 lần và sau đó chỉ chuyển qua các App đã đăng ký, khi chuyển qua app khác sẽ update lại danh sách AppObject và quyền trên AppObject

Mô tả các bước xử lý Authenticate (loginRequest) qua các tầng (sequence diagram)
1/ Client gửi loginRequest->Api: AuthService
2/ AuthService tiếp nhận : kiểm tra tính hợp lệ request ở tầng AuthService (header, para..vv)
3/ AuthService chuyển request cho BLL kiểm tra và thực hiện hàm authenticate-> trả ra đối tượng UserInfo (profile và danh sach object right tương ứng appID)
4/AuthService chuyển UserInfo  cho JwtService thực hiện hàm GenerateJwt-> JwtInfo (bao gồm lưu thông tin refreshtoken trong CSDL)
5/AuthService cập nhật thuộc tính Jwt= JwtInfo của loginResponse (userInfo, JwtInfo)
6/AuthService trả loginResponse  về cho Api trong 1 wrapper BOProcessResult, trong đó OK-true và object= loginResponse  , Message=”Login success”, ErrorCode=”LoginSucess”
7/Api trả về cho client đối tượng BOProcessResult
8/Client thục hiện parse đối tượng BOProcessResult và lưu thông tin loginResponse   bao gồm (UserInfo, JwtInfo), Client sử dụng ErrorCode , Message kết hợp chức năng đa ngôn ngữ dịch ra  “display message” và hiển thị cho user
Mô tả các bước xử lý Authorize (authRequest) qua các tầng (sequence diagram)

•  x-api-key: Để định danh ứng dụng (Mobile app hay Web app).
•  Authorization: Bearer <JWT_Token>: Chứa thông tin danh tính người dùng đã đăng nhập.
 Request check Authorize: AuthorizeRequest: {AppID, ObjectNumber, List<RightToCheck> }
IsAuthorization(AuthorizeRequest  authRequest)

Check API-Key: Nếu không đúng, trả về 401 Unauthorized.
Validate JWT: Web Server sử dụng Secret Key để giải mã JWT.
•	Kiểm tra chữ ký (Signature).
•	Kiểm tra thời gian hết hạn (Expiration).
•	Nếu hợp lệ, chuyển thông tin từ Claims (Payload của JWT) vào HttpContext.User

1/Client gửi request
2/Api check valid Api
3/APi Chuyển qua Jwt =>kiểm tra và parse ra userID,
4/Chuyên UserID, AppID, ObjectNumber, RightToChecks qua Auth.BLL
5/Tra về API kết quả trong 1 wraper BO object Result
6/Api Tra về Client

Luồng xử lý Jwt hết hạn
Client gửi yêu cầu kèm JWT (đã hết hạn).
Server (Middleware xác thực) kiểm tra, thấy Token hết hạn và trả về mã HTTP 401 Unauthorized.
Client (thường là qua bộ lọc Interceptor trong code Mobile/Web) bắt được mã 401.
Thay vì đá người dùng ra màn hình Login, Client sẽ tạm dừng các request khác và gọi hàm API RefreshToken(oldRefreshToken)
Nếu lấy được JWT mới, Client sẽ thực hiện lại (Retry) cái request bị lỗi lúc nãy.
Ưu điểm: Cực kỳ an toàn và chính xác vì Server là người quyết định cuối cùng về tính hợp lệ của Token.
Database dictionary
Database: AppAuthService
Các thuộc tính chung

[ID] [int] IDENTITY(1,1) NOT NULL,
	[Number] [varchar](20) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Description] [nvarchar](250) NOT NULL,	
…body của các bảng
[CreatedBy] [varchar](20) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[ModifiedBy] [varchar](20) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
[IsActive] [bit] NOT NULL,

Notes:
ID: int/Guid là PK
Number: string là khóa của người dung đưa vào
CreatedBy/ModifiedBy :default ‘System’ ( khi record dc tạo kg từ giao diện)
ModifiedOn/CreatedOn: default GetDate()
IsDeleted=false=> khi delete ma kg anh huong den FK thông qua mark delete IsDeleted=true
IsActive=true, khi có cơ chế chờ kích hoạt IsActive=false, sau approved IsActive=true

Companies: mô tả 1 công ty nơi các ứng dụng sinh sống
[ID] [int] IDENTITY(1,1) NOT NULL,
	[RegKey] [varchar](50) NOT NULL, 
	[Name] [varchar](50) NOT NULL,
	[Description] [nvarchar](250) NOT NULL,
	[CreatedBy] [varchar](20) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[ModifiedBy] [varchar](20) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
[IsActive] [bit] NOT NULL,
[AppKey] [nvarchar](250) NOT NULL,
	[Issuer] [nvarchar](250) NOT NULL,
	[Audience] [nvarchar](250) NOT NULL,
	[Expire] [int] NOT NULL,
	[RefExpire] [int] NOT NULL,

Notes:
RegKey= ApiKey, in header api request.
Jwt Config: (AppKey,Issuer,Expire,RefExpire) tùy thuộc yc, nếu kg quá khắt khe dung chung toàn ứng dụng. Nếu khắt khe mỗi ứng dụng sẽ dung riêng

Application: Mô tả 1 ứng dụng trong hệ sinh thái của công ty (CompanyID)
[ID] [int] IDENTITY(1,1) NOT NULL,
	[Number] [varchar](20) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Description] [nvarchar](250) NOT NULL,	
	[CompanyID] [int] NOT NULL,
	[AppKey] [nvarchar](250) NOT NULL,
	[Issuer] [nvarchar](250) NOT NULL,
	[Audience] [nvarchar](250) NOT NULL,
	[Expire] [int] NOT NULL,
	[RefExpire] [int] NOT NULL,
	[CreatedBy] [varchar](20) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[ModifiedBy] [varchar](20) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
[IsActive] [bit] NOT NULL,

Notes:
AppKey: là key sinh ra jwt hoặc là trong body request, nếu kg dung jwt
Issuer, Expire, RefExpire la các thuộc tính của JwtConfig
CompanyID khóa ngoại đến bảng Companies

AppObjects: mô tả các object cần quản lý trong 1 App
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AppID] [int] NOT NULL,
	[Number] [varchar](20) NULL,
	[Name] [varchar](50) NOT NULL,
	[Description] [nvarchar](250) NOT NULL,
	[ObjectType] [varchar](20) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](20) NOT NULL,
	[ModifiedOn] [datetime] NULL,
[ModifiedBy] [varchar](20) NULL,
[IsDeleted] [bit] NOT NULL,
[IsActive] [bit] NOT NULL,


Notes:
ObjectType: là enum string: process/view/data
AppID: quan hệ thuộc ứng dụng
Number: có thể trùng khi có nh app, chỉ khác trong 1 App=> AppID+ Number duy nhất (unique)

AppRoles: Đinh nghĩa (đn) 1  role trong 1 app
[ID] [int] IDENTITY(1,1) NOT NULL,
	[Number] [varchar](20) NOT NULL,
	[AppID] [int] NOT NULL,
	[Name] [varchar](250) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](20) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
[ModifiedBy] [varchar](20) NOT NULL,
[IsDeleted] [bit] NOT NULL,
[IsActive] [bit] NOT NULL,

Notes:
AppID: quan hệ thuộc ứng dụng
Number: có thể trùng khi có nh app, chỉ khác trong 1 App=> AppID+ Number duy nhất (unique)

RoleRights: Chi tiết 1 role có các quyền gì
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleID] [int] NOT NULL,
	[AppObjectID] [int] NOT NULL,
	[CanList] [bit] NOT NULL,
	[CanCreate] [bit] NOT NULL,
	[CanRead] [bit] NOT NULL,
	[CanUpdate] [bit] NOT NULL,
	[CanDelete] [bit] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](20) NOT NULL,
[ModifiedBy] [varchar](50) NOT NULL,

Notes:
RoleID: khóa ngoại của bang AppRole, Role ID lấy từ danh sách role trong 1 app 
AppObjectID: lấy từ ds 1 app object của 1 app

UserRoles: mapping user vào các role của các App
	
[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[AppRoleID] [int] NOT NULL,
	
AppUsers: Dn 1 user trong nền tảng mà kg phụ thuộc vào App
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[FullName] [nvarchar](50) NOT NULL,
	[Pwd] [nvarchar](100) NOT NULL,
	[PwdKey] [nvarchar](100) NULL,
	[Email] [nvarchar](50) NULL,
	[Mobile] [varchar](50) NULL,
	[CardNum] [varchar](50) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](20) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[ModifiedBy] [varchar](50) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
[IsActive] [bit] NOT NULL,

Notes:
[Pwd] MK đã mã hóa
	[PwdKey] mã dung để mã hóa
[UserName] : định danh chính thức
	3 định danh sau là optional
[Email] [nvarchar](50) NULL,
	[Mobile] [varchar](50) NULL,
	[CardNum] [varchar](50) NULL,

Có thể dùng cả 4 hình thức định danh trên để login, tuy nhiên tùy tình huống mà cấu hình kiểm soát tính duy nhất của dữ liệu

RefTokenTracking: dung để quản lý việc sinh ra jwt thông qua refreshToken, 
Khi token mới sinh ra sẽ sinh 1 cặp, ref token sẽ dc lưu nhằm mục đích xin lại token khi cần thiết
Khi client yc api vơi 1 token đã bị expired, client sẽ dc thông báo thông qua http response= 401 và sau đó client sẽ thực hiện xin lại jwt mới và tiếp tục gọi hàm

Lúc này ref token cũ sẽ dc đánh dấu đã sử dụng, và ref token mới cũng sẽ sinh ra cho những lần xin tiếp theo, việc lưu ref token sẽ diễn ra trong khoảng TG cấu hình (30 days)

Sau 1 thời gian thì các ref token đã sử dụng sẽ dc xóa hoàn toàn


	[Id] [uniqueidentifier] NOT NULL,
	[HashToken] [nvarchar](50) NOT NULL,
	[IssuedAt] [datetime] NOT NULL,
	[ExpiredAt] [datetime] NOT NULL,
	[IsRevoked] [bit] NOT NULL,
	[IsUsed] [bit] NOT NULL,
	[JwtId] [nvarchar](50) NOT NULL,
	[UserId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](20) NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
[ModifiedBy] [varchar](20) NOT NULL,
Notes:
IsRevoked: 
Nếu một Refresh Token cũ bị hack và kẻ xấu cố dùng lại, hệ thống thấy nó đã IsUsed sẽ lập tức cảnh báo và thu hồi toàn bộ các token hiện có của User đó (vì đây là dấu hiệu của tấn công chiếm quyền).
IsUsed: khi sử dụng để đổi token mới thì thuộc tính này dc cập nhật IsUsed=true
HashToken: Hash của Refresh Token để bảo mật

