using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AggregateSource;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;

namespace TryingASandNS
{
    public class EventStoreUnitOfWork : UnitOfWork
    {
        public readonly IEventStoreConnection Connection;

        public EventStoreUnitOfWork(IEventStoreConnection connection)
        {
            Connection = connection;
            Connection.ErrorOccurred += (sender, args) => Console.WriteLine(args.Exception.Message);
        }

        public void Commit()
        {
            Parallel.ForEach(GetChanges(), SaveEvents);
        }

        private void SaveEvents(Aggregate affected)
        {
            var events = affected.Root.GetChanges()
                .Select(x => new EventData(Guid.NewGuid(), x.GetType().FullName, true, ToJsonByteArray(x), new byte[0]));

            Connection.AppendToStreamAsync(affected.Identifier, affected.ExpectedVersion, events);
            affected.Root.ClearChanges();
        }

        private static byte[] ToJsonByteArray(object @event)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    JsonSerializer.CreateDefault().Serialize(writer, @event);
                    writer.Flush();
                }
                return stream.ToArray();
            }
        }
    }
}