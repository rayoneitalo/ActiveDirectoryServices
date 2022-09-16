using System.DirectoryServices.Protocols;
using Microsoft.AspNetCore.Mvc;

using api_ldap.Middlewares;
using api_ldap.Models;
using api_ldap.Services;


namespace api_ldap.Controllers
{

    [Route("v1/group")]
    [ApiController]

    public class GroupController : ControllerBase
    {
        public const string SamAccountNameProperty = "samaccountname";
        public const string CanonicalNameProperty = "cn";
        public const string AccountControlProperty = "useraccountcontrol";
        public const string SamAccountTypeProperty = "samaccounttype";
        public const string CreatedAtNameProperty = "whencreated";
        public const string DistinguishedNameProperty = "distinguishedname";
        public const string DescriptionNameProperty = "description";
        public const string MemberOfNameProperty = "memberof";
        public const string MemberNameProperty = "member";
        public const string GivenNameProperty = "givenname";
        public const string UserPrincipalNameProperty = "userPrincipalName";
        public const string NameProperty = "name";
        public const string ObjectCategoryProperty = "objectCategory";
        public const string WhenChangedNameProperty = "whenchanged";

        string[] attributes = { MemberNameProperty, MemberOfNameProperty, WhenChangedNameProperty, CanonicalNameProperty, NameProperty, SamAccountNameProperty, SamAccountTypeProperty, DescriptionNameProperty, ObjectCategoryProperty, DistinguishedNameProperty, CreatedAtNameProperty };

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll(int pageSize)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), "(objectClass=group)", attributes, pageSize);

                if (result == null) return Ok(new { message = "Não foi encontrado nenhum grupo para a listagem." });

                pageSize = pageSize > result.Count ? result.Count : pageSize;
                return Ok(new
                {
                    total = result.Count,
                    result = new Utils.AttributesHelper().GetGroupAttributes(result, attributes).GetRange(0, pageSize).OrderBy(group => group.Cn).ToList(),
                });
            }
            catch (ArgumentException e)
            {
                return Ok(new
                {
                    message = e.Message.ToString()
                });
            }

        }

        [HttpGet("byAccountName/{name}")]
        public async Task<IActionResult> GetGroupByName(string name)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {

                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=group)(name={0}*))", name.Trim()), attributes);

                if (result.Count > 0)
                {

                    return Ok(new { total = result.Count, result = new Utils.AttributesHelper().GetGroupAttributes(result, attributes) });
                }
                else
                {
                    return Ok(new { message = "O grupo especificado não foi encontrado!" });
                }
            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }
        }

        [HttpGet("getAllUsers/{name}")]
        public async Task<IActionResult> GetAllUsersByGroupName(string name)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<dynamic> usersInGroup = new List<dynamic>();

                List<SearchResultEntry> groupList = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=group)(name={0}*))", name.Trim()), attributes);

                if (groupList.Count == 0) return Ok(new { message = "Não foi possível localizar o grupo especificado!" });

                foreach (SearchResultEntry item in groupList)
                {

                    List<SearchResultEntry> userList = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectClass=person)(memberOf={0}))", item.DistinguishedName), attributes);

                    usersInGroup.Add(new { groupName = item.Attributes[NameProperty][0].ToString(), users = new Utils.AttributesHelper().GetUserAttributes(userList, attributes) });
                }

                return Ok(new { total = usersInGroup.Count, result = usersInGroup });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }
        }

        [HttpGet("getAllGroups/{account}")]
        public async Task<IActionResult> GetAllGroupsByUserName(string account)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<dynamic> groupsOfUser = new List<dynamic>();

                List<SearchResultEntry> userList = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=person)(objectClass=user)(|(cn=*{0}*)(SAMAccountName={0})))", account.Trim()), attributes);

                if (userList.Count == 0) return Ok(new { message = "Não foi possível localizar a conta de usuário especificada!" });

                foreach (SearchResultEntry item in userList)
                {

                    List<SearchResultEntry> groupList = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectClass=group)(member={0}))", item.DistinguishedName), attributes);

                    groupsOfUser.Add(new { userNameName = item.Attributes[CanonicalNameProperty][0].ToString(), groups = new Utils.AttributesHelper().GetGroupAttributes(groupList, attributes) });
                }
                return Ok(new { total = groupsOfUser.Count, result = groupsOfUser });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }
        }

        [HttpPut("byGroupName/name")]
        public async Task<IActionResult> Update(GroupModel model, string name)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=group)(name={0}))", name.Trim()), attributes);

                if (result.Count == 0) return Ok(new { message = "Não foi possível localizar o grupo especificado." });


                DirectoryAttributeModification descriptionGroup = new DirectoryAttributeModification();
                descriptionGroup.Name = DescriptionNameProperty;
                descriptionGroup.Add(model.Description);
                descriptionGroup.Operation = DirectoryAttributeOperation.Replace;

                DirectoryAttributeModification[] modifications = { descriptionGroup };

                string distinguishedName = result[0].DistinguishedName;
                string newParentDistinguishedName = result[0].DistinguishedName.ToString().Split(",", 2)[1];
                string newName = $"CN={model.Cn}";

                ModifyRequest modifyRequest = new ModifyRequest(result[0].DistinguishedName, modifications);
                ModifyResponse modifyResponse = (ModifyResponse)connection.SendRequest(modifyRequest);

                DirectoryRequest request = new ModifyDNRequest(distinguishedName, newParentDistinguishedName, newName);
                ModifyDNResponse response = (ModifyDNResponse)connection.SendRequest(request);


                if (modifyResponse.ResultCode != ResultCode.Success) return Ok(new { message = "Houve um problema na atualização do grupo especificado", details = new ArgumentException().Message });

                return Ok(new { message = "O grupo especificado foi atualizada com sucesso" });
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
        public async Task<IActionResult> Create(GroupModel model)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                DirectoryAttributeCollection collection = new DirectoryAttributeCollection() {

                        new DirectoryAttribute(CanonicalNameProperty, model.Cn),
                        new DirectoryAttribute(NameProperty, model.Name),
                        new DirectoryAttribute(DescriptionNameProperty, model.Description),
                };

                model.Ou = model.Ou == "" ? Environment.GetEnvironmentVariable("ROOT_DSE") : model.Ou;

                var response = new LdapService().Create(connection, model, collection, "group");

                if (response.status) return Ok(new { message = "O grupo foi criado com sucesso!" });

                return Ok(new { message = response.message, status = response.status });
            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToList() });
            }

        }

        [HttpPatch("addMember")]
        public async Task<IActionResult> AddMember(string account, string groupName)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });


            try
            {
                List<SearchResultEntry> user = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=person)(objectClass=user)(sAMAccountName={0}))", account.Trim()), null);

                if (user.Count == 0) return Ok(new { message = "A conta de usuário especificada não foi encontrada!" });

                List<SearchResultEntry> group = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=group)(name={0}))", groupName.Trim()), attributes);

                if (group.Count == 0) return Ok(new { message = "Não foi possível localizar o grupo especificado." });

                DirectoryAttributeModification memberGroup = new DirectoryAttributeModification();
                memberGroup.Name = "member";
                memberGroup.Add(user[0].DistinguishedName);
                memberGroup.Operation = DirectoryAttributeOperation.Add;

                ModifyRequest modifyRequest = new ModifyRequest(group[0].DistinguishedName, memberGroup);
                ModifyResponse modifyResponse = (ModifyResponse)connection.SendRequest(modifyRequest);

                if (modifyResponse.ResultCode != ResultCode.Success) return Ok(new { message = "Houve um problema na adição do usuário especificado", details = new ArgumentException().Message });

                return Ok(new { message = "O usuário especificado foi adicionado ao grupo com sucesso" });
            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToList() });
            }
            finally
            {
                connection.Dispose();
            }

        }

        [HttpPatch("removeMember")]
        public async Task<IActionResult> RemoveMember(string account, string groupName)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });


            try
            {
                List<SearchResultEntry> user = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=person)(objectClass=user)(sAMAccountName={0}))", account.Trim()), null);

                if (user.Count == 0) return Ok(new { message = "A conta de usuário especificada não foi encontrada!" });

                List<SearchResultEntry> group = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=group)(name={0}))", groupName.Trim()), attributes);

                if (group.Count == 0) return Ok(new { message = "Não foi possível localizar o grupo especificado." });

                DirectoryAttributeModification memberGroup = new DirectoryAttributeModification();
                memberGroup.Name = "member";
                memberGroup.Add(user[0].DistinguishedName);
                memberGroup.Operation = DirectoryAttributeOperation.Delete;

                ModifyRequest modifyRequest = new ModifyRequest(group[0].DistinguishedName, memberGroup);
                ModifyResponse modifyResponse = (ModifyResponse)connection.SendRequest(modifyRequest);

                if (modifyResponse.ResultCode != ResultCode.Success) return Ok(new { message = "Houve um problema na adição do usuário especificado", details = new ArgumentException().Message });

                return Ok(new { message = "O usuário especificado foi removido com sucesso" });
            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToList() });
            }
            finally
            {
                connection.Dispose();
            }

        }

        [HttpPatch("move")]
        public async Task<IActionResult> Move(GroupModel model, string organizationalUnitName)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=group)(name={0}))", model.Name.Trim()), null);

                if (result.Count == 0) return Ok(new { message = "Não foi possível localizar a conta de usuário especificada." });

                var response = new LdapService().ObjectMovement(connection, result, organizationalUnitName, "group");

                if (response.status == true) return Ok(new { message = response.message });

                return Ok(new { message = response.message });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToList() });
            }

        }

        [HttpDelete("byAccountName/{name}")]
        public async Task<IActionResult> Delete(string name)
        {

            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {

                string[] attributes = { DistinguishedNameProperty };

                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=group)(name={0}))", name.Trim()), null);

                if (result.Count == 0) return Ok(new { message = "Não foi possível localizar o grupo especificado para exclusão." });

                var response = new LdapService().Delete(connection, result);

                if (response.status) return Ok(new { message = "O grupo especificado foi exluído com sucesso!" });

                return Ok(new { message = response.message, status = response.status });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }

        }
    }
}