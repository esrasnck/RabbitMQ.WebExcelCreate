
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ.WebExcelCreate.Hubs
{
    public class MyHub:Hub
    {
        // her hangi bir metot tanımlamıycam. ben client a bilgi göndercem. clientlar bana bilgi gönderecek. çift taraflı bir iletişim var

        // chat e bir data yazıldığında client backend ile haberleşmiş oluyor. backend gelen chat datasını clientlara gönderdiğinde, backend client ile haberleşmiş oluyor.
    }
}
