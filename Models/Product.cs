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
        public int Id { get; set; } 
        
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } 
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [JsonIgnore]
        public ICollection<UserProduct> UserProducts { get; set; }
    }
}