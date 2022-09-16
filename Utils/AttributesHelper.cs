using System.DirectoryServices;
using System.DirectoryServices.Protocols;

using api_ldap.Models;

namespace api_ldap.Utils
{
    public class AttributesHelper
    {
        FormattedDateHelper formattedDateHelper = new Utils.FormattedDateHelper();
        ObjectTypeAdapter objectTypeAdapter = new ObjectTypeAdapter();

        public List<UserModel> GetUserAttributes(List<SearchResultEntry> results, string[] attributes)
        {
            List<UserModel> users = new List<UserModel>();


            foreach (SearchResultEntry item in results)
            {

                UserModel user = new UserModel();

                foreach (string nameProperty in attributes)
                {
                    switch (nameProperty)
                    {
                        case "cn":
                            user.Cn = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "sn":
                            user.Sn = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "initials":
                            user.Initials = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "telephonenumber":
                            user.Telephonenumber = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "title":
                            user.Jobtitle = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "department":
                            user.Department = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "samaccountname":
                            user.Samaccountname = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "userprincipalname":
                            user.Userprincipalname = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "samaccounttype":
                            user.Samaccounttype = item.Attributes.Contains(nameProperty) ? objectTypeAdapter.TypeProperty(item.Attributes[nameProperty][0].ToString()) : null;
                            break;
                        case "useraccountcontrol":

                            if (item.Attributes.Contains(nameProperty))
                            {
                                if (item.Attributes[nameProperty][0].ToString() == "514")
                                {
                                    user.Isenabled = false;
                                }
                                else
                                {

                                    user.Isenabled = true;
                                }
                            }

                            break;
                        case "whencreated":
                            user.Whencreated = item.Attributes.Contains(nameProperty) ? formattedDateHelper.FormattedDate(item.Attributes[nameProperty][0].ToString()) : null;
                            break;
                        case "accountexpires":
                            user.Accountexpires = item.Attributes.Contains(nameProperty) ? formattedDateHelper.FormattedFileTimeToDate(item.Attributes[nameProperty][0].ToString()) : null;
                            break;
                        case "distinguishedname":
                            user.Distinguishedname = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "description":
                            user.Description = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "givenname":
                            user.Givenname = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "memberof":

                            if (item.Attributes.Contains(nameProperty))
                            {
                                List<Byte[]> members = new List<Byte[]>();

                                foreach (byte[] value in item.Attributes[nameProperty])
                                {

                                    members.Add(value);
                                }
                            }

                            user.Memberof = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "userPrincipalName":
                            user.Userprincipalname = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        default:
                            break;
                    }
                }

                users.Add(user);
            }

            return users;

        }

        public List<GroupModel> GetGroupAttributes(List<SearchResultEntry> results, string[] attributes)
        {
            List<GroupModel> groups = new List<GroupModel>();

            foreach (SearchResultEntry item in results)
            {

                GroupModel group = new GroupModel();

                foreach (string nameProperty in attributes)
                {
                    switch (nameProperty)
                    {
                        case "cn":
                            group.Cn = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "name":
                            group.Name = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "samaccounttype":
                            group.Samaccounttype = item.Attributes.Contains(nameProperty) ? objectTypeAdapter.TypeProperty(item.Attributes[nameProperty][0].ToString()) : null;
                            break;
                        case "whencreated":
                            group.Whencreated = item.Attributes.Contains(nameProperty) ? formattedDateHelper.FormattedDate(item.Attributes[nameProperty][0].ToString()) : null;
                            break;
                        case "whenchanged":
                            group.whenchanged = item.Attributes.Contains(nameProperty) ? formattedDateHelper.FormattedDate(item.Attributes[nameProperty][0].ToString()) : null;
                            break;
                        case "distinguishedname":
                            group.Distinguishedname = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "description":
                            group.Description = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "memberof":
                            group.Memberof = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        case "member":
                            group.Members = item.Attributes.Contains(nameProperty) ? item.Attributes[nameProperty][0].ToString() : null;
                            break;
                        default:
                            break;
                    }
                }

                groups.Add(group);
            }

            return groups;
        }

        // private enum AdsUserFlags
        // {
        //     Script = 1,                  // 0x1
        //     AccountDisabled = 2,              // 0x2
        //     HomeDirectoryRequired = 8,           // 0x8 
        //     AccountLockedOut = 16,             // 0x10
        //     PasswordNotRequired = 32,           // 0x20
        //     PasswordCannotChange = 64,           // 0x40
        //     EncryptedTextPasswordAllowed = 128,      // 0x80
        //     TempDuplicateAccount = 256,          // 0x100
        //     NormalAccount = 512,              // 0x200
        //     InterDomainTrustAccount = 2048,        // 0x800
        //     WorkstationTrustAccount = 4096,        // 0x1000
        //     ServerTrustAccount = 8192,           // 0x2000
        //     PasswordDoesNotExpire = 65536,         // 0x10000
        //     MnsLogonAccount = 131072,           // 0x20000
        //     SmartCardRequired = 262144,          // 0x40000
        //     TrustedForDelegation = 524288,         // 0x80000
        //     AccountNotDelegated = 1048576,         // 0x100000
        //     UseDesKeyOnly = 2097152,            // 0x200000
        //     DontRequirePreauth = 4194304,          // 0x400000
        //     PasswordExpired = 8388608,           // 0x800000
        //     TrustedToAuthenticateForDelegation = 16777216, // 0x1000000
        //     NoAuthDataRequired = 33554432         // 0x2000000
        // }
    }
}