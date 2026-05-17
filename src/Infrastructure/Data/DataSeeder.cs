using Bogus;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TiendaTaller.src.Domain.Models;

namespace TiendaTaller.src.Infrastructure.Data
{
    public class DataSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<DataContext>();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                // REEMPLAZO: Eliminamos EnsureCreatedAsync() y dejamos solo MigrateAsync()
                Log.Information("Verificando y aplicando migraciones en SQLite...");
                await context.Database.MigrateAsync();

                var genders = configuration.GetSection("User:Genders").Get<string[]>() ?? throw new ArgumentNullException("Generos not found in configuration");
                if (!await context.Roles.AnyAsync())
                {
                    var roles = new List<Role>
                    {
                        new Role {Name = "Admin"},
                        new Role {Name = "Customer"}
                    };
                    await context.Roles.AddRangeAsync(roles);
                    await context.SaveChangesAsync();
                    Log.Information("Roles creados exitosamente");
                }

                if (!await context.Categories.AnyAsync())
                {
                    var categories = new List<Category>
                    {
                        new Category { Name = "Ropa"},
                        new Category { Name = "Electronica"},
                        new Category { Name = "hogar"},
                        new Category { Name = "Libros"},
                        new Category { Name = "Juguetes"},
                    };
                    await context.Categories.AddRangeAsync(categories);
                    await context.SaveChangesAsync();
                    Log.Information("Categorias creadas exitosamente");
                }
                if (!await context.Brands.AnyAsync())
                {
                    var brands = new List<Brand>
                    {   new Brand { Name = "Nike"},
                        new Brand { Name = "Adidas"},
                        new Brand { Name = "Apple"},
                        new Brand { Name = "Samsung"},
                        new Brand { Name = "Sony"},
                    };
                    await context.Brands.AddRangeAsync(brands);
                    await context.SaveChangesAsync();
                    Log.Information("Marcas creadas exitosamente");
                }
                if (!await context.Users.AnyAsync())
                {
                    //Roles
                    Role customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer") ?? throw new Exception("Customer role not found");
                    Role adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin") ?? throw new Exception("Admin role not found");

                    //admin
                    User adminUser = new User
                    {
                        Name = configuration["User:AdminUser:Name"] ?? throw new InvalidOperationException("No se pudo cargar el nombre del administrador"),
                        Email = configuration["User:AdminUser:Email"] ?? throw new InvalidOperationException("No se pudo cargar el email del administrador"),
                        EmailConfirmed = true,
                        Rut = configuration["User:AdminUser:Rut"] ?? throw new InvalidOperationException("No se pudo cargar el rut del administrador"),
                        PhoneNumber = configuration["User:AdminUser:PhoneNumber"] ?? throw new InvalidOperationException("No se pudo cargar el número de teléfono del administrador"),
                        BirthDate = DateTime.Parse(configuration["User:AdminUser:BirthDate"] ?? throw new InvalidOperationException("No se pudo cargar la fecha de nacimiento del administrador")),
                        Gender = configuration["User:AdminUser:Gender"] ?? throw new InvalidOperationException("No se pudo cargar el genero del administrador"),
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(configuration["User:AdminUser:Password"] ?? throw new InvalidOperationException("No se pudo cargar la contraseña del administrador")),
                        RoleId = adminRole.Id
                    };
                    await context.Users.AddAsync(adminUser);
                    await context.SaveChangesAsync();
                    Log.Information("Usuario administrador creado exitosamente");
                    //User prueba
                    var randomPasswordHash = BCrypt.Net.BCrypt.HashPassword(configuration["User:RandomUserPassword"] ?? throw new InvalidOperationException("No se pudo cargar la contraseña del usuario de prueba"));
                    var UserFaker = new Faker<User>()
                        .RuleFor(u => u.Name, f => f.Name.FullName())
                        .RuleFor(u => u.Email, f => f.Internet.Email())
                        .RuleFor(u => u.EmailConfirmed, true)
                        .RuleFor(u => u.Rut, f => RandomRut())
                        .RuleFor(u => u.PhoneNumber, f => RandomPhoneNumber())
                        .RuleFor(u => u.BirthDate, f => f.Date.Past(30, DateTime.Now.AddYears(-18))) //>18 anios
                        .RuleFor(u => u.Gender, f => f.PickRandom(genders))
                        .RuleFor(u => u.PasswordHash, randomPasswordHash)
                        .RuleFor(u => u.RoleId, customerRole.Id);
                    var users = UserFaker.Generate(99);
                    await context.Users.AddRangeAsync(users);
                    await context.SaveChangesAsync();
                    Log.Information("Usuarios de prueba creados exitosamente");
                }

                //Productos e Imagenes
                if (!await context.Products.AnyAsync())
                {
                    var categoryIds = await context.Categories.Select(c => c.Id).ToListAsync();
                    var brandIds = await context.Brands.Select(b => b.Id).ToListAsync();

                    var imageFaker = new Faker<Image>()
                        .RuleFor(i => i.ImageUrl, f => f.Image.PicsumUrl())
                        .RuleFor(i => i.PublicId, f => f.Random.Guid().ToString());

                    var productFaker = new Faker<Product>()
                        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                        .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(1000, 100000)))
                        .RuleFor(p => p.Stock, f => f.Random.Int(0, 100))
                        .RuleFor(p => p.CategoryId, f => f.PickRandom(categoryIds))
                        .RuleFor(p => p.BrandId, f => f.PickRandom(brandIds))
                        .RuleFor(p => p.Images, f => imageFaker.Generate(f.Random.Int(1, 3)));

                    var products = productFaker.Generate(50);
                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();
                    Log.Information("Productos e imagenes de prueba creados exitosamente");
                }
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Error al aplicar migraciones a la database", ex.Message);
            }
        }

        private static string RandomRut()
        {
            var faker = new Faker();
            var number = faker.Random.Int(10000000, 99999999).ToString();
            var verificar = faker.Random.Int(0, 9).ToString();
            return $"{number}-{verificar}";
        }

        private static string RandomPhoneNumber()
        {
            var faker = new Faker();
            string firstPart = faker.Random.Int(1000, 9999).ToString();
            string secondPart = faker.Random.Int(1000, 9999).ToString();
            return $"+569 {firstPart}-{secondPart}";
        }
    }
}