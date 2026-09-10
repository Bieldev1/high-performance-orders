namespace Domain.SeedWork;

public abstract class Entity : IEntity
{
    public override bool Equals(object? obj)
    {
        if (obj is null or not Entity)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (GetType() != obj.GetType())
            return false;

        return base.Equals(obj);
    }

    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null)
            return right is null;

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
