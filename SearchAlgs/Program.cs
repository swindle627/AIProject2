using System;

class SearchAlgs
{
    static public void Main(String[] args)
    {
        /* Goal Puzzle
         * 1 2 3
         * 8 0 4
         * 7 6 5
         */
        int[,] goalState = { { 1, 2, 3 }, { 8, 0, 4 }, { 7, 6, 5 } };
        int timesToRun = 20; // number of times the algorithms will run in while loop
        int[,] initialState = new int[3, 3]; // Initial Puzzle

        while(timesToRun != 0)
        {
            initialState = GeneratePuzzle(initialState);

            Console.WriteLine("Goal State");
            DisplayPuzzle(goalState);

            if(IsSolvable(initialState))
            {
                Console.WriteLine("Initial State");
            }
            else
            {
                Console.WriteLine("Puzzle not solvable");
            }
            
            DisplayPuzzle(initialState);

            timesToRun--;
        }

    }
    // Randomly generates an initial state for the puzzle and returns it
    public static int[,] GeneratePuzzle(int[,] initialSate)
    {
        List<int> numsToAdd = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        Random rnd = new Random();

        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                int index = rnd.Next(0, numsToAdd.Count);
                initialSate[i, j] = numsToAdd[index];
                numsToAdd.RemoveAt(index);
            }
        }

        return initialSate;
    }
    // Formats and displays the puzzle entered
    public static void DisplayPuzzle(int[,] puzzle)
    {
        string display = "";

        for (int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                display += " " + puzzle[i, j] + " ";

                if(j != 2)
                {
                    display += "|";
                }
            }

            if (i != 2)
            {
                display += "\n------------\n";
            }
        }

        display += "\n\n";
        Console.WriteLine(display);
    }
    // Checks if the puzzle is solvable
    // Odd inversion count == not solvable
    public static bool IsSolvable(int[,] initialState)
    {
        int[] linForm =
        {
            initialState[0,0], initialState[0,1], initialState[0,2],
            initialState[1,2], initialState[2,2], initialState[2,1],
            initialState[2,0], initialState[1,0], initialState[1,1]
        };

        return (CountInversions(linForm) % 2 == 0);
    }
    // Counts inversions of the array entered
    public static int CountInversions(int[] puzzleLinForm)
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

        return count;
    }
}