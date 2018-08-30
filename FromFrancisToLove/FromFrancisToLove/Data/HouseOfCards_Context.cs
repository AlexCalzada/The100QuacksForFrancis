using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Models
{
    public class HouseOfCards_Context : DbContext
    {
        public HouseOfCards_Context(DbContextOptions<HouseOfCards_Context> options) : base(options) { }
        public DbSet<@object> Producto { get; set; }
    }
}
