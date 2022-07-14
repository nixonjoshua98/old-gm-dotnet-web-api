namespace GMServer.Common
{
    public static class Guard
    {
        public static void ThrowIfNotNull(object value, string? message = null)
        {
            if (value is not null)
                throw new System.Exception(message);
        }
    }
}
