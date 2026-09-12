namespace SmartX.Shared.Telemetry
{
    public class TelemetryBuffer<T>
    {
        private readonly List<T> _items = new();

        public int Count => _items.Count;

        public void Add(T item)
        {
            _items.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            _items.AddRange(items);
        }

        public IReadOnlyList<T> GetAll()
        {
            return _items.AsReadOnly();
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
