using System;
using System.Reflection;
using AggregateSource;
using AggregateSource.EventStore;

namespace TryingASandNS
{
    public class EventStoreRepository<T> : Repository<T> where T : IAggregateRootEntity
    {
        public EventStoreRepository(EventStoreUnitOfWork unitOfWork, EventReaderConfiguration configuration) 
            : base(AggregateFactory, unitOfWork, unitOfWork.Connection, configuration)
        {
        }

        private static T AggregateFactory()
        {
            var ctor = typeof (T).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new Type[0], null);
            return (T) ctor.Invoke(new object[0]);
        }
    }
}