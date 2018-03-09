using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Minesweeper
{
    public partial class MainWindow : Window
    {
        private int boardHeight;
        private int boardLength;
        private int mineCount;
        private bool isGameGenerated;
        private int nonMineFields;
        private MineField[] gameBoard;
        private int minesLeft;
        private int timer;
        private DispatcherTimer dispatcherTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            isGameGenerated = false;
            boardHeight = 16;
            boardLength = 30;
            mineCount = 99;
            minesLeft = mineCount;
            nonMineFields = boardHeight * boardLength - mineCount;
            gameBoard = new MineField[boardHeight * boardLength];
            timer = 0;

            foreach (Button btn in MineGrid.Children)
            {
                btn.MouseEnter -= Tile_MouseEnter;
                btn.MouseLeave -= Tile_MouseLeave;
                btn.Click -= Tile_Click;
                btn.MouseRightButtonDown -= Tile_MouseRightButtonDown;
                btn.MouseUp -= Tile_MouseUp;
            }

            MineLabel.Content = minesLeft;
            MineGrid.Children.Clear();
            AddButtons();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            TimeLabel.Content = timer;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            timer++;
            TimeLabel.Content = timer;
        }

        private void AddButtons()
        {
            var imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_unexplored.png");

            for (int i = 0; i < boardHeight * boardLength; i++)
            {
                var imageUnexplored = new Image
                {
                    Source = new BitmapImage(imageSource)
                };
                var tile = new Button
                {
                    Content = imageUnexplored,
                    BorderThickness = new Thickness(0)
                };
                tile.MouseEnter += Tile_MouseEnter;
                tile.MouseLeave += Tile_MouseLeave;
                tile.Click += Tile_Click;
                tile.MouseRightButtonDown += Tile_MouseRightButtonDown;
                tile.MouseUp += Tile_MouseUp;

                gameBoard[MineGrid.Children.Count] = new MineField { button = tile };
                MineGrid.Children.Add(tile);
            }
        }

        private void Tile_MouseEnter(object sender, MouseEventArgs e)
        {
            var imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_mine_hit.png");
            var imageHover = new Image
            {
                Source = new BitmapImage(imageSource)
            };

            var btn = (sender as Button);
            var btnIndex = MineGrid.Children.IndexOf(btn);

            if (gameBoard[btnIndex].Status == MineField.ClickStatus.Unclicked)
            {
                btn.Content = imageHover;
            }
        }

        private void Tile_MouseLeave(object sender, MouseEventArgs e)
        {
            var imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_unexplored.png");
            var imageHover = new Image
            {
                Source = new BitmapImage(imageSource)
            };

            var btn = (sender as Button);
            var btnIndex = MineGrid.Children.IndexOf(btn);

            if (gameBoard[btnIndex].Status == MineField.ClickStatus.Unclicked)
            {
                btn.Content = imageHover;
            }
        }

        private void Tile_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var index = MineGrid.Children.IndexOf(btn);

            if (!isGameGenerated)
            {
                GenerateGame(index);
                isGameGenerated = true;
            }
            if (gameBoard[index].Status == MineField.ClickStatus.Unclicked)
            {
                RevealTiles(index);
            }
                
        }

        private void Tile_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            var btn = (sender as Button);
            var btnIndex = MineGrid.Children.IndexOf(btn);

            if (gameBoard[btnIndex].Status != MineField.ClickStatus.Clicked)
            {
                Uri imageSource;
                if (gameBoard[btnIndex].Status == MineField.ClickStatus.Unclicked)
                {
                    imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_flag.png");
                    gameBoard[btnIndex].Status = MineField.ClickStatus.Flag;
                    minesLeft--;
                }
                else
                {
                    imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_unexplored.png");
                    gameBoard[btnIndex].Status = MineField.ClickStatus.Unclicked;
                    minesLeft++;
                }

                var img = new Image
                {
                    Source = new BitmapImage(imageSource)
                };

                MineLabel.Content = minesLeft;
                btn.Content = img;
            }
        }

        private void Tile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var btn = (sender as Button);
            var btnIndex = MineGrid.Children.IndexOf(btn);
            int flagCount = 0;

            if (e.ChangedButton == MouseButton.Middle && gameBoard[btnIndex].Status == MineField.ClickStatus.Clicked)
            {
                var neighbours = ValidNeighbours(btnIndex);
                foreach (var neighbour in neighbours)
                {
                    if (gameBoard[neighbour].Status == MineField.ClickStatus.Flag)
                    {
                        flagCount++;
                    }
                }

                if (flagCount == gameBoard[btnIndex].neighbourMines)
                {
                    foreach (var neighbour in neighbours)
                    {
                        if (gameBoard[neighbour].Status == MineField.ClickStatus.Unclicked)
                        {
                            RevealTiles(neighbour);
                        }
                    }
                }
            }
            if (nonMineFields <= 0)
            {
                MessageBox.Show("You Win!");
                dispatcherTimer.Stop();
            }
        }

        private void GenerateGame(int clickedButton)
        {
            int numMines = mineCount;
            Random rnd = new Random();

            //want first click to be a 0 so 8 surrounding tiles can't have mines
            int row = clickedButton / boardLength;
            int col = clickedButton % boardLength;
            int tl = (row - 1) * boardLength + (col - 1);
            int t = (row - 1) * boardLength + col;
            int tr = (row - 1) * boardLength + (col + 1);
            int l = row * boardLength + (col - 1);
            int r = row * boardLength + (col + 1);
            int bl = (row + 1) * boardLength + (col - 1);
            int b = (row + 1) * boardLength + col;
            int br = (row + 1) * boardLength + (col + 1);

            while (numMines > 0 && numMines < (boardLength * boardHeight - 8))
            {
                int rndField = rnd.Next(boardLength * boardHeight);
                if (!gameBoard[rndField].isMine && rndField != clickedButton && rndField != tl && rndField != t &&
                    rndField != tr && rndField != l && rndField != r && rndField != bl && rndField != b &&
                    rndField != br)
                {
                    numMines--;
                    gameBoard[rndField].isMine = true;
                }
            }

            CalculateNeighbouringMines();
            dispatcherTimer.Start();
        }

        private void RevealMines()
        {
            int i = 0;

            foreach (var field in gameBoard)
            {
                if (field.isMine)
                {
                    var imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_mine_hit.png");
                    var imageHover = new Image
                    {
                        Source = new BitmapImage(imageSource)
                    };

                    field.button.Content = imageHover;
                }
                gameBoard[i].Status = MineField.ClickStatus.Clicked;
                i++;
            }
        }

        private void CalculateNeighbouringMines()
        {
            for (int i = 0; i < gameBoard.Length; i++)
            {
                mineCount = 0;
                if (!gameBoard[i].isMine)
                {
                    var neighbours = ValidNeighbours(i);

                    foreach (var neighbour in neighbours)
                    {
                        if (gameBoard[neighbour].isMine)
                            mineCount++;
                    }

                    gameBoard[i].neighbourMines = mineCount;
                }
            }
        }

        private void RevealTiles(int index)
        {
            var mineCount = gameBoard[index].neighbourMines;
            Uri imageSource;

            if (gameBoard[index].isMine)
            {
                imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_mine_hit.png");
                RevealMines();
                dispatcherTimer.Stop();
            }
            else
            {
                gameBoard[index].Status = MineField.ClickStatus.Clicked;
                switch (mineCount)
                {
                    case 0:
                        var neighbours = ValidNeighbours(index);

                        foreach (var neighbour in neighbours)
                        {
                            if (gameBoard[neighbour].Status == MineField.ClickStatus.Unclicked)
                            {
                                RevealTiles(neighbour);
                            }
                        }

                        imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_0.png");
                        break;
                    case 1:
                        imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_1.png");
                        break;
                    case 2:
                        imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_2.png");
                        break;
                    case 3:
                        imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_3.png");
                        break;
                    case 4:
                        imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_4.png");
                        break;
                    case 5:
                        imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_5.png");
                        break;
                    case 6:
                        imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_6.png");
                        break;
                    case 7:
                        imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_7.png");
                        break;
                    default:
                        imageSource = new Uri("pack://application:,,,/images/Minesweeper_LAZARUS_21x21_8.png");
                        break;
                }
                nonMineFields--;
            }

            var imageHover = new Image
            {
                Source = new BitmapImage(imageSource)
            };

            gameBoard[index].button.Content = imageHover;
        }

        /*
         * Takes index of button
         * Returns index list of neighbours inside gameboard
         */
        private List<int> ValidNeighbours(int index)
        {
            List<int> validNeighbours = new List<int>();
            int row = index / boardLength;
            int col = index % boardLength;
            int tl = (row - 1) * boardLength + (col - 1);
            int t = (row - 1) * boardLength + col;
            int tr = (row - 1) * boardLength + (col + 1);
            int l = row * boardLength + (col - 1);
            int r = row * boardLength + (col + 1);
            int bl = (row + 1) * boardLength + (col - 1);
            int b = (row + 1) * boardLength + col;
            int br = (row + 1) * boardLength + (col + 1);

            if (row > 0)
            {
                if (col > 0)
                    validNeighbours.Add(tl);
                validNeighbours.Add(t);
                if (col < boardLength - 1)
                    validNeighbours.Add(tr);
            }
            if (col > 0)
                validNeighbours.Add(l);
            if (col < boardLength - 1)
                validNeighbours.Add(r);
            if (row < boardHeight - 1)
            {
                if (col > 0)
                    validNeighbours.Add(bl);
                validNeighbours.Add(b);
                if (col < boardLength - 1)
                    validNeighbours.Add(br);
            }

            return validNeighbours;
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            InitializeVariables();
        }

        private void ExitButon_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OptionsButon_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("What you don't like how it's set up?");
        }

        private void StatisticsButon_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You're doing great!");
        }
    }
}
