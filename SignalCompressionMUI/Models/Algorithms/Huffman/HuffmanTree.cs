using System.Collections.Generic;
using System.Linq;

namespace SignalCompressionMUI.Models.Algorithms.Huffman
{
    public class HuffmanTree
    {
        public HuffmanTreeNode Root;

        public HuffmanTree()
        {
            Root = new HuffmanTreeNode(new HuffmanTreeNode(null), new HuffmanTreeNode(null))
            {
                Left = { Weight = 1, Num = 1, IsLeaf = true, EofOrEsc = true },
                Rigth = { Weight = 1, Num = 2, IsLeaf = true, EofOrEsc = false },
                Weight = 2,
                Num = 3,
                IsLeaf = false,
                EofOrEsc = null
            };
            Root.Left.Parent = Root;
            Root.Rigth.Parent = Root;
        }

        public bool IsRoot(HuffmanTreeNode node) => node.Parent == null;

        public void ReplaceNode(HuffmanTreeNode oldNode, HuffmanTreeNode newNode)
        {
            if (!IsRoot(oldNode))
            {
                newNode.Parent = oldNode.Parent;
                if (oldNode.Parent.Rigth != null && oldNode.Parent.Rigth == oldNode)
                    oldNode.Parent.Rigth = newNode;
                if (oldNode.Parent.Left != null && oldNode.Parent.Left == oldNode)
                    oldNode.Parent.Left = newNode;
            }
            else
            {
                Root = newNode;
            }
        }

        //добавление символа в дерево которого еще не было, уже завернутого в treenode тип
        public void AddNode(HuffmanTreeNode node, HuffmanTreeNode addingNode)
        {
            //обход дерева до самого легкого элемента (редкого)

            if (node.IsLeaf) //или если ушли не туда
            {
                return;
            }

            //if (node.Left.Value == "eof" || node.Left.Value == "esc") //дошли до нужного узла
            //if (node.Left.EofOrEsc == true || node.Left.EofOrEsc == false)
            if (node.Left.EofOrEsc == false || node.Rigth.EofOrEsc == false)
            {
                //добавляем
                HuffmanTreeNode newNode = new HuffmanTreeNode(addingNode, node) { Parent = node.Parent, IsLeaf = false };

                //новый узел на место старого
                ReplaceNode(node, newNode);

                newNode.Weight = newNode.Rigth.Weight + newNode.Left.Weight;
                node.Parent = newNode;
                addingNode.Parent = newNode;

                //после надо обновить все веса до корня, и номера
                UpdateNodeWeights(newNode.Parent);
                UpdateNums();

                return;
            }

            //заходить надо только в более легкий узел
            if ((node.Left != null && !node.Left.IsLeaf) && (node.Rigth != null && !node.Rigth.IsLeaf))
            {
                //if (node.Left.Weight == node.Rigth.Weight)
                //{
                AddNode(node.Left, addingNode);
                AddNode(node.Rigth, addingNode);
                //}
                //if (node.Left.Weight > node.Rigth.Weight)
                //    AddNode(node.Left, addingNode);
                //else
                //    AddNode(node.Rigth, addingNode);
            }
            else
            {
                if (node.Left != null && !node.Left.IsLeaf)
                    AddNode(node.Left, addingNode);
                if (node.Rigth != null && !node.Rigth.IsLeaf)
                    AddNode(node.Rigth, addingNode);
            }
        }

        public void FindNodeEofOrEsc(HuffmanTreeNode currentNode, bool? eofOrEscValue, List<bool> path, bool? fromLeft, ref List<bool> findPath, ref HuffmanTreeNode findedNode)
        {
            if (fromLeft != null)
                path.Add(fromLeft == true ? false : true);
            //path += (fromLeft == true) ? "0" : "1";

            if (currentNode.IsLeaf)
            {
                if (eofOrEscValue == currentNode.EofOrEsc)
                {
                    findPath = path;
                    findedNode = currentNode;
                }
                return;
            }

            var path2 = path.ToList();

            if (currentNode.Rigth != null) FindNodeEofOrEsc(currentNode.Rigth, eofOrEscValue, path, false, ref findPath, ref findedNode);
            if (currentNode.Left != null) FindNodeEofOrEsc(currentNode.Left, eofOrEscValue, path2, true, ref findPath, ref findedNode);
        }

