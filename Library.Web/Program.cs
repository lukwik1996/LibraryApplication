using System;
using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Library.Web
{
    public class Program
    {
        public static IConnection connection;

        public static void Main(string[] args)
        {
            string host = "bear.rmq.cloudamqp.com", user = "xcsjfyxh", password = "hR7htI3GdB2u3IsE7G5wzvM_JsvI9P82";

            connection = GetConnection(host, user, password);

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static IConnection GetConnection(string hostName, string userName, string password)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = hostName;
            connectionFactory.UserName = userName;
            connectionFactory.Password = password;
            connectionFactory.Uri = new Uri("amqp://xcsjfyxh:hR7htI3GdB2u3IsE7G5wzvM_JsvI9P82@bear.rmq.cloudamqp.com/xcsjfyxh");
            return connectionFactory.CreateConnection();
        }

        public static void Send(string queue, string data)
        {
            using (IModel channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue, false, false, false, null);
                channel.BasicPublish(string.Empty, queue, null, Encoding.UTF8.GetBytes(data));
            }
        }
    }
}