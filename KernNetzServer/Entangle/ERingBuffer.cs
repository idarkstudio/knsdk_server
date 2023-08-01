namespace KernNetz
{
    public class ERingBuffer<T>
    {
        private readonly T[] Data;
        private int head = 0;
        private int tail = 0;
        private int capacity;
        public ERingBuffer(int capacity)
        {
            Data = new T[capacity];
            this.capacity = capacity;
        }

        public void Enqueue(T data)
        {
            Data[head] = data;
            head++;
            if (head >= capacity) head = 0;
        }

        public T Dequeue()
        {
            var data = Data[tail];
            tail++;
            if (tail >= capacity) tail = 0;
            return data;
        }

        public bool IsEmpty => head == tail;
    }
}