        public void FindNode(HuffmanTreeNode currentNode, byte? value, List<bool> path, bool? fromLeft, ref List<bool> findPath, ref HuffmanTreeNode findedNode)
        {
            if (fromLeft != null)
                path.Add(fromLeft != true);
            //path += (fromLeft ==  true) ? "0" : "1";

            if (currentNode.IsLeaf)
            {
                if (value == currentNode.Value)
                {
                    findPath = path;
                    findedNode = currentNode;
                }
                return;
            }
            var path2 = path.ToList();

            if (currentNode.Rigth != null) FindNode(currentNode.Rigth, value, path, false, ref findPath, ref findedNode);
            if (currentNode.Left != null) FindNode(currentNode.Left, value, path2, true, ref findPath, ref findedNode);
        }

        public void UpdateNodeWeights(HuffmanTreeNode parentNode)
        {
            //пересчет весов с node и наверх
            if (parentNode == null) return;
            else
            {
                parentNode.Weight = parentNode.Left.Weight + parentNode.Rigth.Weight;
                UpdateNodeWeights(parentNode.Parent);
            }
        }

        private Queue<HuffmanTreeNode> _queue;
        public List<HuffmanTreeNode> treeList;

        //обновление номеров после добавления или перестановок. нумерует в обратном порядке методом обхода в ширину.
        //(можно и в прямом, но тогда нужно знать количество узлов)
        public void UpdateNums()
        {
            _queue = new Queue<HuffmanTreeNode>();
            _queue.Enqueue(Root);

            //заодно в список дерево перегоним в нужном порядке
            treeList = new List<HuffmanTreeNode>();

            int i = 0;
            while (_queue.Count > 0)
            {
                HuffmanTreeNode node = _queue.Dequeue();
                node.Num = i++;
                treeList.Add(node);
                if (node.Rigth != null) _queue.Enqueue(node.Rigth);
                if (node.Left != null) _queue.Enqueue(node.Left);
            }

            treeList.Reverse(); //потому что все было в обратном порядке, теперь хорошо
        }

        public void ExchangeNodes(HuffmanTreeNode node1, HuffmanTreeNode node2)
        {
            //если надо обменять просто ветви одного узла между собой (смена родителей не поможет)
            if (node1.Parent == node2.Parent)
            {
                var tmp = node1.Parent.Rigth;
                node1.Parent.Rigth = node1.Parent.Left;
                node1.Parent.Left = tmp;

                //еще и веса надо пересчитать. и номера.
                UpdateNodeWeights(node1.Parent);
                UpdateNums();
            }
            else
            {
                //изменяем только связи, т.е. вместе с узлами будет происходить обмен поддеревьями, если они есть
                var node1Parent = node1.Parent;

                if (node2.Parent.Left != null && node2.Parent.Left == node2)
                    node2.Parent.Left = node1;
                if (node2.Parent.Rigth != null && node2.Parent.Rigth == node2)
                    node2.Parent.Rigth = node1;

                if (node1Parent.Left != null && node1Parent.Left == node1)
                    node1Parent.Left = node2;
                if (node1Parent.Rigth != null && node1Parent.Rigth == node1)
                    node1Parent.Rigth = node2; //

                node1.Parent = node2.Parent;
                node2.Parent = node1Parent;

                //проверка не поменялся ли корень 
                if (IsRoot(node1)) Root = node1;
                if (IsRoot(node2)) Root = node2;

                //еще и веса надо пересчитать. и номера.
                UpdateNodeWeights(node1.Parent);
                UpdateNodeWeights(node2.Parent);
                UpdateNums();
            }
        }

        public void UpdateTree(HuffmanTreeNode changedNode)
        {
            //обмен узлов если не отвечают условию упорядоченности (чем больше номер - тем больше вес, в порядке неубывания)
            //внимание! номера то у нас в обратном порядке, значит наоборот!
            //а еще нельзя с родителем обменивать..

            bool flag = true;

            while (flag)
            {
                flag = false;

                HuffmanTreeNode badNode = null;
                for (int i = 0; i < treeList.Count - 1; i++)
                {
                    if (badNode != null)
                    {
                        if (treeList[i].Weight >= badNode.Weight)
                        {
                            if (badNode.Parent != treeList[i - 1])
                            {
                                ExchangeNodes(treeList[i - 1], badNode);
                                flag = true;
                                break;
                            }
                            else
                            {
                                badNode = treeList[i - 1];
                                continue;
                            }
                        }
                    }

                    if (treeList.ElementAt(i).Weight > treeList.ElementAt(i + 1).Weight)
                    {
                        badNode = treeList.ElementAt(i);
                    }
                }
            }
        }

    }
}
