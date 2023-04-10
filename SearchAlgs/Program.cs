using SearchAlgs;
using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

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
        
        int[] goalStateLin = ConvertToLinear(goalState, ref goalPos);

        while(puzzleCount != 0)
        {
            Tuple<int, int> emptyPos = new Tuple<int, int>(0, 0);
            Stopwatch watch = new Stopwatch();
            int heuristicVal;
            int[,] initialState;
            int[] initialStateLin;
            int nodesVisited = 0;

            do
            {
                initialState = GeneratePuzzle(goalState.GetLength(0), goalState.GetLength(1), goalStateLin, ref emptyPos);
                initialStateLin = ConvertToLinear(initialState, goalPos);
            }
            while (!PuzzleSolvable(initialStateLin));

            heuristicVal = CalculateHeuristic(initialState, goalState, goalPos);
            //Console.WriteLine(heuristicVal);

            Node root = new Node(initialState, emptyPos);
            watch.Start();
            bool dfsSolved = DepthFirstSearch(root, goalState, 0, depthLimit, ref nodesVisited);
            watch.Stop();
            dfsStats[puzzleCount - 1] = Tuple.Create(watch.ElapsedMilliseconds, nodesVisited);
            Console.WriteLine(nodesVisited);
            watch.Reset();
            nodesVisited = 0;

            DisplayPuzzle(initialState, goalState, dfsSolved);
            puzzleCount--;
        }

        DisplayStats(dfsStats);
    }
    // Performs recursive DFS search
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
                currNode.left = new Node(leftState, leftEmptyPos, currNode);

                if (currNode.parent != null)
                {
                    if(!CompareArrays(currNode.left.state, currNode.parent.state))
                    {
                        goalReached = DepthFirstSearch(currNode.left, goalState, currDepth, depthLimit, ref nodesVisited);
                    }
                }
                else
                {
                    goalReached = DepthFirstSearch(currNode.left, goalState, currDepth, depthLimit, ref nodesVisited);
                }
            }

            if (rightValid && !goalReached)
            {
                Tuple<int, int> rightEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1, currNode.emptyPos.Item2 + 1);
                int[,] rightState = SwapValues(currNode.state, rightEmptyPos, currNode.emptyPos);
                currNode.right = new Node(rightState, rightEmptyPos, currNode);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(currNode.right.state, currNode.parent.state))
                    {
                        goalReached = DepthFirstSearch(currNode.right, goalState, currDepth, depthLimit, ref nodesVisited);
                    }
                }
                else
                {
                    goalReached = DepthFirstSearch(currNode.right, goalState, currDepth, depthLimit, ref nodesVisited);
                }
            }

            if (upValid && !goalReached)
            {
                Tuple<int, int> upEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1 - 1, currNode.emptyPos.Item2);
                int[,] upState = SwapValues(currNode.state, upEmptyPos, currNode.emptyPos);
                currNode.up = new Node(upState, upEmptyPos, currNode);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(currNode.up.state, currNode.parent.state))
                    {
                        goalReached = DepthFirstSearch(currNode.up, goalState, currDepth, depthLimit, ref nodesVisited);
                    }
                }
                else
                {
                    goalReached = DepthFirstSearch(currNode.up, goalState, currDepth, depthLimit, ref nodesVisited);
                }
            }

            if (downValid && !goalReached)
            {
                Tuple<int, int> downEmptyPos = new Tuple<int, int>(currNode.emptyPos.Item1 + 1, currNode.emptyPos.Item2);
                int[,] downState = SwapValues(currNode.state, downEmptyPos, currNode.emptyPos);
                currNode.down = new Node(downState, downEmptyPos, currNode);

                if (currNode.parent != null)
                {
                    if (!CompareArrays(currNode.down.state, currNode.parent.state))
                    {
                        goalReached = DepthFirstSearch(currNode.down, goalState, currDepth, depthLimit, ref nodesVisited);
                    }
                }
                else
                {
                    goalReached = DepthFirstSearch(currNode.down, goalState, currDepth, depthLimit, ref nodesVisited);
                }
            }
        }
        
        return goalReached;
    }
    // Calculates the heuristic value for the entered state
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
                            //Console.WriteLine("Direct Reversal: " + initialState[i, j] + ", " + initialState[i, j - 1]);
                        }
                        // right reversal check
                        else if (j < initialState.GetLength(1) - 1 && initialState[i, j] == goalState[i, j + 1] && goalState[i, j] == initialState[i, j + 1])
                        {
                            reversals++;
                            //Console.WriteLine("Direct Reversal: " + initialState[i, j] + ", " + initialState[i, j + 1]);
                        }
                        // up reversal check
                        else if (i > 0 && initialState[i, j] == goalState[i - 1, j] && goalState[i, j] == initialState[i - 1, j])
                        {
                            reversals++;
                            //Console.WriteLine("Direct Reversal: " + initialState[i, j] + ", " + initialState[i - 1, j]);
                        }
                        // down reversal check
                        else if (i < initialState.GetLength(0) - 1 && initialState[i, j] == goalState[i + 1, j] && goalState[i, j] == initialState[i + 1, j])
                        {
                            reversals++;
                            //Console.WriteLine("Direct Reversal: " + initialState[i, j] + ", " + initialState[i + 1, j]);
                        }
                    }
                }
            }
        }

        //Console.WriteLine(distance);
        //Console.WriteLine(reversals);
        //Console.WriteLine(distance + reversals);
        return (distance + (2 * reversals));
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
    // numsToAdd is an array of numbers that are in the goal puzzle
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
    // Formats and displays the puzzle entered
    public static void DisplayPuzzle(int[,] initialPuzzle, int[,] goalPuzzle, bool dfsSolved)
    {
        string display = "Initial Puzzle\tGoal Puzzle\n";
        display += "------------\t------------\n";
        
        for (int i = 0; i < initialPuzzle.GetLength(0); i++)
        {
            string initialDisplay = "";
            string goalDisplay = "";

            for(int j = 0; j < initialPuzzle.GetLength(1); j++)
            {
                initialDisplay += " " + initialPuzzle[i, j] + " ";
                goalDisplay += " " + goalPuzzle[i, j] + " ";

                if(j != initialPuzzle.GetLength(1) - 1)
                {
                    initialDisplay += "|";
                    goalDisplay += "|";
                }
            }

            display += initialDisplay + "\t" + goalDisplay + "\n";
            display += "------------\t------------\n";
        }

        display += "DFS Solved: " + dfsSolved;
        Console.WriteLine(display + "\n");
    }
    // Displays algorithm statistics at the end of the program
    public static void DisplayStats(Tuple<long, int>[] dfsStats)
    {
        double dfsNodeAvg = 0, dfsTimeAvg = 0;

        for(int i = 0; i < dfsStats.Length; i++)
        {
            dfsTimeAvg += dfsStats[i].Item1;
            dfsNodeAvg += dfsStats[i].Item2;
        }

        dfsTimeAvg = Math.Round(dfsTimeAvg / dfsStats.Length, 2);
        dfsNodeAvg = Math.Round(dfsNodeAvg / dfsStats.Length, 2);

        Console.WriteLine("DFS Runtime Average: " + dfsTimeAvg);
        Console.WriteLine("DFS Node Visit Average: " + dfsNodeAvg);
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
}