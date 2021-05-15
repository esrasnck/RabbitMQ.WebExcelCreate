using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ.WebExcelCreate.Services
{
    public class RabbitMQClientService : IDisposable
    {

        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "ExcelDirectExchange";
        public static string RoutingÊxcel = "excel-route-file";
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
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false); // direkt olarak bir sitede kaydolsun. rabbitMQ ya birşey olursa kaybolmasın.

            _channel.QueueDeclare(QueueName, true, false, false, null); // kuyruk oluşturmayı producer kısmında yapıyorum.

            _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingÊxcel);

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
