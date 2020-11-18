using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LightsOut
{
    
    public partial class LightsOut : Form
    {
        #region Constructor
        public LightsOut()
        {
            InitializeComponent();

            Rand = new Random();

            mainBoard = new Board(3, 3);
            mainBoard.Randomize();
            PushBoard(mainBoard);

            mode = "Play";
        }
        #endregion

        #region Global Variables
        static Random Rand;
        static Board mainBoard;
        static string mode;
        #endregion


        #region Functions
        private void PushBoard(Board B)
        {
            for (int x = 0; x < B.sizeX; x++)
            {
                for (int y = 0; y < B.sizeY; y++)
                {
                    if (B.state[x,y])
                    {
                        table_Grid.GetControlFromPosition(x, y).BackColor = System.Drawing.SystemColors.HotTrack;
                    }
                    else
                    {
                        table_Grid.GetControlFromPosition(x, y).BackColor = System.Drawing.SystemColors.WindowFrame;
                    }
                    if (B.presses[x,y])
                    {
                        table_Grid.GetControlFromPosition(x, y).Text = B.presses[x, y].ToString();
                    } else
                    {
                        table_Grid.GetControlFromPosition(x, y).Text = "";
                    }
                }
            }
        }
        private void GridButtonPressed(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            var bPos = table_Grid.GetCellPosition(b);
            int x = bPos.Column, y=bPos.Row;

            //System.Diagnostics.Debug.WriteLine(String.Format("Button pressed: ({0},{1})",x,y));

            // Logic for text. If the button was to be pressed to reach the solution, delete the text.
            if (b.Text == "1")
            {
                b.Text = "";
            }

            switch (mode)
            {
                case "Play":
                    // Flip self and neighbours
                    mainBoard.SimulatePress(x, y);
                    // Remove the press indicator
                    if (mainBoard.presses[x, y])
                    {
                        mainBoard.presses[x, y] = false;
                    }
                    break;
                case "Set":
                    // Flip only self
                    mainBoard.FlipAtIndex(x,y);
                    break;
            }

            PushBoard(mainBoard);
        }
        private void ButtonPressed_Solve(object sender, EventArgs e)
        {
            // This method is called 'Chase the lights'

            // Virtual copy of the board to keep track of origional board state
            Board virtualBoard;

            mainBoard.ResetPresses();

            // This loops through the permutations of the possible move sets.
            // [0,0,0 , 0,0,0 , 0,0,0]...[1,1,1 , 1,1,1 , 1,1,1]

            int lightAmount = mainBoard.sizeX * mainBoard.sizeY;
            int permAmount = Convert.ToInt32(Math.Pow(2, lightAmount));

            for (int permI = 0; permI < permAmount; permI++)
            {
                // Reset board
                virtualBoard = new Board(mainBoard.state);

                string line = Convert.ToString(permI, 2).PadLeft(lightAmount, '0');

                //System.Diagnostics.Debug.WriteLine("Trying: " + line);

                // For each char in the permutation
                char[] chars = line.ToArray();
                for (int i = 0; i<chars.Length; i++)
                {
                    int x = i % mainBoard.sizeX;
                    int y = i / 3;
                    //System.Diagnostics.Debug.WriteLine("("+x+","+y+")");
                    if (chars[i] == '1')
                    {
                        virtualBoard.SimulatePress(x, y);
                        //System.Diagnostics.Debug.WriteLine(virtualBoard.GetString());
                        //System.Threading.Thread.Sleep(1000);
                    }
                }

                // Determine if this is a solution
                bool allOff = true;
                foreach (bool b in virtualBoard.state)
                {
                    if (b)
                    {
                        allOff = false;
                        break;
                    }
                }

                // If permute was succesfull
                if (allOff)
                {
                    // Push presses to mainBoard
                    for (int i = 0; i < chars.Length; i++)
                    {
                        int x = i % mainBoard.sizeX;
                        int y = i / 3;

                        if (chars[i] == '1')
                        {
                            mainBoard.presses[x, y] = true;
                        }
                    }

                    // Push mainBoard
                    PushBoard(mainBoard);

                    // Stop
                    return;
                }
            }
            // No solution found
            NoSolutionPopupForm popup = new NoSolutionPopupForm();
            popup.ShowDialog();
            popup.Dispose();
        }
        private void ButtonPressed_SetPlay(object sender, EventArgs e)
        {
            if (mode == "Play")
            {
                mode = "Set";
                button_set.Text = "Play";
            } else
            {
                mode = "Play";
                button_set.Text = "Set";
            }
            
        }
        public void SetLight(int x, int y, bool s)
        {
            
        }

        private void ButtonPressed_Randomize(object sender, EventArgs e)
        {
            mainBoard.Randomize();
            mainBoard.ResetPresses();
            PushBoard(mainBoard);
        }
        #endregion

        #region Board Class

    }
    #endregion

    public partial class Board
    {
        public bool[,] state;
        public int sizeX, sizeY;
        public bool[,] presses;
        Random Rand;
        public void Initialize()
        {
            sizeX = state.GetLength(0);
            sizeY = state.GetLength(1);
            presses = new bool[sizeX, sizeY];
            ResetPresses();
            Rand = new Random();
        }
        public Board(int x, int y)
        // Constructor that takes the size
        {
            state = new bool[x, y];
            Initialize();
        }
        public Board(bool[,] set)
        // Constructor that takes a board state
        {
            state = set.Clone() as bool[,];
            Initialize();
        }

        public void Randomize()
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    state[x, y] = (Rand.Next(2) == 1);
                }
            }
        }
        public void SimulatePress(int x, int y)
        {
            // For self and neighbours
            foreach ((int xOffset, int yOffset) in new List<(int, int)>()
                    { (0, 0), (-1, 0), (0, 1), (1, 0), (0, -1) })
            {
                int X = x + xOffset;
                int Y = y + yOffset;
                if (IsIn(X, Y))
                {
                    // Flip the light
                    state[X, Y] = !state[X, Y];
                }
            }
        }
        public void FlipAtIndex(int x, int y)
        {
            state[x, y] = !state[x, y];
        }
        public bool IsIn(int x, int y)
        {
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void ResetPresses()
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    presses[x,y] = false;
                }
            }
        }
        public string GetString()
        {
            string s = "";
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if(state[x, y])
                    {
                        s += "1";
                    }
                    else
                    {
                        s += "0";
                    }
                }
                s += "\n";
            }
            return s;
        }
    }
}
