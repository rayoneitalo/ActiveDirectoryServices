using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;
using System.Web.Http.Cors;

using api_ldap.Models;
using api_ldap.Middlewares;
using api_ldap.Services;
using api_ldap.Commands.Requests;
using api_ldap.Utils;

namespace api_ldap.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Route("v1/user")]
    [ApiController]

    public class UserController : ControllerBase
    {

        // Propriedades dos Objetos
        public const string CanonicalNameProperty = "cn";
        public const string SecondNameProperty = "sn";
        public const string Initials = "initials";
        public const string SamAccountNameProperty = "samaccountname";
        public const string SamAccountTypeProperty = "samaccounttype";
        public const string CreatedAtNameProperty = "whencreated";
        public const string DistinguishedNameProperty = "distinguishedname";
        public const string DescriptionNameProperty = "description";
        public const string GivenNameProperty = "givenname";
        public const string DisplayNameProperty = "displayname";
        public const string MemberOfNameProperty = "memberof";
        public const string UserPrincipalNameProperty = "userprincipalname";
        public const string TelephoneNumberProperty = "telephonenumber";
        public const string TitleJobProperty = "title";
        public const string DepartamentNameProperty = "department";
        public const string AccountControlProperty = "useraccountcontrol";
        public const string AccountExpiresPropert = "accountexpires";

        public string[] attributes = { CanonicalNameProperty, SecondNameProperty, Initials, SamAccountNameProperty, SamAccountTypeProperty, CreatedAtNameProperty, DistinguishedNameProperty, GivenNameProperty, DisplayNameProperty, MemberOfNameProperty, UserPrincipalNameProperty, TelephoneNumberProperty, TitleJobProperty, DepartamentNameProperty, AccountControlProperty, AccountExpiresPropert, DescriptionNameProperty };
        FormattedDateHelper formattedDateHelper = new Utils.FormattedDateHelper();

        [HttpGet("getAll/{pageSize}")]
        public async Task<IActionResult> GetAll([FromRoute] int pageSize)
        {

            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), "(&(objectCategory=person)(objectClass=user))", attributes, pageSize);

                if (result == null) return Ok(new { message = "Não foi encontrado nenhum usuário para a listagem." });

                pageSize = pageSize > result.Count ? result.Count : pageSize;
                return Ok(new
                {
                    total = result.Count,

                    result = new Utils.AttributesHelper().GetUserAttributes(result, attributes).GetRange(0, pageSize).OrderBy(user => user.Cn).ToList()

                });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }
            finally
            {
                connection.Dispose();
            }

        }

        [HttpGet("byAccountName/{name}")]
        public async Task<IActionResult> GetByAccountName([FromRoute] string name)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=person)(objectClass=user)(|(cn=*{0}*)(SAMAccountName={0})))", name.Trim()), attributes);

                if (result.Count > 0)
                {

                    return Ok(new { total = result.Count, result = new Utils.AttributesHelper().GetUserAttributes(result, attributes) });
                }
                else
                {
                    return Ok(new { message = "A conta de usuário especificada não foi encontrada!" });
                }

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }
            finally
            {
                connection.Dispose();
            }

        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] UserModel user)
        {

            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                // string accountDisabledFlag = user.Isdisabled != false ? "514" : "544"; // Flag that defines whether the user account will be created disabled
                string accountDisabledFlag = user.Isenabled != false ? "544" : "514"; // Flag that defines whether the user account will be created disabled

                DirectoryAttributeCollection collection = new DirectoryAttributeCollection() {

                        new DirectoryAttribute(CanonicalNameProperty, user.Cn),
                        new DirectoryAttribute(SamAccountNameProperty, user.Samaccountname),
                        new DirectoryAttribute(DescriptionNameProperty, user.Description),
                        new DirectoryAttribute(AccountExpiresPropert, formattedDateHelper.FormattedDateToFileTime(user.Accountexpires)),
                        new DirectoryAttribute(AccountControlProperty, accountDisabledFlag)

                    };

                user.Ou = user.Ou == "" ? Environment.GetEnvironmentVariable("ROOT_DSE") : user.Ou;

                var response = new LdapService().Create(connection, user, collection, "user");

                bool statusCode = response.status;

                if (statusCode == true) return Ok(new { message = response.message });

                return Ok(new { message = response.message });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToList() });
            }
        }

        [HttpPut("update/{samaccountname}")]
        public async Task<IActionResult> Update([FromBody] IUpdateUserRequest user, [FromRoute] string samaccountname)
        {

            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=person)(objectClass=user)(sAMAccountName={0}))", samaccountname.Trim()), null);

                if (result.Count == 0) return Ok(new { message = "Não foi possível localizar a conta de usuário especificada." });

                string accountDisabledFlag = user.Isenabled != false ? "544" : "514"; // Flag that defines whether the user account will be created disabled

                DirectoryAttributeModification disabledAccount = new DirectoryAttributeModification();
                disabledAccount.Name = AccountControlProperty;
                disabledAccount.Add(accountDisabledFlag);
                disabledAccount.Operation = DirectoryAttributeOperation.Replace;

                DirectoryAttributeModification descriptionAccount = new DirectoryAttributeModification();
                descriptionAccount.Name = DescriptionNameProperty;
                descriptionAccount.Add(user.Description);
                disabledAccount.Operation = DirectoryAttributeOperation.Replace;

                DirectoryAttributeModification samaccountName = new DirectoryAttributeModification();
                samaccountName.Name = SamAccountNameProperty;
                samaccountName.Add(user.Samaccountname);
                disabledAccount.Operation = DirectoryAttributeOperation.Replace;

                DirectoryAttributeModification expiresAccount = new DirectoryAttributeModification();
                expiresAccount.Name = AccountExpiresPropert;
                expiresAccount.Add(formattedDateHelper.FormattedDateToFileTime(user.Accountexpires));
                expiresAccount.Operation = DirectoryAttributeOperation.Replace;

                DirectoryAttributeModification[] modifications = { disabledAccount, descriptionAccount, samaccountName, expiresAccount };

                ModifyRequest modifyRequest = new ModifyRequest(result[0].DistinguishedName, modifications);
                ModifyResponse modifyResponse = (ModifyResponse)connection.SendRequest(modifyRequest);

                if (modifyResponse.ResultCode != ResultCode.Success) return Ok(new { message = "Houve um problema na atualização da conta especificada", details = new ArgumentException().Message });

                return Ok(new { message = "A conta de usuário foi atualizada com sucesso" });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }
            finally
            {
                connection.Dispose();
            }
        }

        [HttpPatch("movement/{organizationName}")]
        public async Task<IActionResult> Movement([FromBody] IMovementeUserRequest user, [FromRoute] string organizationName)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=person)(objectClass=user)(|(cn=*{0}*)(SAMAccountName={0})))", user.Samaccountname.Trim()), attributes);

                if (result.Count == 0) return Ok(new { message = "Não foi possível localizar a conta de usuário especificada." });

                var response = new LdapService().ObjectMovement(connection, result, organizationName, "user");

                if (response.status == true) return Ok(new { message = response.message });

                return Ok(new { message = response.message });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToList() });
            }

        }

        [HttpDelete("delete/{samaccountname}")]
        public async Task<IActionResult> Delete(string samaccountname)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {

                string[] attributes = { DistinguishedNameProperty, AccountControlProperty, DescriptionNameProperty };

                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=person)(objectClass=user)(sAMAccountName={0}))", samaccountname.Trim()), null);

                if (result.Count == 0) return Ok(new { message = "Não foi possível localizar a conta de usuário especificada." });

                DirectoryAttributeModification disabledAccount = new DirectoryAttributeModification();
                disabledAccount.Name = AccountControlProperty;
                disabledAccount.Add("514");
                disabledAccount.Operation = DirectoryAttributeOperation.Replace;

                DirectoryAttributeModification[] modifications = { disabledAccount };

                ModifyRequest modifyRequest = new ModifyRequest(result[0].DistinguishedName, modifications);
                ModifyResponse modifyResponse = (ModifyResponse)connection.SendRequest(modifyRequest);

                if (modifyResponse.ResultCode != ResultCode.Success) return Ok(new { message = "Houve um problema na exclusão da conta especificada", details = new ArgumentException().Message });

                return Ok(new { message = "A conta de usuário foi desabilitada com sucesso!" });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }
            finally
            {
                connection.Dispose();
            }

        }
    }
}