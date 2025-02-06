using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace treinamento_estagiarios.Request
{
    public class MockUserRequest
    {
        public string Name { get; set; }

        public string Email { get; set; } 

        public List<int> ProductsId { get; set; }
    }
}