using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gestor.RedisCache.Models
{
    public class ClienteDTO
    {
        public string CPF { get; set; }

        public string Endereco { get; set; }

        public string Nome { get; set; }
    }
}
