namespace MSA.Shared
{
    public enum EProductMode
    {
        Prod,
        Dev,
        Test
    }
    public enum ELang
    {
        En,
        Vn
    };
    public enum EAppUserType
    {
        UserName,
        Email,
        MobileNo,
        CardNo
    };
    public enum JwtStatus
    {
        InvalidToken,
        TokenIsNotExpired,
        TokenIsNotExist,
        TokenIsUsed,
        IsRevoked,
        AccessTokenIdIsNotMatch,
        Success,
        BadRequest
    };
    public class BOProcessResult
    {
        public bool OK { get; set; }
        public string Message { get; set; } = string.Empty;
        public int NumOfRow { get; set; }
        public object? Content { get; set; }
        public int ErrorNumber { get; set; }

        public BOProcessResult()
        {
            OK = false;
            NumOfRow = -1;
        }

        public static BOProcessResult Success(object? content = null, string message = "Success", int errorNumber = 0)
        {
            return new BOProcessResult { OK = true, Content = content, Message = message, ErrorNumber = errorNumber, NumOfRow = 1 };
        }

        public static BOProcessResult Failure(string message, int errorNumber = 1)
        {
            return new BOProcessResult { OK = false, Message = message, ErrorNumber = errorNumber, NumOfRow = 0 };
        }
    }

    public enum ESendMethod
    {
        Email,
        SMS
    }
    public enum EUserNameType
    {
        MemID,// dung khi tu GUI gui truc tiep memid
        MobileNo, // khi GUI gui Mobile
        Email, // khi GUI gui Email
        CardNo, // GUI
        NameOnCard, //GUI
        SocialNetWork, //dung Social Network de login hoac la key de tra cuu
        Other = 9, //khong ro dung de lam gi (sua tu name=Account)
    }
    public enum EAccountType
    {
        Office = 1, Social = 0
    }
    public enum EGender
    {
        Male = 1, Female = 0, Undefind = -1
    }
    public enum EGenderVN
    { Nam = 1, Nữ = 0, Không_Rõ = -1 };
   
    public enum ESchedulerType
    {
        None = 0,
        WorkDays = 1,
        Daily = 2,
        Weekly = 3,
        Monthly = 4,
        Yearly = 5,
        ByMinutes = 6,
        ByHours = 7,
        ByDays = 8,
        ByMonths = 9,
        ByYears = 10
    }
    public enum ETimeIntervalType
    {
        Day,
        Week,
        Month,
        Year,
        End
    }
    public enum EDataMethod
    {
        Read,
        Create,
        Update,
        Delete
    }
    public enum EApprovalStatus
    {
        Approved = 1, Reject = 2, Pending = 3
    }
}
