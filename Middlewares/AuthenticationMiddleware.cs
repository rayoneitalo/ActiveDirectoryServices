using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Net;

namespace api_ldap.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly string _username = Environment.GetEnvironmentVariable("BASE_DN_USERNAME");
        private readonly string _password = Environment.GetEnvironmentVariable("PASSWORD");
        private readonly string _domain = Environment.GetEnvironmentVariable("DOMAIN");
        public dynamic Authenticate()
        {

            try
            {

                LdapConnection connection = new LdapConnection(new LdapDirectoryIdentifier(_domain), new NetworkCredential()
                {
                    UserName = _username,
                    Password = _password,

                }, AuthType.Basic);

                connection.SessionOptions.ProtocolVersion = 3;
                connection.Bind();

                return connection;

            }
            catch (LdapException e)
            {
                var errorMessage = e.Message.ToString();
                var errorCode = e.ErrorCode.ToString();

                return new { message = errorMessage, error = errorCode };
            }
        }
    }
}