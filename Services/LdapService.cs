using System.Diagnostics;
using System.DirectoryServices.Protocols;

namespace api_ldap.Services
{
    public class LdapService
    {
        public List<SearchResultEntry> Search(LdapConnection connection, string baseDN, string filter, string[]? attributes, int pageSize = 500)
        {
            List<SearchResultEntry> results = new List<SearchResultEntry>();

            SearchRequest request = new SearchRequest(
                baseDN,
                filter,
                System.DirectoryServices.Protocols.SearchScope.Subtree,
                attributes
                );

            PageResultRequestControl prc = new PageResultRequestControl((int)pageSize);
            SearchOptionsControl soc = new SearchOptionsControl(System.DirectoryServices.Protocols.SearchOption.DomainScope);

            //add the paging control
            request.Controls.Add(prc);
            request.Controls.Add(soc);
            int pages = 0;
            while (true)
            {
                pages++;
                SearchResponse? response = connection.SendRequest(request, TimeSpan.FromSeconds(5)) as SearchResponse;

                if (response.Entries.Count > 0)
                {
                    //find the returned page response control
                    foreach (DirectoryControl control in response.Controls)
                    {
                        if (control is PageResultResponseControl)
                        {
                            //update the cookie for next set
                            prc.Cookie = ((PageResultResponseControl)control).Cookie;
                            break;
                        }
                    }

                    //add them to our collection
                    foreach (SearchResultEntry sre in response.Entries)
                    {
                        results.Add(sre);
                    }
                }

                else
                {
                    return results;
                }

                //our exit condition is when our cookie is empty
                if (prc.Cookie.Length == 0)
                {
                    Trace.WriteLine("Warning GetAllAdSdsp exiting in paged search wtih cookie = zero and page count =" + pages + " and user count = " + results.Count);
                    break;
                }
            }

            pageSize = pageSize > results.Count ? results.Count : pageSize;

            return results.GetRange(0, (int)pageSize);
        }
        public Object Create(LdapConnection connection, dynamic model, DirectoryAttributeCollection collection, string objectClass)
        {
            try
            {
                string attributeName = objectClass == "organizationalUnit" ? "ou" : "cn";

                AddRequest request = new AddRequest(String.Format("{0}={1},{2}", attributeName, objectClass == "organizationalUnit" ? model.Name : model.Cn, model.Ou), objectClass);
                request.Attributes.AddRange(collection);

                AddResponse response = (AddResponse)connection.SendRequest(request);
                if (response.ResultCode.ToString() == "Success") return new { message = "O objeto foi criado com sucesso!", status = true };

                return new { message = "Ocorreu um problema ao criar o objeto.", status = false };
            }
            catch (DirectoryOperationException e)
            {
                Trace.WriteLine(e.Message);
                return new { message = e.Message, status = false };
            }
            finally
            {
                connection.Dispose();
            }
        }
        public Object ObjectMovement(LdapConnection connection, List<SearchResultEntry> entry, string organizationalUnitName, string objectClassType)
        {

            string attributeIdentification = String.Empty;
            string attributeNameProperty = String.Empty;

            switch (objectClassType)
            {
                case "organizationalUnit":
                    attributeIdentification = "OU";
                    attributeNameProperty = "name";
                    break;
                default:
                    attributeIdentification = "CN";
                    attributeNameProperty = "cn";
                    break;
            }

            try
            {
                List<SearchResultEntry> organizationalUnitResult = this.Search(connection, Environment.GetEnvironmentVariable("ROOT_DSE"), String.Format("(&(objectCategory=organizationalUnit)(name={0}))", organizationalUnitName.Trim()), null);

                string distinguishedName = entry[0].DistinguishedName;
                string newParentDistinguishedName = organizationalUnitResult[0].DistinguishedName;
                string newName = $"{attributeIdentification}={entry[0].Attributes[$"{attributeNameProperty}"][0].ToString()}";

                DirectoryRequest request = new ModifyDNRequest(distinguishedName, newParentDistinguishedName, newName);
                ModifyDNResponse response = (ModifyDNResponse)connection.SendRequest(request);

                if (response.ResultCode == ResultCode.Success) return new { message = "A movimentação do objeto foi concluída com sucesso!", status = true };

                return new { message = "Não foi possível realizar a movimentação do objeto", status = false };
            }
            catch (DirectoryOperationException e)
            {
                Trace.WriteLine(e.Message);
                return new { message = e.Message, status = false };
            }
            finally
            {
                connection.Dispose();
            }
        }
        public Object Delete(LdapConnection connection, List<SearchResultEntry> entry)
        {
            try
            {
                DeleteRequest request = new DeleteRequest(entry[0].DistinguishedName);
                DeleteResponse response = (DeleteResponse)connection.SendRequest(request);

                if (response.ResultCode == ResultCode.Success) return new { message = "Objeto exluído com sucesso!", status = true };

                return new { message = "Não foi possível a exclusão do objeto.", status = false };

            }
            catch (DirectoryOperationException e)
            {
                return new { message = e.Message, status = false };
            }
            finally
            {
                connection.Dispose();
            }
        }
    }
}
