using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        // background service hazır eklenmiş.
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        // uygulama ayağa kalktığı anda, Background servicedeki ExecureAysnc metodu, otomatikman çalışmaya başlıyor. 
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken); // her bir saniyede bir çalışıyor.
            }
        }
    }
}
