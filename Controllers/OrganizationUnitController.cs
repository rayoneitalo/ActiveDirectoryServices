using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;
using System.Diagnostics;

using api_ldap.Middlewares;
using api_ldap.Models;
using api_ldap.Services;

namespace api_ldap.Controllers
{
    [Route("v1/organizationUnit")]
    [ApiController]
    public class OrganizationUnitController : ControllerBase
    {
        public const string CanonicalNameProperty = "cn";
        public const string NameProperty = "name";
        public const string SamAccountNameProperty = "sAMAccountName";
        public const string SamAccountTypeProperty = "sAMAccountType";
        public const string DescriptionNameProperty = "description";
        public const string ObjectCategoryProperty = "objectCategory";
        public const string DistinguishedNameProperty = "distinguishedName";
        public const string CreatedAtNameProperty = "whenCreated";


        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll(int pageSize)
        {
            var connection = new AuthenticationMiddleware().Authenticate();


            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<OrganizationUnitModel> organizationUnits = new List<OrganizationUnitModel>();

                string[] attributes = { NameProperty, DistinguishedNameProperty, CreatedAtNameProperty };

                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), "(objectCategory=organizationalUnit)", attributes, pageSize);

                foreach (SearchResultEntry item in result)
                {
                    OrganizationUnitModel organizationUnit = new OrganizationUnitModel();
                    var utils = new Utils.FormattedDateHelper();

                    organizationUnit.Distinguishedname = item.Attributes[DistinguishedNameProperty][0].ToString();
                    organizationUnit.Name = item.Attributes[NameProperty][0].ToString();
                    organizationUnit.Whencreated = utils.FormattedDate(item.Attributes[CreatedAtNameProperty][0].ToString());

                    organizationUnits.Add(organizationUnit);
                }

                pageSize = pageSize > result.Count ? result.Count : pageSize;
                return Ok(new
                {
                    total = organizationUnits.Count,
                    result = organizationUnits.GetRange(0, pageSize).FindAll(delegate (OrganizationUnitModel organizationUnit)
                {

                    return organizationUnit.Name != null && organizationUnit.Distinguishedname != null;

                }).OrderBy(organizationUnit => organizationUnit.Name).ToList()

                });
            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }

        }

        [HttpGet("byAccountName/{name}")]
        public async Task<IActionResult> GetOrganizationUnitByName(string name)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<OrganizationUnitModel> organizationUnits = new List<OrganizationUnitModel>();

                string[] attributes = { NameProperty, DistinguishedNameProperty, CreatedAtNameProperty };

                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=organizationalUnit)(name={0}*))", name.Trim()), attributes);

                if (result.Count > 0)
                {
                    foreach (SearchResultEntry item in result)
                    {
                        OrganizationUnitModel organizationUnit = new OrganizationUnitModel();
                        var utils = new Utils.FormattedDateHelper();

                        organizationUnit.Distinguishedname = item.Attributes[DistinguishedNameProperty][0].ToString();
                        organizationUnit.Name = item.Attributes[NameProperty][0].ToString();
                        organizationUnit.Whencreated = utils.FormattedDate(item.Attributes[CreatedAtNameProperty][0].ToString());

                        organizationUnits.Add(organizationUnit);
                    }


                    return Ok(new { total = organizationUnits.Count, result = organizationUnits });
                }
                else
                {
                    return Ok(new { message = "A unidade organizacional especificada não foi encontrada!" });
                }

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }

        }


        [HttpPost("create")]
        public async Task<IActionResult> Create(OrganizationUnitModel model)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {

                DirectoryAttributeCollection collection = new DirectoryAttributeCollection() {


                        new DirectoryAttribute(NameProperty, model.Name),
                        new DirectoryAttribute(DescriptionNameProperty, model.Description),

                    };

                model.Ou = model.Ou == "" ? Environment.GetEnvironmentVariable("ROOT_DSE") : model.Ou;

                var status = new LdapService().Create(connection, model, collection, "organizationalUnit");

                bool statusCode = status.status;

                if (statusCode == true) return Ok(new { message = status.message });

                return Ok(new { message = status.message });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToList() });
            }

        }

        [HttpPut("byOrganizationalUnitName/{name}")]
        public async Task<IActionResult> Update(OrganizationUnitModel model, string name)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=organizationalUnit)(name={0}))", name.Trim()), null);

                if (result.Count == 0) return Ok(new { message = "Não foi possível localizar a Unidade Organizacional especificada." });

               DirectoryAttributeCollection collection = new DirectoryAttributeCollection() {
                        new DirectoryAttribute(DescriptionNameProperty, model.Description),
                    };

                DirectoryAttributeModification organizationalUnitName = new DirectoryAttributeModification();
                organizationalUnitName.Name = DescriptionNameProperty;
                organizationalUnitName.Add(model.Description);
                organizationalUnitName.Operation = DirectoryAttributeOperation.Replace;

                DirectoryAttributeModification[] modifications = { organizationalUnitName };

                string distinguishedName = result[0].DistinguishedName;
                string newParentDistinguishedName = result[0].DistinguishedName.ToString().Split(",", 2)[1];
                string newName = $"OU={model.Name}";

                ModifyRequest modifyRequest = new ModifyRequest(result[0].DistinguishedName, modifications);
                ModifyResponse modifyResponse = (ModifyResponse)connection.SendRequest(modifyRequest);

                DirectoryRequest request = new ModifyDNRequest(distinguishedName, newParentDistinguishedName, newName);
                ModifyDNResponse response = (ModifyDNResponse)connection.SendRequest(request);

                if (modifyResponse.ResultCode != ResultCode.Success) return Ok(new { message = "Houve um problema na atualização da Unidade Organizacional especificada", details = new ArgumentException().Message });

                return Ok(new { message = "A Unidade Organizacional foi atualizada com sucesso" });

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



        [HttpPatch("move")]
        public async Task<IActionResult> Move(OrganizationUnitModel model, string organizationalUnitName)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {
                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=organizationalUnit)(name={0}))", model.Name.Trim()), null);

                if (result.Count == 0) return Ok(new { message = "Não foi possível localizar a conta de usuário especificada." });

                var response = new LdapService().ObjectMovement(connection, result, organizationalUnitName, "organizationalUnit");

                if (response.status == true) return Ok(new { message = response.message });

                return Ok(new { message = response.message });

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToList() });
            }

        }

        [HttpDelete("byAccountName/{account}")]

        public async Task<IActionResult> Delete(string account)
        {
            var connection = new AuthenticationMiddleware().Authenticate();

            if (!(connection is LdapConnection)) return Ok(new { details = connection });

            try
            {

                string[] attributes = { DistinguishedNameProperty };

                List<SearchResultEntry> result = new LdapService().Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=organizationalUnit)(name={0}))", account.Trim()), attributes);

                if (result.Count > 0)
                {
                    var request = new DeleteRequest(result[0].DistinguishedName);

                    Trace.WriteLine(result[0].DistinguishedName);

                    connection.SendRequest(request);

                    return Ok(new { message = "Unidade Organizacional excluída com sucesso!" });
                }
                else
                {
                    return Ok(new { message = "Não foi possível localizar a Unidade Organizacioanl especificada para exclusão." });
                }

            }
            catch (ArgumentException e)
            {
                return Ok(new { message = e.Message.ToString() });
            }

        }
    }
}