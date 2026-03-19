using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserService.Dtos;

namespace UserService.AsyncDataServices;

public class MessageBusClient : IMessageBusClient
{
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public MessageBusClient(IConfiguration configuration)
    {
        _configuration = configuration;

        if(string.IsNullOrEmpty(_configuration["RabbitMQ:Host"])
            || string.IsNullOrEmpty(_configuration["RabbitMQ:Port"])
        )
        {
            Console.WriteLine("RabbitMQ configuration is missing. Message Bus cannot be initialized.");
            return;
        }

        var factory = new ConnectionFactory() {
                HostName = _configuration["RabbitMQ:Host"]!,
                Port = int.Parse(_configuration["RabbitMQ:Port"]!)
            };

        try
        {
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;

            _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);

            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;

            Console.WriteLine("Connected to Message Bus");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not connect to Message Bus: {ex.Message}");
        }
    }

    public void PublishNewUser(UserPublishedDto userPublishedDto)
    {
        var message = JsonSerializer.Serialize(userPublishedDto);

        if (_connection.IsOpen)
        {
            Console.WriteLine("RabbitMQ connection open, sending message...");
            SendMessage(message);
        }
        else
        {
            Console.WriteLine("RabbitMQ connection closed, not sending");
        }
    }

    private async void SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        var props = new BasicProperties();
        
        await _channel.BasicPublishAsync(exchange: "trigger",
                                    routingKey: "",
                                    mandatory: true,
                                    basicProperties: props,
                                    body: body);
        Console.WriteLine($"Sent {message}");
    }

    public void Dispose()
    {
        Console.WriteLine("MessageBus Disposed");
        if (_channel.IsOpen)
        {
            _channel.CloseAsync();
            _connection.CloseAsync();
        }
    }

    private Task RabbitMQ_ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("RabbitMQ connection shutdown");
        return Task.CompletedTask;
    }
}
