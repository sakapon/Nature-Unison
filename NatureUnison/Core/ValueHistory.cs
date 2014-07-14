using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatureUnison
{
    public class ValueHistory<T>
    {
        public int MaxLength { get; private set; }
        public Queue<T> History { get; private set; }
        public bool IsFull { get { return History.Count == MaxLength; } }

        public ValueHistory(int maxLength)
        {
            if (maxLength <= 0) throw new ArgumentOutOfRangeException("maxLength", maxLength, "The value must be larger than 0.");

            MaxLength = maxLength;
            History = new Queue<T>(maxLength);
        }

        public void UpdateValue(T value)
        {
            if (IsFull)
            {
                History.Dequeue();
            }
            History.Enqueue(value);
        }
    }

    public class ShortValueHistory<T>
    {
        public T Previous { get; private set; }
        public T Current { get; private set; }

        public ShortValueHistory(T defaultValue)
        {
            Previous = defaultValue;
            Current = defaultValue;
        }

        public void UpdateValue(T value)
        {
            Previous = Current;
            Current = value;
        }
    }
}
