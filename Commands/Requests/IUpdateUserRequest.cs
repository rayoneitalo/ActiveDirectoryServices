namespace api_ldap.Commands.Requests
{
    public class IUpdateUserRequest
    {
        public string Accountname { get; set; }
        public bool Isenabled { get; set; }
        public string Samaccountname { get; set; }
        public string Description { get; set; }
        public string? Accountexpires { get; set; }
    }

    public class IMovementeUserRequest
    {
        public string Samaccountname { get; set; }
    }

    public class ICreateUserRequest
    {
        public string Cn { get; set; } // Nome Completo da conta de usuário
        public string? Ou { get; set; } // Caminho da Unidade Organizacional da conta de usuário
        public string? Sn { get; set; } // Definição do segundo nome da conta de usuário
        public string Samaccountname { get; set; } // Usuário de login da conta de usuário
        public string? Description { get; set; } // Descrição da conta de usuário
        public bool Isenabled { get; set; } // Definição dos status da conta de usuário
        public string? Accountexpires { get; set; } // Definição da data de expiração da conta de usuário
        public string? Givenname { get; set; } // Definição do primeiro nome da conta de usuário
        public string? Initials { get; set; } // Definição das iniciais da conta de usuário
        public string? Displayname { get; set; } // Definição do nome de apresentação da conta de usuário
        public string? Mail { get; set; } // Definição dos e-mail da conta de usuário
        public string? Telephonenumber { get; set; } // Definição dos telefone de contato da conta de usuário
        public string? Userprincipalname { get; set; } // Definição do nome de logon da conta de usuário
    }
}