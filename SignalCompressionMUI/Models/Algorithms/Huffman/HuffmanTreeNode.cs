namespace SignalCompressionMUI.Models.Algorithms.Huffman
{
    public class HuffmanTreeNode
    {
        public HuffmanTreeNode(HuffmanTreeNode left, HuffmanTreeNode rigth)
        {
            Left = left;
            Rigth = rigth;
        }

        public HuffmanTreeNode(byte? value)
        {
            Value = value;
        }

        public HuffmanTreeNode Left { get; set; }
        public HuffmanTreeNode Rigth { get; set; }
        public HuffmanTreeNode Parent { get; set; }
        public byte? Value { get; set; }
        public int Weight { get; set; }
        public int Num { get; set; }
        public bool IsLeaf { get; set; }

        /// <summary>
        /// true - eof, esc - false, null - ни одно из них
        /// </summary>
        public bool? EofOrEsc { get; set; }
    }
}
