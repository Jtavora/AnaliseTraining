using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace treinamento_estagiarios.Models
{
    public class Product
    {
        public int Id { get; set; } // Chave primária
        
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } // Nome do produto

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Preço do produto

        [Column(TypeName = "int")]
        public int UserId { get; set; } // FK para User

        [ForeignKey(nameof(UserId))] // Corrige a referência

        [JsonIgnore] // Prevent serialization loop
        public User User { get; set; } // Relacionamento com User
    }
}