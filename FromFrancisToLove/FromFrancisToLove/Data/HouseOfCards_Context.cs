using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FromFrancisToLove.Models;
using FromFrancisToLove.Requests;

namespace FromFrancisToLove.Data
{
    public class HouseOfCards_Context : DbContext
    {
        public HouseOfCards_Context(DbContextOptions<HouseOfCards_Context> options) : base(options) { }
        public DbSet<Conexion_Config> conexion_Configs { get; set; }
        public DbSet<Tienda_Config> tienda_Configs { get; set; }
        public DbSet<Catalogos_Producto> catalogos_Productos { get; set; }
        public DbSet<Transaccion> transaccions { get; set; }

        public DbSet<Transaccion2> transaccion2s { get; set; }

    }
}
