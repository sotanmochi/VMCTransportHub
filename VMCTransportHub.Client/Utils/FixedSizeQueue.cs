namespace System.Collections.Generic.Extension
{
    public class FixedSizeQueue<T> : Queue<T>
    {
        public int MaxSize => _maxSize;
        private int _maxSize;

        public FixedSizeQueue(int maxsize) : base(maxsize)
        {
            _maxSize = maxsize;
        }

        public new void Enqueue(T item)
        {
            while (this.Count >= _maxSize)
            {
                Dequeue();
            }
            base.Enqueue(item);
        }
    }
}