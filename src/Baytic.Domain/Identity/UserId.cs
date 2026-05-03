namespace Baytic.Domain.Identity;

public readonly struct UserId : IEquatable<UserId>
{
    public Guid Value { get; }

    public UserId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public override bool Equals(object? obj) => obj is UserId other && Equals(other);

    public bool Equals(UserId other) => Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(UserId left, UserId right) => left.Equals(right);

    public static bool operator !=(UserId left, UserId right) => !(left == right);
}