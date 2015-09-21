using System;

namespace TryingASandNS
{
    public class EntityId: IEquatable<EntityId>
    {
        private readonly Guid _value;

        public EntityId(Guid value)
        {
            _value = value;
        } 

        public bool Equals(EntityId other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == GetType() && Equals((EntityId)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static implicit operator Guid(EntityId id)
        {
            return id._value;
        }

        public override string ToString()
        {
            return GetType().Name + "-" + _value;
        }
    }
}