using SearchAlgs;
using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.NetworkInformation;

class SearchAlgorithms
{
    static public void Main(String[] args)
    {
        /* Goal Puzzle
         * 1 2 3
         * 8 0 4
         * 7 6 5
         */
        int[,] goalState = { { 1, 2, 3 }, { 8, 0, 4 }, { 7, 6, 5 } };

        // The amount of solvable puzzles the algorithms will solve
        int puzzleCount = 20;

        // Depth limit for DFS
        // Max moves needed to solve hardest 8-puzzle is 31 (80 for 15-puzzle)
        int depthLimit = 31;

        RunAlgs(puzzleCount, goalState, depthLimit);
    }
    // Runs the various search algorithms
    public static void RunAlgs(int puzzleCount, int[,] goalState, int depthLimit)
    {
        Tuple<int, int>[] goalPos = new Tuple<int, int>[goalState.Length];
        Tuple<long, int>[] dfsStats = new Tuple<long, int>[puzzleCount];
        Tuple<long, int>[] ucsStats = new Tuple<long, int>[puzzleCount];
        Tuple<long, int>[] bfsStats = new Tuple<long, int>[puzzleCount];
        Tuple<long, int>[] aStarStats = new Tuple<long, int>[puzzleCount];

        int[] goalStateLin = ConvertToLinear(goalState, ref goalPos);

        while(puzzleCount != 0)
        {
            Tuple<int, int> emptyPos = new Tuple<int, int>(0, 0);
            Stopwatch watch = new Stopwatch();
            int[,] initialState;
            int[] initialStateLin;
            int nodesVisited = 0;

            do
            {
                initialState = GeneratePuzzle(goalState.GetLength(0), goalState.GetLength(1), goalStateLin, ref emptyPos);
                initialStateLin = ConvertToLinear(initialState, goalPos);
            }
            while (!PuzzleSolvable(initialStateLin));

            // Starting Depth First Search
            Console.WriteLine("DFS Running");
            Node root = new Node(initialState, emptyPos);
            watch.Start();
            bool dfsSolved = DepthFirstSearch(root, goalState, 0, depthLimit, ref nodesVisited);
            watch.Stop();
            dfsStats[puzzleCount - 1] = Tuple.Create(watch.ElapsedMilliseconds, nodesVisited);
            watch.Reset();
            nodesVisited = 0;
            Console.WriteLine("DFS complete");

            // Starting Uniform Cost Search
            Console.WriteLine("UCS Running");
            root = new Node(initialState, emptyPos);
            watch.Start();
            bool ucsSolved = UniformCostSearch(root, goalState, ref nodesVisited);
            watch.Stop();
            ucsStats[puzzleCount - 1] = Tuple.Create(watch.ElapsedMilliseconds, nodesVisited);
            watch.Reset();
            nodesVisited = 0;
            Console.WriteLine("UCS complete");

            // Starting Best First Search
            Console.WriteLine("BFS Running");
            root = new Node(initialState, emptyPos);
            watch.Start();
            bool bfsSolved = BestFirstSearch(root, goalState, goalPos, depthLimit, ref nodesVisited);
            watch.Stop();
            bfsStats[puzzleCount - 1] = Tuple.Create(watch.ElapsedMilliseconds, nodesVisited);
            watch.Reset();
            nodesVisited = 0;
            Console.WriteLine("BFS complete");

            // Starting A* Search
            Console.WriteLine("A* Running");
            root = new Node(initialState, emptyPos);
            watch.Start();
            bool aStarSolved = AStarSearch(root, goalState, goalPos, ref nodesVisited);
            watch.Stop();
            aStarStats[puzzleCount - 1] = Tuple.Create(watch.ElapsedMilliseconds, nodesVisited);
            watch.Reset();
            nodesVisited = 0;
            Console.WriteLine("A* complete");
            Console.WriteLine();

            DisplayPuzzle(initialState, goalState, dfsSolved, ucsSolved, bfsSolved, aStarSolved);
            puzzleCount--;
        }

        DisplayStats(dfsStats, ucsStats, bfsStats, aStarStats);
    }
    // Performs recursive DFS
    // Goes down paths one at a time until it reaches a depth limit that is set to 31
    // The most difficult 8-puzzle can be solved in 31 steps so if an answer isn't found by that depth it doesn't exist on that path
    // This is done to prevent infinite path traversals
    // Before a path is traversed the node will be checked to see if it is identical to it's grandparent
    // If it is the path won't be traversed. This is to prevent cycles which would prevent the answer from being found
    public static bool DepthFirstSearch(Node currNode, int[,] goalState, int currDepth, int depthLimit, ref int nodesVisited)
    {
        bool goalReached = false;
        nodesVisited++;

        if (CompareArrays(currNode.state, goalState))
        {
            goalReached = true;
            return goalReached;
        }

        if(currDepth != depthLimit)
        {
            bool upValid = (currNode.emptyPos.Item1 - 1 >= 0);
            bool downValid = (currNode.emptyPos.Item1 + 1 < currNode.state.GetLength(0));
            bool leftValid = (currNode.emptyPos.Item2 - 1 >= 0);
            bool rightValid = (currNode.emptyPos.Item2 + 1 < currNode.state.GetLength(1));

            currDepth += 1;

            if (leftValid && !goalReached)
            {
                Tuple<int, int> leftEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1, currNode.emptyPos.Item2 - 1);
                int[,] leftState = SwapValues(currNode.state, leftEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if(!CompareArrays(leftState, currNode.parent.state))
                    {
                        currNode.left = new Node(leftState, leftEmptyPos, currNode);
                        goalReached = DepthFirstSearch(currNode.left, goalState, currDepth, depthLimit, ref nodesVisited);
                    }
                }
                else
                {
                    currNode.left = new Node(leftState, leftEmptyPos, currNode);
                    goalReached = DepthFirstSearch(currNode.left, goalState, currDepth, depthLimit, ref nodesVisited);
                }
            }

            if (rightValid && !goalReached)
            {
                Tuple<int, int> rightEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1, currNode.emptyPos.Item2 + 1);
                int[,] rightState = SwapValues(currNode.state, rightEmptyPos, currNode.emptyPos);
                

                if (currNode.parent != null)
                {
                    if (!CompareArrays(rightState, currNode.parent.state))
                    {
                        currNode.right = new Node(rightState, rightEmptyPos, currNode);
                        goalReached = DepthFirstSearch(currNode.right, goalState, currDepth, depthLimit, ref nodesVisited);
                    }
                }
                else
                {
                    currNode.right = new Node(rightState, rightEmptyPos, currNode);
                    goalReached = DepthFirstSearch(currNode.right, goalState, currDepth, depthLimit, ref nodesVisited);
                }
            }

