using System;

namespace NetCrawler.Boundaries
{
    public class RestRequestTimedOutException : Exception
    {
        public RestRequestTimedOutException() : base("Request Timed Out")
        {
        }
    }

    public class RestRequestFailedException : Exception
    {
        public RestRequestFailedException() : base("Request Was Not Successfull")
        {
        }
    }
}
