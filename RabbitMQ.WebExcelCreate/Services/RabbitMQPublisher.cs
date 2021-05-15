using RabbitMQ.Client;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMQ.WebExcelCreate.Services
{
    public class RabbitMQPublisher
    {

        private readonly RabbitMQClientService _rabbitMQClientService;
        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(CreateExcelMessage createExcelMessage)
        {
            var channel = _rabbitMQClientService.Connect();  //?? => bağlantı kurulu şekilde geldi

            var bodyString = JsonSerializer.Serialize(createExcelMessage);

            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();

            properties.Persistent = true; // mesajlar rabbitMQ içerisinde kaydolsun

            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingÊxcel, basicProperties: properties, body: bodyByte);


        }


    }
}
