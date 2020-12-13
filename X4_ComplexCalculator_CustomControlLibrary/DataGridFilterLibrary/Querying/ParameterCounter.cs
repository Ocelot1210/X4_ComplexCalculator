namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Querying
{
    public class ParameterCounter
    {
        private int _Count = 0;
        public int ParameterNumber => _Count - 1;

        public ParameterCounter()
        {
        }

        public ParameterCounter(int count)
        {
            _Count = count;
        }

        public void Increment() => _Count++;

        public void Decrement() => _Count--;

        public override string ToString()
            => ParameterNumber.ToString();
    }
}
