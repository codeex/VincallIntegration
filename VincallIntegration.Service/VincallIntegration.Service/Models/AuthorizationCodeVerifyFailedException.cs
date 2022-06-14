using System;
using System.Runtime.Serialization;

namespace VincallIntegration.Service.Controllers
{
    [Serializable]
    internal class AuthorizationCodeVerifyFailedException : Exception
    {
        public AuthorizationCodeVerifyFailedException()
        {
        }

        public AuthorizationCodeVerifyFailedException(string message) : base(message)
        {
        }

        public AuthorizationCodeVerifyFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AuthorizationCodeVerifyFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}