namespace GMServer.MediatR
{
    public abstract class AbstractLoginResponse : AbstractResponseWithError
    {
        public GetUserDataResponse UserData;
    }
}