            if (upValid && !goalReached)
            {
                Tuple<int, int> upEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1 - 1, currNode.emptyPos.Item2);
                int[,] upState = SwapValues(currNode.state, upEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(upState, currNode.parent.state))
                    {
                        currNode.up = new Node(upState, upEmptyPos, currNode);
                        goalReached = DepthFirstSearch(currNode.up, goalState, currDepth, depthLimit, ref nodesVisited);
                    }
                }
                else
                {
                    currNode.up = new Node(upState, upEmptyPos, currNode);
                    goalReached = DepthFirstSearch(currNode.up, goalState, currDepth, depthLimit, ref nodesVisited);
                }
            }

            if (downValid && !goalReached)
            {
                Tuple<int, int> downEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1 + 1, currNode.emptyPos.Item2);
                int[,] downState = SwapValues(currNode.state, downEmptyPos, currNode.emptyPos);
                
                if (currNode.parent != null)
                {
                    if (!CompareArrays(downState, currNode.parent.state))
                    {
                        currNode.down = new Node(downState, downEmptyPos, currNode);
                        goalReached = DepthFirstSearch(currNode.down, goalState, currDepth, depthLimit, ref nodesVisited);
                    }
                }
                else
                {
                    currNode.down = new Node(downState, downEmptyPos, currNode);
                    goalReached = DepthFirstSearch(currNode.down, goalState, currDepth, depthLimit, ref nodesVisited);
                }
            }
        }
        
        return goalReached;
    }
    // Performs UCS using a queue for open nodes and a list for closed nodes
    // The cost of nodes is solely determined by their level in the tree
    // Because of that, this implementation of UCS functions exactly the same as Breadth First Search
    // Nodes are expanded level by level until the solution is found
    // Nodes that are identical to their grandparent aren't expanded for the sake of runtime
    public static bool UniformCostSearch(Node currNode, int[,] goalState, ref int nodesVisited)
    {
        currNode.cost = 0;
        Queue<Node> open = new Queue<Node>();
        List<Node> closed = new List<Node>();

        while(!CompareArrays(currNode.state, goalState))
        {
            closed.Add(currNode);
            nodesVisited++;

            bool upValid = (currNode.emptyPos.Item1 - 1 >= 0);
            bool downValid = (currNode.emptyPos.Item1 + 1 < currNode.state.GetLength(0));
            bool leftValid = (currNode.emptyPos.Item2 - 1 >= 0);
            bool rightValid = (currNode.emptyPos.Item2 + 1 < currNode.state.GetLength(1));

            if(leftValid)
            {
                Tuple<int, int> leftEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1, currNode.emptyPos.Item2 - 1);
                int[,] leftState = SwapValues(currNode.state, leftEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(leftState, currNode.parent.state))
                    {
                        currNode.left = new Node(leftState, leftEmptyPos, currNode);
                        currNode.left.cost = currNode.cost + 1;
                        open.Enqueue(currNode.left);
                    }
                }
                else
                {
                    currNode.left = new Node(leftState, leftEmptyPos, currNode);
                    currNode.left.cost = currNode.cost + 1;
                    open.Enqueue(currNode.left);
                }
            }

            if(rightValid)
            {
                Tuple<int, int> rightEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1, currNode.emptyPos.Item2 + 1);
                int[,] rightState = SwapValues(currNode.state, rightEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(rightState, currNode.parent.state))
                    {
                        currNode.right = new Node(rightState, rightEmptyPos, currNode);
                        currNode.right.cost = currNode.cost + 1;
                        open.Enqueue(currNode.right);
                    }
                }
                else
                {
                    currNode.right = new Node(rightState, rightEmptyPos, currNode);
                    currNode.right.cost = currNode.cost + 1;
                    open.Enqueue(currNode.right);
                }
            }

            if(upValid)
            {
                Tuple<int, int> upEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1 - 1, currNode.emptyPos.Item2);
                int[,] upState = SwapValues(currNode.state, upEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(upState, currNode.parent.state))
                    {
                        currNode.up = new Node(upState, upEmptyPos, currNode);
                        currNode.up.cost = currNode.cost + 1;
                        open.Enqueue(currNode.up);
                    }
                }
                else
                {
                    currNode.up = new Node(upState, upEmptyPos, currNode);
                    currNode.up.cost = currNode.cost + 1;
                    open.Enqueue(currNode.up);
                }
            }

            if(downValid)
            {
                Tuple<int, int> downEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1 + 1, currNode.emptyPos.Item2);
                int[,] downState = SwapValues(currNode.state, downEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(downState, currNode.parent.state))
                    {
                        currNode.down = new Node(downState, downEmptyPos, currNode);
                        currNode.down.cost = currNode.cost + 1;
                        open.Enqueue(currNode.down);
                    }
                }
                else
                {
                    currNode.down = new Node(downState, downEmptyPos, currNode);
                    currNode.down.cost = currNode.cost + 1;
                    open.Enqueue(currNode.down);
                }
            }

            // Because node cost is only determined by its level in the tree...
            // And because all of a node's children are added to the open queue when expanded...
            // The first node in the open queue should always be the least cost in the open queue
            currNode = open.Dequeue(); 
        }


        return true;
    }
    // Performs BFS using a priority queue for open nodes and a list for closed nodes
    // The priority queue in C# uses a min-heap which is perfect because the heuristic used will decrease the solution gets closer
    // All of the current node's children are opened and added to the queue but only the node with the smallest heuristic is expanded
    // Nodes that are identical to their grandparent aren't expanded for the sake of runtime
    // Also has a depth limit of 31 similar to DFS to prevent it from running infinitely
    public static bool BestFirstSearch(Node currNode, int[,] goalState, Tuple<int, int>[] goalPos, int depthLimit, ref int nodesVisited)
    {
        currNode.depth = 0;
        PriorityQueue<Node, int> open = new PriorityQueue<Node, int>(); // The int is the heuristic
        List<Node> closed = new List<Node>();

        while (!CompareArrays(currNode.state, goalState))
        {
            closed.Add(currNode);
            nodesVisited++;

            bool upValid = (currNode.emptyPos.Item1 - 1 >= 0);
            bool downValid = (currNode.emptyPos.Item1 + 1 < currNode.state.GetLength(0));
            bool leftValid = (currNode.emptyPos.Item2 - 1 >= 0);
            bool rightValid = (currNode.emptyPos.Item2 + 1 < currNode.state.GetLength(1));

            if (leftValid && currNode.depth < depthLimit)
            {
                Tuple<int, int> leftEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1, currNode.emptyPos.Item2 - 1);
                int[,] leftState = SwapValues(currNode.state, leftEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(leftState, currNode.parent.state))
                    {
                        currNode.left = new Node(leftState, leftEmptyPos, currNode);
                        currNode.left.depth = currNode.depth + 1;
                        open.Enqueue(currNode.left, CalculateHeuristic(leftState, goalState, goalPos));
                    }
                }
                else
                {
                    currNode.left = new Node(leftState, leftEmptyPos, currNode);
                    currNode.left.depth = currNode.depth + 1;
                    open.Enqueue(currNode.left, CalculateHeuristic(leftState, goalState, goalPos));
                }
            }

            if (rightValid && currNode.depth < depthLimit)
            {
                Tuple<int, int> rightEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1, currNode.emptyPos.Item2 + 1);
                int[,] rightState = SwapValues(currNode.state, rightEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(rightState, currNode.parent.state))
                    {
                        currNode.right = new Node(rightState, rightEmptyPos, currNode);
                        currNode.right.depth = currNode.depth + 1;
                        open.Enqueue(currNode.right, CalculateHeuristic(rightState, goalState, goalPos));
                    }
                }
                else
                {
                    currNode.right = new Node(rightState, rightEmptyPos, currNode);
                    currNode.right.depth = currNode.depth + 1;
                    open.Enqueue(currNode.right, CalculateHeuristic(rightState, goalState, goalPos));
                }
            }

            if (upValid && currNode.depth < depthLimit)
            {
                Tuple<int, int> upEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1 - 1, currNode.emptyPos.Item2);
                int[,] upState = SwapValues(currNode.state, upEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(upState, currNode.parent.state))
                    {
                        currNode.up = new Node(upState, upEmptyPos, currNode);
                        currNode.up.depth = currNode.depth + 1;
                        open.Enqueue(currNode.up, CalculateHeuristic(upState, goalState, goalPos));
                    }
                }
                else
                {
                    currNode.up = new Node(upState, upEmptyPos, currNode);
                    currNode.up.depth = currNode.depth + 1;
                    open.Enqueue(currNode.up, CalculateHeuristic(upState, goalState, goalPos));
                }
            }

            if (downValid && currNode.depth < depthLimit)
            {
                Tuple<int, int> downEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1 + 1, currNode.emptyPos.Item2);
                int[,] downState = SwapValues(currNode.state, downEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(downState, currNode.parent.state))
                    {
                        currNode.down = new Node(downState, downEmptyPos, currNode);
                        currNode.down.depth = currNode.depth + 1;
                        open.Enqueue(currNode.down, CalculateHeuristic(downState, goalState, goalPos));
                    }
                }
                else
                {
                    currNode.down = new Node(downState, downEmptyPos, currNode);
                    currNode.down.depth = currNode.depth + 1;
                    open.Enqueue(currNode.down, CalculateHeuristic(downState, goalState, goalPos));
                }
            }

            currNode = open.Dequeue();
        }

        return true;
    }
    // Performs A* Search using a priority queue for open nodes and a list for closed nodes
    // Basically a combination of UCS and BFS
    // The cost of a node (tree level) and the heuristic are added together to determine the best move
    // Nodes that are identical to their grandparent aren't expanded for the sake of runtime
    public static bool AStarSearch(Node currNode, int[,] goalState, Tuple<int, int>[] goalPos, ref int nodesVisited)
    {
        currNode.cost = 0;
        PriorityQueue<Node, int> open = new PriorityQueue<Node, int>(); // The int is the heuristic + least cost move
        List<Node> closed = new List<Node>();

        while (!CompareArrays(currNode.state, goalState))
        {
            closed.Add(currNode);
            nodesVisited++;

            bool upValid = (currNode.emptyPos.Item1 - 1 >= 0);
            bool downValid = (currNode.emptyPos.Item1 + 1 < currNode.state.GetLength(0));
            bool leftValid = (currNode.emptyPos.Item2 - 1 >= 0);
            bool rightValid = (currNode.emptyPos.Item2 + 1 < currNode.state.GetLength(1));

            if (leftValid)
            {
                Tuple<int, int> leftEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1, currNode.emptyPos.Item2 - 1);
                int[,] leftState = SwapValues(currNode.state, leftEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(leftState, currNode.parent.state))
                    {
                        currNode.left = new Node(leftState, leftEmptyPos, currNode);
                        currNode.left.cost = currNode.cost + 1;
                        open.Enqueue(currNode.left, CalculateHeuristic(leftState, goalState, goalPos) + currNode.left.cost);
                    }
                }
                else
                {
                    currNode.left = new Node(leftState, leftEmptyPos, currNode);
                    currNode.left.cost = currNode.cost + 1;
                    open.Enqueue(currNode.left, CalculateHeuristic(leftState, goalState, goalPos) + currNode.left.cost);
                }
            }

            if (rightValid)
            {
                Tuple<int, int> rightEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1, currNode.emptyPos.Item2 + 1);
                int[,] rightState = SwapValues(currNode.state, rightEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(rightState, currNode.parent.state))
                    {
                        currNode.right = new Node(rightState, rightEmptyPos, currNode);
                        currNode.right.cost = currNode.cost + 1;
                        open.Enqueue(currNode.right, CalculateHeuristic(rightState, goalState, goalPos) + currNode.right.cost);
                    }
                }
                else
                {
                    currNode.right = new Node(rightState, rightEmptyPos, currNode);
                    currNode.right.cost = currNode.cost + 1;
                    open.Enqueue(currNode.right, CalculateHeuristic(rightState, goalState, goalPos) + currNode.right.cost);
                }
            }

            if (upValid)
            {
                Tuple<int, int> upEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1 - 1, currNode.emptyPos.Item2);
                int[,] upState = SwapValues(currNode.state, upEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(upState, currNode.parent.state))
                    {
                        currNode.up = new Node(upState, upEmptyPos, currNode);
                        currNode.up.cost = currNode.cost + 1;
                        open.Enqueue(currNode.up, CalculateHeuristic(upState, goalState, goalPos) + currNode.up.cost);
                    }
                }
                else
                {
                    currNode.up = new Node(upState, upEmptyPos, currNode);
                    currNode.up.cost = currNode.cost + 1;
                    open.Enqueue(currNode.up, CalculateHeuristic(upState, goalState, goalPos) + currNode.up.cost);
                }
            }

            if (downValid)
            {
                Tuple<int, int> downEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1 + 1, currNode.emptyPos.Item2);
                int[,] downState = SwapValues(currNode.state, downEmptyPos, currNode.emptyPos);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(downState, currNode.parent.state))
                    {
                        currNode.down = new Node(downState, downEmptyPos, currNode);
                        currNode.down.cost = currNode.cost + 1;
                        open.Enqueue(currNode.down, CalculateHeuristic(downState, goalState, goalPos) + currNode.down.cost);
                    }
                }
                else
                {
                    currNode.down = new Node(downState, downEmptyPos, currNode);
                    currNode.down.cost = currNode.cost + 1;
                    open.Enqueue(currNode.down, CalculateHeuristic(downState, goalState, goalPos) + currNode.down.cost);
                }
            }

            currNode = open.Dequeue();
        }

        return true;
    }
    // Calculates the heuristic value for the entered state
    // The heuristic used is: Manhatten Distance + Direct Reversals
    public static int CalculateHeuristic(int[,] initialState, int[,] goalState, Tuple<int, int>[] goalPos)
    {
        int distance = 0;
        int reversals = 0;

        for(int i = 0; i < initialState.GetLength(0); i++)
        {
            for(int j = 0; j < initialState.GetLength(1); j++)
            {
                int value = initialState[i, j];

                if(value != 0)
                {
                    int index = value - 1;
                    int diff = Math.Abs(i - goalPos[index].Item1);
                    diff += Math.Abs(j - goalPos[index].Item2);
                    distance += diff;

                    if(diff != 0)
                    {
                        // left reversal check
                        if (j > 0 && initialState[i, j] == goalState[i, j - 1] && goalState[i, j] == initialState[i, j - 1])
                        {
                            reversals++;
                        }
                        // right reversal check
                        else if (j < initialState.GetLength(1) - 1 && initialState[i, j] == goalState[i, j + 1] && goalState[i, j] == initialState[i, j + 1])
                        {
                            reversals++;
                        }
                        // up reversal check
                        else if (i > 0 && initialState[i, j] == goalState[i - 1, j] && goalState[i, j] == initialState[i - 1, j])
                        {
                            reversals++;
                        }
                        // down reversal check
                        else if (i < initialState.GetLength(0) - 1 && initialState[i, j] == goalState[i + 1, j] && goalState[i, j] == initialState[i + 1, j])
                        {
                            reversals++;
                        }
                    }
                }
            }
        }

        return (distance + reversals);
    }
    // Compares two 2d arrays for equality
    public static bool CompareArrays(int[,] arr1, int[,] arr2)
    {
        for(int i = 0; i < arr1.GetLength(0); i++)
        {
            for(int j = 0; j < arr1.GetLength(1); j++)
            {
                if (arr1[i, j] != arr2[i, j])
                {
                    return false;
                }
            }
        }

        return true;
    }
    // Swaps the two index values entered and returns the array
    public static int[,] SwapValues(int[,] currState, Tuple<int, int> val1, Tuple<int, int> val2)
    {
        int[,] newState = new int[currState.GetLength(0), currState.GetLength(1)];
        Array.Copy(currState, newState, currState.Length);
        int temp = newState[val1.Item1, val1.Item2];
        newState[val1.Item1, val1.Item2] = newState[val2.Item1, val2.Item2];
        newState[val2.Item1, val2.Item2] = temp;
        return newState;
    }
    // Randomly generates an initial state for the puzzle and returns it
    // Height and width are the dimensions of the goal puzzle
    // numsToAdd is an 1d array of numbers that are in the goal puzzle
    public static int[,] GeneratePuzzle(int height, int width, int[] numsToAdd, ref Tuple<int,int> emptyPos)
    {
        int[,] initialState = new int[height, width];
        Random rnd = new Random();
        List<int> nums = new List<int>(numsToAdd);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int index = rnd.Next(0, nums.Count);
                initialState[i, j] = nums[index];
                if (nums[index] == 0)
                {
                    emptyPos = new Tuple<int, int>(i, j);
                }
                nums.RemoveAt(index);
            }
        }

        return initialState;
    }
    // Converts goal puzzle from 2d array to 1d array
    // Sets up an array for the goal positions of each tile
    public static int[] ConvertToLinear(int[,] goalPuzzle, ref Tuple<int, int>[] goalPos)
    {
        int[] linForm = new int[goalPuzzle.Length];

        for (int i = 0; i < goalPuzzle.GetLength(0); i++)
        {
            for (int j = 0; j < goalPuzzle.GetLength(1); j++)
            {
                int index;

                if (goalPuzzle[i, j] == 0)
                {
                    index = goalPuzzle.Length - 1;
                }
                else
                {
                    index = goalPuzzle[i, j] - 1;
                }

                goalPos[index] = Tuple.Create(i, j);
                linForm[index] = goalPuzzle[i, j];
            }
        }

        return linForm;
    }
    // Converts initial puzzle from 2d array to 1d array
    // Uses the goalPos array to make sure each 2d array index is assigned to the correct 1d array index
    public static int[] ConvertToLinear(int[,] initialPuzzle, Tuple<int, int>[] goalPos)
    {
        int[] linForm = new int[initialPuzzle.Length];

        for(int i = 0; i < linForm.Length; i++)
        {
            linForm[i] = initialPuzzle[goalPos[i].Item1, goalPos[i].Item2];
        }

        return linForm;
    }
    // Checks if the puzzle is solvable
    // Counts inversions in the puzzle and returns false if inversion count is odd
    // If an odd board size has an odd number of inversions it isn't solvable
    // 8-puzzle has 9! total configurations and 9!/2 or 181440 of them are solvable
    public static bool PuzzleSolvable(int[] puzzleLinForm)
    {
        int count = 0;

        for (int i = 0; i < puzzleLinForm.Length; i++)
        {
            for (int j = i + 1; j < puzzleLinForm.Length; j++)
            {
                if (puzzleLinForm[i] > 0 && puzzleLinForm[j] > 0 && puzzleLinForm[i] > puzzleLinForm[j])
                {
                    count++;
                }
            }
        }

        //Console.WriteLine(count);
        //Console.WriteLine(count % 2);
        return (count % 2 == 0);
    }
    // Formats and displays the puzzle entered
    public static void DisplayPuzzle(int[,] initialPuzzle, int[,] goalPuzzle, bool dfsSolved, bool ucsSolved, bool bfsSolved, bool aStarSolved)
    {
        string display = "Initial Puzzle\tGoal Puzzle\n";
        display += "------------\t------------\n";

        for (int i = 0; i < initialPuzzle.GetLength(0); i++)
        {
            string initialDisplay = "";
            string goalDisplay = "";

            for (int j = 0; j < initialPuzzle.GetLength(1); j++)
            {
                initialDisplay += " " + initialPuzzle[i, j] + " ";
                goalDisplay += " " + goalPuzzle[i, j] + " ";

                if (j != initialPuzzle.GetLength(1) - 1)
                {
                    initialDisplay += "|";
                    goalDisplay += "|";
                }
            }

            display += initialDisplay + "\t" + goalDisplay + "\n";
            display += "------------\t------------\n";
        }

        display += "DFS Solved: " + dfsSolved + "\nUCS Solved: " + ucsSolved;
        display += "\nBFS Solved: " + bfsSolved + "\nA* Solved: " + aStarSolved;
        Console.WriteLine(display + "\n");
    }
    // Displays algorithm statistics at the end of the program
    public static void DisplayStats(Tuple<long, int>[] dfsStats, Tuple<long, int>[] ucsStats, Tuple<long, int>[] bfsStats, Tuple<long, int>[] aStarStats)
    {
        double dfsNodeAvg = 0, dfsTimeAvg = 0;
        double ucsNodeAvg = 0, ucsTimeAvg = 0;
        double bfsNodeAvg = 0, bfsTimeAvg = 0;
        double aStarNodeAvg = 0, aStarTimeAvg = 0;

        for (int i = 0; i < dfsStats.Length; i++)
        {
            dfsTimeAvg += dfsStats[i].Item1;
            dfsNodeAvg += dfsStats[i].Item2;

            ucsTimeAvg += ucsStats[i].Item1;
            ucsNodeAvg += ucsStats[i].Item2;

            bfsTimeAvg += bfsStats[i].Item1;
            bfsNodeAvg += bfsStats[i].Item2;

            aStarTimeAvg += aStarStats[i].Item1;
            aStarNodeAvg += aStarStats[i].Item2;
        }

        dfsTimeAvg = Math.Round(dfsTimeAvg / dfsStats.Length, 4);
        dfsNodeAvg = Math.Round(dfsNodeAvg / dfsStats.Length, 4);

        ucsTimeAvg = Math.Round(ucsTimeAvg / ucsStats.Length, 4);
        ucsNodeAvg = Math.Round(ucsNodeAvg / ucsStats.Length, 4);

        bfsTimeAvg = Math.Round(bfsTimeAvg / bfsStats.Length, 4);
        bfsNodeAvg = Math.Round(bfsNodeAvg / bfsStats.Length, 4);

        aStarTimeAvg = Math.Round(aStarTimeAvg / aStarStats.Length, 4);
        aStarNodeAvg = Math.Round(aStarNodeAvg / aStarStats.Length, 4);

        Console.WriteLine("DFS Runtime Average: " + dfsTimeAvg);
        Console.WriteLine("DFS Node Visit Average: " + dfsNodeAvg);
        Console.WriteLine();
        Console.WriteLine("UCS Runtime Average: " + ucsTimeAvg);
        Console.WriteLine("UCS Node Visit Average: " + ucsNodeAvg);
        Console.WriteLine();
        Console.WriteLine("BFS Runtime Average: " + bfsTimeAvg);
        Console.WriteLine("BFS Node Visit Average: " + bfsNodeAvg);
        Console.WriteLine();
        Console.WriteLine("A* Runtime Average: " + aStarTimeAvg);
        Console.WriteLine("A* Node Visit Average: " + aStarNodeAvg);
    }
}