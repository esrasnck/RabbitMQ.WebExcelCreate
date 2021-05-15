using ClosedXML.Excel;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        // background service hazır eklenmiş.
        private readonly ILogger<Worker> _logger;

        private RabbitMQClientService _rabbitMQClientService;


        // run tarafında context'i ekledim. bu context scoped(yani her istek geldiiğinde, yeni bir context sınıf newleniyor.) bir contex.(AddDbContext default olarak scoped geliyor.) ama bu tarafa contextim singleton. bu durumda bir service provider geçmeliyim yani. bunu için service provider'ı kullancak.

        private readonly IServiceProvider _serviceProvider;

        private IModel _channel;

        // veritabanına bağlanma işlemi kaldı. ben run tarafına bu contexti ekledim. fakat bu context scoped bir context. 
        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitMQClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        { // rabbitmq ya bağlanıyorum.

            _channel =  _rabbitMQClientService.Connect(); // uygulama ilk ayağa kalkıtığında bana bir channel oluşsun
            _channel.BasicQos(0, 1, false);  // boyutu önemli değil. sen bana bir bir gönder. global false olsun. her bir subricber birbir gönder

            
            return base.StartAsync(cancellationToken);
        }

        // uygulama ayağa kalktığı anda, Background servicedeki ExecureAysnc metodu, otomatikman çalışmaya başlıyor. 
        protected override  Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel); // consumer ürettik mi?

            _channel.BasicConsume(RabbitMQClientService.QueueName, false,consumer);  // client servisten gelen kuyruğu dinleyecem. otomatik bilgi versin mi? hayır ben kendim bilgi veriyorum mesajdan. son olarak da consumer veriyorum
            consumer.Received += Consumer_Received;
            return Task.CompletedTask;
       
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);

            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            using var ms = new MemoryStream(); // oluşturduğum excell'i memory de stream olarak tutacam.

            var wb = new XLWorkbook(); // workbook oluşturuyorum
            var ds = new DataSet(); // veritabanı gibi düşünün. buna bir tablo ekliyormuş gibi düşünün.
            ds.Tables.Add(GetTable("products"));

            wb.Worksheets.Add(ds); // tablo oluşturcak benim için
            wb.SaveAs(ms); // oluştuduğun excel tablosunu memory stream e kaydet diyorum. Excell dosyam memory streamde. bellekte. artık bunu gönderebilirim. 

            // artık ben excelCreate deki files controllerındaki upload metodunu çağırabilirim.

            // önce gönderceğimiz file nesnesini oluşturuyoruz.
            MultipartFormDataContent multipartFormDataContent = new();
            multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()),"file",Guid.NewGuid().ToString()+".xlsx"); //http content bir byte array olacak. ikinci isim IFormfile a gönderceğim isimle aynı olacak, sonrasında random isim, sonrasında dosya formatı olacak

            var baseUrl = "https://localhost:44327/api/files";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"{baseUrl}?fileId={createExcelMessage.FileId}", multipartFormDataContent);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"File(Id:{createExcelMessage.FileId}) was created by succesful");
                    _channel.BasicAck(@event.DeliveryTag, false); // basarısızsa mesaj rabit mq ya bildirmeyecek, mesjaı belli bir süre bekletip, başka bir subcriber a göndercek.

                }
            }

        }

        private DataTable GetTable(string tableName) // tabloyu aldık. bunu cloudXml kütüphanesi ile excel e çevircez.
        {
            List<FileCreateWorkerService.Models.Product> products;

            using(var scope = _serviceProvider.CreateScope())
            {

                var context = scope.ServiceProvider.GetRequiredService<NorthwindContext>();

                products = context.Products.ToList();
            }

            DataTable table = new DataTable { TableName = tableName }; // memoryde tablo oluşturuyorum
            table.Columns.Add("ProductId",typeof(int));
            table.Columns.Add("ProductName", typeof(string));
            table.Columns.Add("QuantityPerUnit", typeof(string));
            table.Columns.Add("UnitPrice", typeof(decimal));
            table.Columns.Add("UnitsInStock", typeof(short));

            products.ForEach(x =>
            {
                table.Rows.Add(x.ProductId, x.ProductName, x.QuantityPerUnit, x.UnitPrice, x.UnitsInStock);
            });

            return table;

        }
    }
}
