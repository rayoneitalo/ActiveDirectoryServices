namespace api_ldap.Utils
{
    public class ObjectTypeAdapter
    {
        public string TypeProperty(string propertyValue)
        {
            long _propertyValue = long.Parse(propertyValue);
            String samaccounttype;

            switch (_propertyValue)
            {
                case 0:
                    samaccounttype = "SAM_DOMAIN_OBJECT";
                    break;
                case 268435456:
                    samaccounttype = "SAM_GROUP_OBJECT";
                    break;
                case 268435457:
                    samaccounttype = "SAM_NON_SECURITY_GROUP_OBJECT";
                    break;
                case 536870912:
                    samaccounttype = "SAM_ALIAS_OBJECT";
                    break;
                case 536870913:
                    samaccounttype = "SAM_NON_SECURITY_ALIAS_OBJECT";
                    break;
                case 805306368:
                    samaccounttype = "SAM_USER_OBJECT || SAM_NORMAL_USER_ACCOUNT";
                    break;
                case 805306369:
                    samaccounttype = "SAM_MACHINE_ACCOUNT";
                    break;
                case 805306370:
                    samaccounttype = "SAM_TRUST_ACCOUNT";
                    break;
                case 1073741824:
                    samaccounttype = "SAM_APP_BASIC_GROUP";
                    break;
                case 1073741825:
                    samaccounttype = "SAM_APP_QUERY_GROUP";
                    break;
                case 0x7fffffff:
                    samaccounttype = "SAM_ACCOUNT_TYPE_MAX";
                    break;
                default:
                    samaccounttype = "Não foi possível identificar o tipo de conta";
                    break;
            }

            return samaccounttype;
        }
    }
}
