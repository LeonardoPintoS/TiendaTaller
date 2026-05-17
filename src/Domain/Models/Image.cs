using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TiendaTaller.src.Domain.Models
{
    public class Image
    {
        public int Id { get; set; }
        public required string ImageUrl { get; set; }
        public required string PublicId { get; set; } // ID público de Cloudinary para limpieza de medios

        // Relación con Producto
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}