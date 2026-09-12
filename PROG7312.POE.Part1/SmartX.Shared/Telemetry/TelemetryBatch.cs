namespace SmartX.Shared.Telemetry
{
    public class TelemetryBatch<T>
    {
        public T[][] RawBatches { get; set; }

        public TelemetryBatch(T[][] rawBatches)
        {
            RawBatches = rawBatches;
        }

        public List<T> ToList()
        {
            List<T> flattenedTelemetry = new();

            foreach (T[] batch in RawBatches)
            {
                flattenedTelemetry.AddRange(batch);
            }

            return flattenedTelemetry;
        }

        public int GetTotalReadingCount()
        {
            int total = 0;

            foreach (T[] batch in RawBatches)
            {
                total += batch.Length;
            }

            return total;
        }
    }
}
