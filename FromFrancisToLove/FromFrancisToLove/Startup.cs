using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FromFrancisToLove.Models;
using FromFrancisToLove.Data;
using Microsoft.EntityFrameworkCore;

namespace FromFrancisToLove
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
          //  MySqlConnection connection = new MySqlConnection("server = 104.196.236.172; user id = Rene; persistsecurityinfo = True; database = HouseOfCards");
            //  services.AddDbContext<HouseOfCards_Context>(options => options.(connection));

            services.AddDbContext<HouseOfCards_Context>(options =>
            {
                options.UseMySql("server = 104.196.236.172; user id = Rene; pwd = qwerty; persistsecurityinfo = True; database = HouseOfCards");
            
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

         

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
