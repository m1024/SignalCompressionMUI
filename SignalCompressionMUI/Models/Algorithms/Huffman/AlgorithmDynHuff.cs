using System.Collections.Generic;
using System.Linq;

namespace SignalCompressionMUI.Models.Algorithms.Huffman
{
    public static class AlgorithmDynHuff
    {
        public static byte[] Encode(byte[] input, ref HuffmanTree tree)
        {
            var result = new List<bool>();

            for (int i = 0; i < input.Length; i++)
            {
                var pathRez = new List<bool>();
                var findedNode = new HuffmanTreeNode(null, null);
                tree.FindNode(tree.Root, input[i], new List<bool>(), null, ref pathRez, ref findedNode);

                if (pathRez.Count == 0)  //нету, надо добавить
                {
                    tree.FindNodeEofOrEsc(tree.Root, false, new List<bool>(), null, ref pathRez, ref findedNode);
                    result.InsertRange(result.Count, pathRez);
                    result.InsertRange(result.Count, BitHelper.ByteToBoolBits(input[i]));

                    tree.AddNode(tree.Root, new HuffmanTreeNode(input[i]) { IsLeaf = true, Weight = 1, EofOrEsc = null });
                    tree.UpdateTree(tree.Root);
                }
                else //уже есть в дереве, увеличить вес
                {
                    findedNode.Weight++;
                    tree.UpdateNodeWeights(findedNode.Parent); //пересчет весов
                    tree.UpdateNums(); //возможно и зря
                    tree.UpdateTree(tree.Root);

                    result.InsertRange(result.Count, pathRez);
                }

            }

            //надо закодированное в byte[] перевести, а поскольку число бит наверняка не кратно 8, 
            //в первый байт запишем сколько бит в последнем
            var bytes = BitHelper.BoolsToBytes(result);
            return bytes;
        }

        public static byte[] Encode(byte[] input)
        {
            var tree = new HuffmanTree();
            var result = new List<bool>();

            for (int i = 0; i < input.Length; i++)
            {
                var pathRez = new List<bool>();
                var findedNode = new HuffmanTreeNode(null, null);
                tree.FindNode(tree.Root, input[i], new List<bool>(), null, ref pathRez, ref findedNode);

                if (pathRez.Count == 0)  //нету, надо добавить
                {
                    tree.FindNodeEofOrEsc(tree.Root, false, new List<bool>(), null, ref pathRez, ref findedNode);
                    result.InsertRange(result.Count, pathRez);
                    result.InsertRange(result.Count, BitHelper.ByteToBoolBits(input[i]));

                    tree.AddNode(tree.Root, new HuffmanTreeNode(input[i]) { IsLeaf = true, Weight = 1, EofOrEsc = null });
                    tree.UpdateTree(tree.Root);
                }
                else //уже есть в дереве, увеличить вес
                {
                    findedNode.Weight++;
                    tree.UpdateNodeWeights(findedNode.Parent); //пересчет весов
                    tree.UpdateNums(); //возможно и зря
                    tree.UpdateTree(tree.Root);

                    result.InsertRange(result.Count, pathRez);
                }

            }

            //надо закодированное в byte[] перевести, а поскольку число бит наверняка не кратно 8, 
            //в первый байт запишем сколько бит в последнем
            var bytes = BitHelper.BoolsToBytes(result);
            return bytes;
        }


        public static byte[] Decode(byte[] data)
        {
            var result = BitHelper.BytesToBools(data);

            var tree = new HuffmanTree();
            HuffmanTreeNode current = tree.Root;
            var decoded = new List<byte>();

            //попытка декодирования
            for (int i = 0; i < result.Count; i++)
            {
                var bit = result[i];
                if (bit == true)
                {
                    if (current.Rigth != null)
                        current = current.Rigth;
                }
                else if (bit == false)
                {
                    if (current.Left != null)
                        current = current.Left;
                }

                if (current.IsLeaf)
                {
                    //if (current.Value == "esc") //первый раз символ встретился, не закодирован
                    if (current.EofOrEsc == false)
                    {
                        //считать символ, переместить указатель
                        //считывать надо byte, 8 значений
                        decoded.Add(BitHelper.BitsToByte(result.GetRange(++i, 8)));
                        i += 7;
                        //decoded += result[++i];

                        //необходимо перестроить дерево
                        tree.AddNode(tree.Root, new HuffmanTreeNode(decoded.Last()) { IsLeaf = true, Weight = 1, EofOrEsc = null });
                        tree.UpdateTree(tree.Root);
                        current = tree.Root;
                    }
                    else
                    {
                        //string pathRez = "";
                        var pathRez = new List<bool>();
                        var findedNode = new HuffmanTreeNode(null, null);
                        tree.FindNode(tree.Root, current.Value, new List<bool>(), null, ref pathRez, ref findedNode);
                        findedNode.Weight++;
                        tree.UpdateNodeWeights(findedNode.Parent); //пересчет весов
                        tree.UpdateNums(); //возможно и зря
                        tree.UpdateTree(tree.Root);

                        //decoded += current.Value;
                        decoded.Add((byte)current.Value);
                        current = tree.Root;
                    }
                }
            }

            return decoded.ToArray();
        }
    }
}
