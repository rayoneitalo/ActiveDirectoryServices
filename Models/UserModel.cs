namespace api_ldap.Models
{
    public class UserModel
    {
        public string Cn { get; set; }
        public string Sn { get; set; }
        public string Initials { get; set; }
        public string Telephonenumber { get; set; }
        public string Jobtitle { get; set; }
        public string Department { get; set; }
        public string Samaccountname { get; set; }
        public string Samaccounttype { get; set; }
        public string Whencreated { get; set; }
        public string Distinguishedname { get; set; }
        public string Description { get; set; }
        public string Givenname { get; set; }
        public string Memberof { get; set; }
        public string Userprincipalname { get; set; }
        public string Ou { get; set; }
        public bool Isenabled { get; set; }
        public string? Accountexpires { get; set; }
    }
}
