using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaTaller.src.Domain.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsDeleted { get; set; } = false; // Requerido para Soft Delete

        // Llaves foráneas y propiedades de navegación
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;

        // Un producto tiene muchas imágenes
        public List<Image> Images { get; set; } = new();
    }
}