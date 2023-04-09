using System;
using System.Numerics;
using System.Runtime.InteropServices;

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
        // the amount of solvable puzzles the algorithms will solve
        int puzzleCount = 1;

        RunAlgs(puzzleCount, goalState);

    }
    // Runs the various search algorithms
    public static void RunAlgs(int puzzleCount, int[,] goalState)
    {
        Tuple<int, int>[] goalPos = new Tuple<int, int>[goalState.Length];
        int[] goalStateLin = ConvertToLinear(goalState, ref goalPos);

        while(puzzleCount != 0)
        {
            Tuple<int, int> emptyPos = new Tuple<int, int>(1, 1);
            int heuristicVal;
            int[,] initialState = { { 1, 3, 4 }, { 8, 0, 2 }, { 7, 6, 5 } };
            int[,] dfsArray = new int[goalState.GetLength(0), goalState.GetLength(1)];
            int[] initialStateLin;

            initialStateLin = ConvertToLinear(initialState, goalPos);
            PuzzleSolvable(initialStateLin);
            heuristicVal = CalculateHeuristic(initialState, goalState, goalPos);

            //do
            //{
            //    initialState = GeneratePuzzle(goalState.GetLength(0), goalState.GetLength(1), goalStateLin, ref emptyPos);
            //    initialStateLin = ConvertToLinear(initialState, goalPos);
            //}
            //while(!PuzzleSolvable(initialStateLin));

            Array.Copy(initialState, dfsArray, initialState.Length);
            bool solved = DepthFirstSearch(dfsArray, null, goalState, emptyPos, 0, heuristicVal);

            DisplayPuzzle(initialState, goalState);
            Console.WriteLine(heuristicVal);
            Console.WriteLine("Puzzle solved: " + solved);
            puzzleCount--;
        }
    }
    // Performs recursive DFS search
    public static bool DepthFirstSearch(int[,] currState, int[,] prevState, int[,] goalState, Tuple<int, int> emptyPos, int currDepth, int depthLimit)
    {
        bool goalReached = false;

        //DisplayPuzzle(currState, goalState);
        // i actually need to check to see if the state im about to change to equals the parent's parent
        // aka prevPrevState

        if (currState == goalState)
        {
            goalReached = true;
            return goalReached;
        }

        if(currDepth != depthLimit)
        {
            bool upValid, downValid, leftValid, rightValid;

            if(currDepth == 0)
            {
                upValid = (emptyPos.Item1 - 1 >= 0);
                downValid = (emptyPos.Item1 + 1 < currState.GetLength(0));
                leftValid = (emptyPos.Item2 - 1 >= 0);
                rightValid = (emptyPos.Item2 + 1 < currState.GetLength(1));
            }
            else
            {
                upValid = (emptyPos.Item1 - 1 >= 0) && (currState != prevState);
                downValid = (emptyPos.Item1 + 1 < currState.GetLength(0)) && (currState != prevState);
                leftValid = (emptyPos.Item2 - 1 >= 0) && (currState != prevState);
                rightValid = (emptyPos.Item2 + 1 < currState.GetLength(1)) && (currState != prevState);
                DisplayPuzzle(currState, prevState);
            }

            currDepth += 1;

            if (leftValid && !goalReached)
            {
                Console.WriteLine("Left " + currDepth);
                Tuple<int, int> leftEmptyPos = new Tuple<int, int>(emptyPos.Item1, emptyPos.Item2 - 1);
                int[,] leftState = SwapValues(currState, leftEmptyPos, emptyPos);
                goalReached = DepthFirstSearch(leftState, currState, goalState, leftEmptyPos, currDepth, depthLimit);
            }

            if (rightValid && !goalReached)
            {
                Console.WriteLine("Right " + currDepth);
                Tuple<int, int> rightEmptyPos = new Tuple<int, int>(emptyPos.Item1, emptyPos.Item2 + 1);
                int[,] rightState = SwapValues(currState, rightEmptyPos, emptyPos);
                goalReached = DepthFirstSearch(rightState, currState, goalState, rightEmptyPos, currDepth, depthLimit);
            }

            if (upValid && !goalReached)
            {
                Console.WriteLine("Up " + currDepth);
                Tuple<int, int> upEmptyPos = new Tuple<int, int>(emptyPos.Item1 - 1, emptyPos.Item2);
                int[,] upState = SwapValues(currState, upEmptyPos, emptyPos);
                goalReached = DepthFirstSearch(upState, currState, goalState, upEmptyPos, currDepth, depthLimit);
            }

            if (downValid && !goalReached)
            {
                Console.WriteLine("Down " + currDepth);
                Tuple<int, int> downEmptyPos = new Tuple<int, int>(emptyPos.Item1 + 1, emptyPos.Item2);
                int[,] downState = SwapValues(currState, downEmptyPos, emptyPos);
                goalReached = DepthFirstSearch(downState, currState, goalState, downEmptyPos, currDepth, depthLimit);
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
                            //Console.WriteLine("left running");
                        }
                        // right reversal check
                        else if (j < initialState.GetLength(1) - 1 && initialState[i, j] == goalState[i, j + 1] && goalState[i, j] == initialState[i, j + 1])
                        {
                            reversals++;
                            //Console.WriteLine("right running");
                        }
                        // up reversal check
                        else if (i > 0 && initialState[i, j] == goalState[i - 1, j] && goalState[i, j] == initialState[i - 1, j])
                        {
                            reversals++;
                            //Console.WriteLine("up running");
                        }
                        // down reversal check
                        else if (i < initialState.GetLength(0) - 1 && initialState[i, j] == goalState[i + 1, j] && goalState[i, j] == initialState[i + 1, j])
                        {
                            reversals++;
                            //Console.WriteLine("down running");
                        }
                    }
                }
            }
        }

        //Console.WriteLine(distance);
        //Console.WriteLine(reversals);
        return (distance + (2 * reversals));
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
    public static void DisplayPuzzle(int[,] initialPuzzle, int[,] goalPuzzle)
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

        Console.WriteLine(display + "\n");
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
    // Odd inversion count == not solvable
    public static bool PuzzleSolvable(int[] puzzleLinForm)
    {
        int count = 0;

        for(int i = 0; i < puzzleLinForm.Length; i++)
        {
            for(int j = i + 1; j < puzzleLinForm.Length; j++)
            {
                if (puzzleLinForm[i] > 0 && puzzleLinForm[j] > 0 && puzzleLinForm[i] > puzzleLinForm[j])
                {
                    count++;
                }
            }
        }

        //Console.WriteLine(count);
        return (count % 2 == 0);
    }
}