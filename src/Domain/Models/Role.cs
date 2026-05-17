using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaTaller.src.Domain.Models
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}