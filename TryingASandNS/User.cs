using System;
using AggregateSource;

namespace TryingASandNS
{
    public class User : Entity
    {
        public UserId Id { get; private set; }
        public string UserName { get; private set; }

        public User(Action<object> applier) : base(applier)
        {
            Register<NewUserAdded>(When);
            Register<UserNameChanged>(When);
        }

        private void When(UserNameChanged @event)
        {
            UserName = @event.NewUserName;
        }

        private void When(NewUserAdded @event)
        {
            Id = new UserId(@event.UserId);
            UserName = @event.UserName;
        }
    }

    public class UserId : EntityId
    {
        public UserId(Guid value) : base(value)
        {
        }
    }

    public class NewUserAdded
    {
        public readonly Guid CustomerId;
        public readonly Guid UserId;
        public readonly string UserName;

        public NewUserAdded(Guid customerId, Guid userId, string userName)
        {
            CustomerId = customerId;
            UserId = userId;
            UserName = userName;
        }
    }

    public class UserNameChanged
    {
        public readonly Guid UserId;
        public readonly string NewUserName;

        public UserNameChanged(Guid userId, string newUserName)
        {
            UserId = userId;
            NewUserName = newUserName;
        }
    }

}