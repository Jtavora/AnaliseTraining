using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace treinamento_estagiarios.Request
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<ProductDTO> Products { get; set; }       
    }
}