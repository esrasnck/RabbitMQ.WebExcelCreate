using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCreateWorkerService.Services
{
   public class RabbitMQClientService:IDisposable
    {
        // bunları sürekli kullanıyoruz :| core katmanına bu konabilir. Ceren'ime ve Kübram'a not :)

        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;

        public static string QueueName = "queue-excel-file";

        private readonly ILogger<RabbitMQClientService> _logger;

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;

        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();

            if (_channel is { IsOpen: true })
            {
                return _channel;
            }
            _channel = _connection.CreateModel();  // rabbirmg ile bağlantı kurup, geriye bağlantı dönsün isiyorum

            #region some gereksiz issues
            // /excell create de bunu yaptığım için burada gerek yok. yorum satırına alınır. kuyruk deklare etmeye gerek yok. bunu bind etmeye gerek yok. o yüzden bu üç satır uçtuu

            // _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false); 

            // _channel.QueueDeclare(QueueName, true, false, false, null); // kuyruk oluşturmayı producer kısmında yapıyorum.

            // _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingÊxcel);

            #endregion

            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu. ");

            return _channel;


        }

        public void Dispose()
        {
            _channel?.Close(); // kanal varsa kapatalım
            _channel?.Dispose(); // kanal varsa dispose edelim

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile bağlantı koptu... ");

        }
    }
}
