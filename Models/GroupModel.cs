namespace api_ldap.Models
{

    public class GroupModel
    {

        public string Cn { get; set; }
        public string Name { get; set; }
        public string Samaccounttype { get; set; }
        public string Whencreated { get; set; }
        public string whenchanged { get; set; }
        public string Distinguishedname { get; set; }
        public string Description { get; set; }
        public string Memberof { get; set; }
        public string Members { get; set; }
        public string Ou { get; set; }
        public string ObjectCategory { get; set; }

    }

}