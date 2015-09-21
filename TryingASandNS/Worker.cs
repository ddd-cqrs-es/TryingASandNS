using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using AggregateSource.EventStore;
using AggregateSource.EventStore.Resolvers;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using Paramol.Executors;
using Projac;
using static System.Console;

namespace TryingASandNS
{
    public class Worker
    {
        public void Work()
        {
            var credentials = new UserCredentials("admin", "changeit");
            var connection = EventStoreConnection.Create(
                ConnectionSettings.Create().UseConsoleLogger().SetDefaultUserCredentials(credentials),
                new IPEndPoint(IPAddress.Loopback, 1113),
                "SampleEventSourcing");
            connection.ConnectAsync();
            //Thread.Sleep(1000);

            ConfigureProjections(connection);

            var configuration = new EventReaderConfiguration(
                new SliceSize(512),
                new JsonDeserializer(),
                new PassThroughStreamNameResolver(),
                new FixedStreamUserCredentialsResolver(credentials));

            while (true)
            {
                WriteLine("Choose an option:");
                WriteLine("1) Create new customer");
                WriteLine("2) Change customer name");
                WriteLine("3) Add user");
                WriteLine("4) Change user name");
                WriteLine("q) Quit");
                Write("Your choice: ");
                var choice = ReadLine();
                if (choice == "q") break;

                var unitOfWork = new EventStoreUnitOfWork(connection);
                var repository = new EventStoreRepository<Customer>(unitOfWork, configuration);

                switch (choice)
                {
                    case "1":
                        {
                            Write("Enter customer name: ");
                            var id = new CustomerId(Guid.NewGuid());
                            var customer = new Customer(id, ReadLine());
                            repository.Add(id.ToString(), customer);
                        }
                        break;
                    case "2":
                        {
                            Write("Enter customer id: ");
                            var customer = repository.Get(new CustomerId(Guid.Parse(ReadLine())).ToString());
                            Write("Enter new customer name: ");
                            customer.ChangeName(ReadLine());
                        }
                        break;
                    case "3":
                        {
                            Write("Enter customer id: ");
                            var customer = repository.Get(new CustomerId(Guid.Parse(ReadLine())).ToString());
                            Write("Enter user name: ");
                            customer.AddUser(new UserId(Guid.NewGuid()), ReadLine());
                        }
                        break;
                    case "4":
                        {
                            Write("Enter customer id: ");
                            var customer = repository.Get(new CustomerId(Guid.Parse(ReadLine())).ToString());
                            Write("Enter user id: ");
                            var userId = new UserId(Guid.Parse(ReadLine()));
                            Write("Enter new user name: ");
                            customer.ChangeUserName(userId, ReadLine());
                        }
                        break;
                }

                unitOfWork.Commit();
                WriteLine("Saved, press enter to continue");
                ReadLine();
            }
        }

        private void ConfigureProjections(IEventStoreConnection connection)
        {
            var projector = new SqlProjector(
                Resolve.WhenEqualToHandlerMessageType(new CustomerProjection()),
                new TransactionalSqlCommandExecutor(
                    new ConnectionStringSettings(
                        "projac",
                        @"Data Source=.\SQLEXPRESS;Initial Catalog=Projections;Integrated Security=SSPI;",
                        "System.Data.SqlClient"),
                    IsolationLevel.ReadCommitted));
            projector.Project(new object[] {new DropSchema(), new CreateSchema()});

            var position = Position.Start;
            connection.SubscribeToAllFrom(position, false, (subscription, @event) =>
            {
                object data = null;
                try
                {
                    data = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(@event.Event.Data),
                        Type.GetType(@event.Event.EventType, false));
                }
                catch
                {
                    // ignored
                }
                if (data != null) projector.Project(data);
            });
        }
    }

    internal class JsonDeserializer : IEventDeserializer
    {
        public IEnumerable<object> Deserialize(ResolvedEvent resolvedEvent)
        {
            var type = Type.GetType(resolvedEvent.Event.EventType, true);
            using (var stream = new MemoryStream(resolvedEvent.Event.Data))
            {
                using (var reader = new StreamReader(stream))
                {
                    yield return JsonSerializer.CreateDefault().Deserialize(reader, type);
                }
            }
        }
    }
}