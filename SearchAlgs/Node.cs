using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAlgs
{
    internal class Node
    {
        public int[,] state;
        public Tuple<int, int> emptyPos;
        public Node? left, right, up, down;
        public Node? parent;
        public int cost;
        public int depth;

        public Node(int[,] state, Tuple<int, int> emptyPos)
        {
            this.state = new int[state.GetLength(0), state.GetLength(1)];
            Array.Copy(state, this.state, state.Length);
            this.emptyPos = emptyPos;
            parent = null;
            left = null;
            right = null;
            up= null;
            down = null;
        }
        public Node(int[,] state, Tuple<int, int> emptyPos, Node parent)
        {
            this.state = new int[state.GetLength(0), state.GetLength(1)];
            Array.Copy(state, this.state, state.Length);
            this.emptyPos = emptyPos;
            this.parent = parent;
            left = null;
            right = null;
            up = null;
            down = null;
        }
    }
}
