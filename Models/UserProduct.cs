using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace treinamento_estagiarios.Models
{
    public class UserProduct
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }

        // Adicionando JsonIgnore para evitar a serialização do relacionamento
        [JsonIgnore]
        public User User { get; set; }

        [JsonIgnore]
        public Product Product { get; set; }
    }
}