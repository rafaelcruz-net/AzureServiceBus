using Dapper;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Consumer
{
    class Program
    {
        const string ServiceBusConnectionString = "{Service EndPoint}";
        const string QueueName = "{Queue Name}";
        static IQueueClient queueClient;
        const string ConnectionString = "Server=tcp:samples-db.database.windows.net,1433;Initial Catalog=azure-service-bus;Persist Security Info=False;User ID=samples;Password=a123456A;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        static void Main(string[] args)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            Console.ReadKey();

            queueClient.CloseAsync().Wait();
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            var messageBody = Encoding.UTF8.GetString(message.Body);

            var bug = JsonConvert.DeserializeObject<BugModel>(messageBody);

            CreateNewBug(bug);

            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private static void CreateNewBug(BugModel obj)
        {
            using (var sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Execute("INSERT INTO BUG([Id], [Title], [Description], [User], [Severity]) VALUES (@Id, @Title, @Description, @User, @Severity)", obj);
            }
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
