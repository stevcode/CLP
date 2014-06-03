namespace Classroom_Learning_Partner
{
    public enum Operation
    {
        Add,
        Remove,
    }

    public class ObservableCollectionOperation<T>
    {
        public readonly T Value;
        public readonly Operation Operation;

        public static ObservableCollectionOperation<T> Add(T value) { return new ObservableCollectionOperation<T>(value, Operation.Add); }

        public static ObservableCollectionOperation<T> Remove(T value) { return new ObservableCollectionOperation<T>(value, Operation.Remove); }

        public ObservableCollectionOperation(T value, Operation operation)
        {
            Value = value;
            Operation = operation;
        }

        public override int GetHashCode() { return Value.GetHashCode() * (Operation == Operation.Add ? 1 : -1); }

        public override bool Equals(object obj)
        {
            if(!(obj is ObservableCollectionOperation<T>))
            {
                return false;
            }

            var other = obj as ObservableCollectionOperation<T>;
            return Value.Equals(other.Value) && Operation.Equals(other.Operation);
        }
    }
}