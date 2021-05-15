using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileCreateWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;  // configurasyon.getConnectionString bu sekilde yakalanıyor. startupda direk olarak, DI dan geliyordu.
                    services.AddDbContext<NorthwindContext>(options =>
                    {
                        options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
                    });
                    services.AddSingleton<RabbitMQClientService>();
                    services.AddHostedService<Worker>(); // ben istersem bir başka worker service de ekleyebilirim.
                    services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(configuration.GetConnectionString("RabbitMQ")), DispatchConsumersAsync = true });
                    // connectionFactory'i burada kullandık. o yüzden start-updaki gibi buraya da ekliyoruz. 
                });
    }
}
// startupdaki service gibi burada da bir service var. configure service üzerinden beni geliyor. hem static dosyalarımı tutabileceğim appsettin.json var
//hem DI container desteği geliyor.  tek işi çalışmak. ne dış dünyaya data sunar ne de bir arayüz vardır. hiç birşey yoktur.


