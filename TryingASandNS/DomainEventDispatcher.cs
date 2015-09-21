using System;
using System.Collections.Generic;

namespace TryingASandNS
{
    public static class DomainEventDispatcher
    {
        [ThreadStatic] private static Dictionary<Type, List<Action<object>>> _index;

        public static IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (_index == null)
                _index = new Dictionary<Type, List<Action<object>>>();
            List<Action<object>> handlers;
            if (!_index.TryGetValue(typeof (TEvent), out handlers))
            {
                handlers = new List<Action<object>>();
                _index.Add(typeof (TEvent), handlers);
            }
            var action = new Action<object>(@event => handler((TEvent) @event));
            handlers.Add(action);
            return new Disposable(() => handlers.Remove(action));
        }

        public static void Publish(object @event)
        {
            if (_index == null) return;
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            List<Action<object>> handlers;
            if (!_index.TryGetValue(@event.GetType(), out handlers)) return;
            foreach (var handler in handlers)
            {
                handler(@event);
            }
        }

        private class Disposable : IDisposable
        {
            private readonly Action _disposer;
            private bool _disposed;

            public Disposable(Action disposer)
            {
                if (disposer == null) throw new ArgumentNullException("disposer");
                _disposer = disposer;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposer();
                _disposed = true;
            }
        }
    }
}