using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace treinamento_estagiarios.Models
{
    public class User
    {
        public int Id { get; set; } // Chave primária
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } // Nome do usuário
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; } // Email do usuário
        public ICollection<Product> Products { get; set; } // Relacionamento com Products
    }
}