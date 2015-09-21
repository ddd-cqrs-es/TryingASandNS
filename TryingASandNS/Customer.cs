using System;
using System.Collections.Generic;
using AggregateSource;

namespace TryingASandNS
{
    public class Customer : AggregateRootEntity
    {
        public CustomerId Id { get; private set; }
        public string Name { get; private set; }

        private List<User> _users;

        protected Customer()
        {
            Register<CustomerCreated>(When);
            Register<CustomerNameChanged>(When);
            Register<NewUserAdded>(When);
            Register<UserNameChanged>(When);
        }

        public Customer(CustomerId id, string customerName) : this()
        {
            ApplyChange(new CustomerCreated(id, customerName));
        }

        public void ChangeName(string newName)
        {
            ApplyChange(new CustomerNameChanged(Id, newName));
        }

        public void AddUser(UserId userId, string userName)
        {
            ApplyChange(new NewUserAdded(Id, userId, userName));
        }

        public void ChangeUserName(UserId userId, string newUserName)
        {
            ApplyChange(new UserNameChanged(userId, newUserName));
        }

        public IEnumerable<User> QueryUsers()
        {
            return _users;
        } 

        private void When(CustomerCreated @event)
        {
            Id = new CustomerId(@event.Id);
            Name = @event.CustomerName;
            _users = new List<User>();
        }

        private void When(CustomerNameChanged @event)
        {
            Name = @event.NewCustomerName;
        }

        private void When(UserNameChanged @event)
        {
            _users.Find(x => x.Id == @event.UserId).Route(@event);
        }

        private void When(NewUserAdded @event)
        {
            var user = new User(ApplyChange);
            user.Route(@event);
            _users.Add(user);
        }

    }

    public class CustomerId : EntityId
    {
        public CustomerId(Guid value) : base(value)
        {
        }
    }

    public class CustomerCreated
    {
        public readonly string CustomerName;
        public readonly Guid Id;

        public CustomerCreated(Guid id, string customerName)
        {
            CustomerName = customerName;
            Id = id;
        }
    }

    public class CustomerNameChanged
    {
        public readonly string NewCustomerName;
        public readonly Guid Id;

        public CustomerNameChanged(Guid id, string newCustomerName)
        {
            Id = id;
            NewCustomerName = newCustomerName;
        }
    }
}