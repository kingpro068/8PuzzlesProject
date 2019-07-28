using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Timers;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Windows.Controls.Primitives;

namespace _8PuzzleProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        class DogTag
        {
            public int cord_X { get; set; }
            public int cord_Y { get; set; }
        }

        int canvasLeftPadding = 52;
        int canvasTopPadding = 42;
        bool isDragging = false;
        PuzzlePiece selectedPiece = null;
        int newi = -1;
        int newj = -1;
        double leftBorder { get; set; }
        double rightBorder { get; set; }
        double topBorder { get; set; }
        double bottomBorder { get; set; }

        class PuzzlePiece
        {
            public Image image { get; set; }
            public DogTag numTag { get; set; }
            public int originalPos_X { get; set; }
            public int originalPos_Y { get; set; }
            public int newPos_X { get; set; }
            public int newPos_Y { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BaseScore = 1000;
            //Add Input Time 
            string time = TimerTextBox.Text;
            OrigTime = (int)System.TimeSpan.Parse(time).TotalSeconds;
            BaseScoreTextBlock.Text = BaseScore.ToString();
            TimerTextBox.IsReadOnly = true;

            for (int i = 0; i < 3; i++)
            {
                var list = new List<PuzzlePiece>();
                for (int j = 0; j < 3; j++)
                {
                    var image = new PuzzlePiece();
                    list.Add(image);
                }
                scrambledList.Add(list);
            }
            scrambledList[2][2] = null;
            leftBorder = canvasLeftPadding;
            rightBorder = container.Width + canvasLeftPadding;
            topBorder = canvasTopPadding + TitleBar.Height;
            bottomBorder = container.Height + canvasTopPadding;

            //Key Arrows 
            this.KeyDown += new KeyEventHandler(KeyDownHandler);
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (isStart && isEnded == false)
            {
                int null_X = 0;
                int null_Y = 0;
                GetNullPosition(ref null_X, ref null_Y);

                switch (e.Key)
                {
                    case Key.Up:
                        if (null_X == 2)
                            return;

                        selectedPiece = scrambledList[null_X + 1][null_Y];
                        scrambledList[null_X][null_Y] = selectedPiece;
                        scrambledList[null_X + 1][null_Y] = null;
                        selectedPiece.newPos_X = null_Y * (croppedImageWidth + croppedImagePadding);
                        selectedPiece.newPos_Y = null_X * (croppedImageHeight + croppedImagePadding);
                        Canvas.SetLeft(selectedPiece.image, selectedPiece.newPos_X);
                        Canvas.SetTop(selectedPiece.image, selectedPiece.newPos_Y);
                        checkWinningState();
                        break;

                    case Key.Down:
                        if (null_X == 0)
                            return;

                        selectedPiece = scrambledList[null_X - 1][null_Y];
                        scrambledList[null_X][null_Y] = selectedPiece;
                        scrambledList[null_X - 1][null_Y] = null;
                        selectedPiece.newPos_X = null_Y * (croppedImageWidth + croppedImagePadding);
                        selectedPiece.newPos_Y = null_X * (croppedImageHeight + croppedImagePadding);
                        Canvas.SetLeft(selectedPiece.image, selectedPiece.newPos_X);
                        Canvas.SetTop(selectedPiece.image, selectedPiece.newPos_Y);
                        checkWinningState();
                        break;

                    case Key.Left:
                        if (null_Y == 2)
                            return;

                        selectedPiece = scrambledList[null_X][null_Y + 1];
                        scrambledList[null_X][null_Y] = selectedPiece;
                        scrambledList[null_X][null_Y + 1] = null;
                        selectedPiece.newPos_X = null_Y * (croppedImageWidth + croppedImagePadding);
                        selectedPiece.newPos_Y = null_X * (croppedImageHeight + croppedImagePadding);
                        Canvas.SetLeft(selectedPiece.image, selectedPiece.newPos_X);
                        Canvas.SetTop(selectedPiece.image, selectedPiece.newPos_Y);
                        checkWinningState();
                        break;

                    case Key.Right:
                        if (null_Y == 0)
                            return;

                        selectedPiece = scrambledList[null_X][null_Y - 1];
                        scrambledList[null_X][null_Y] = selectedPiece;
                        scrambledList[null_X][null_Y - 1] = null;
                        selectedPiece.newPos_X = null_Y * (croppedImageWidth + croppedImagePadding);
                        selectedPiece.newPos_Y = null_X * (croppedImageHeight + croppedImagePadding);
                        Canvas.SetLeft(selectedPiece.image, selectedPiece.newPos_X);
                        Canvas.SetTop(selectedPiece.image, selectedPiece.newPos_Y);
                        checkWinningState();
                        break;

                    default: return;
                }
            } 
        }

        List<List<PuzzlePiece>> puzzlePieceList = new List<List<PuzzlePiece>>();
        List<List<PuzzlePiece>> scrambledList = new List<List<PuzzlePiece>>();
        Image[,] images = new Image[3, 3];
        int croppedImageWidth;
        int croppedImageHeight;
        int croppedImagePadding = 5;

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var screen = new OpenFileDialog();
                screen.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                screen.DefaultExt = "*.png";
                if (screen.ShowDialog() == true)
                {
                    imageSource = screen.FileName;
                    puzzlePieceList = new List<List<PuzzlePiece>>();
                    clearCanvas();

                    var coreImage = new BitmapImage();
                    //Resize Image
                    coreImage.BeginInit();
                    coreImage.UriSource = new Uri(screen.FileName);
                    coreImage.DecodePixelHeight = 420;
                    coreImage.DecodePixelWidth = 420;
                    coreImage.EndInit();

                    HintImage.Source = coreImage;

                    var coreImageWidth = 420;
                    croppedImageWidth = (int)coreImageWidth / 3;
                    croppedImageHeight = (int)(coreImageWidth * coreImage.Height / coreImage.Width) / 3;

                    for (int i = 0; i < 3; i++)
                    {
                        var list = new List<PuzzlePiece>();
                        for (int j = 0; j < 3; j++)
                        {
                            if (i != 2 || j != 2)
                            {
                                var croppedImage = new CroppedBitmap(coreImage, new Int32Rect(
                                        (int)(j * coreImage.Width / 3), (int)(i * coreImage.Height / 3),
                                        (int)coreImage.Width / 3, (int)coreImage.Height / 3));
                                var imagePiece = new PuzzlePiece() { image = new Image() { Source = croppedImage, Width = croppedImageWidth, Height = croppedImageHeight } };
                                container.Children.Add(imagePiece.image);
                                imagePiece.originalPos_X = j * (croppedImageWidth + croppedImagePadding);
                                imagePiece.originalPos_Y = i * (croppedImageHeight + croppedImagePadding);
                                imagePiece.numTag = new DogTag();
                                imagePiece.numTag.cord_X = i;
                                imagePiece.numTag.cord_Y = j;
                                list.Add(imagePiece);
                            }
                        }
                        puzzlePieceList.Add(list);
                    }

                    do
                    {
                        var rng = new Random();
                        var pool = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
                        var pooli = new List<int> { 0, 0, 0, 1, 1, 1, 2, 2 };
                        var poolj = new List<int> { 0, 1, 2, 0, 1, 2, 0, 1 };

                        for (int i = 0; i < 3; i++)
                        {
                            //var list = new List<PuzzlePiece>();
                            for (int j = 0; j < 3; j++)
                            {
                                if (i != 2 || j != 2)
                                {
                                    // Set vi tri
                                    var k = rng.Next(pool.Count); // Chon ngau nhien mot chi muc trong pool

                                    var imagePiece = puzzlePieceList[i][j];
                                    // Tao giao dien
                                    //container.Children.Add(imagePiece.image);
                                    imagePiece.newPos_X = poolj[k] * (croppedImageWidth + croppedImagePadding);
                                    imagePiece.newPos_Y = pooli[k] * (croppedImageHeight + croppedImagePadding);
                                    scrambledList[pooli[k]].Insert(poolj[k], imagePiece);
                                    scrambledList[pooli[k]].RemoveAt(poolj[k] + 1);

                                    //list.Add(imagePiece);
                                    Canvas.SetLeft(imagePiece.image, imagePiece.newPos_X);
                                    Canvas.SetTop(imagePiece.image, imagePiece.newPos_Y);
                                    pool.RemoveAt(k);
                                    pooli.RemoveAt(k);
                                    poolj.RemoveAt(k);
                                }
                            }
                        }

                        //For solvable check
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (i != 2 || j != 2)
                                {
                                    var temp = scrambledList[i][j].numTag.cord_X * 3 + scrambledList[i][j].numTag.cord_Y + 1;
                                    initList[i][j] = temp;
                                }
                                else
                                {
                                    initList[i][j] = 0;
                                }
                            }
                        }
                    } while (!isSolvablePuzzle(initList));

                    isStart = false;

                    //ENABLE BUTTON
                    StartButton.IsEnabled = true;
                    HintButton.IsEnabled = true;
                    SaveButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        //CHECK SOLVABLE
        List<List<int>> initList = new List<List<int>>() { new List<int> { 0, 0, 0 },
                                                           new List<int> { 0, 0, 0 },
                                                           new List<int> { 0, 0, 0 } };
        List<List<int>> currentList = new List<List<int>>() { new List<int> { 0, 0, 0 },
                                                              new List<int> { 0, 0, 0 },
                                                              new List<int> { 0, 0, 0 } };
        List<List<int>> goalList = new List<List<int>>() { new List<int> { 1, 2, 3 },
                                                           new List<int> { 4, 5, 6 },
                                                           new List<int> { 7, 8, 0 } };
        //WINNING CHECKED VARIABLE
        bool isEnded = false;

        int countSmallerTitles(List<List<int>> list)
        {
            var puzzle1D = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int arrIndex = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    puzzle1D[arrIndex] = initList[i][j];
                    arrIndex++;
                }
            }

            int count = 0;
            for (int i = 0; i < 9 - 1; i++)
            {
                for (int j = i + 1; j < 9; j++)
                {
                    if (puzzle1D[j] < puzzle1D[i] && puzzle1D[j] != 0)
                        count++;
                }
            }
            return count;
        }

        bool isSolvablePuzzle(List<List<int>> initList)
        {
            int countingInit = countSmallerTitles(initList);
            return countingInit % 2 == 0;
        }

        void checkWinningState()
        {
            //Get current state to check win
            int null_X = 0;
            int null_Y = 0;
            GetNullPosition(ref null_X, ref null_Y);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i != null_X || j != null_Y)
                    {
                        var temp = scrambledList[i][j].numTag.cord_X * 3 + scrambledList[i][j].numTag.cord_Y + 1;
                        currentList[i][j] = temp;
                    }
                    else
                    {
                        currentList[i][j] = 0;
                    }
                }
            }

            //CHECK WIN
            int count = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (currentList[i][j] == goalList[i][j])
                        count++;
                }
            }

            if (count == 9)
            {
                isEnded = true;
            }
            else
            {
                isEnded = false;
            }

            if (isEnded)
            {
                MessageBox.Show("YOU WIN!!");
                isEnded = false;
                timerX.Stop();
                StartButton.Content = "Start";
                isStart = false;
                StartButton.IsEnabled = false;
                SaveButton.IsEnabled = false;
                HintButton.IsEnabled = false;
            }
        }

        private void Container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isStart || isEnded)
            {
                return;
            }

            isDragging = true;
            var position = e.GetPosition(this);
            //legal mouse click
            if (position.X < leftBorder && position.X > rightBorder || position.Y < topBorder && position.Y > bottomBorder)
            {
                return;
            }
            //
            var j = (int)Math.Floor((position.X - canvasLeftPadding) / (croppedImageWidth + croppedImagePadding));
            var i = (int)Math.Floor((position.Y - canvasTopPadding) / (croppedImageHeight + croppedImagePadding));
            this.Title = $"{i} - {j}";
            if (!isAbleToMove(i, j))
                return;
            if (!scrambledList[i].Any())
            {
                return;
            }
            if (i > 2 || i < 0 || j > 2 || j < 0)
                return;
            if (scrambledList[i][j] == null)
            {
                return;
            }
            selectedPiece = scrambledList[i][j];
            scrambledList[i][j] = null;
        }

        private void Container_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                if (selectedPiece == null)
                    return;

                var newPos = e.GetPosition(this);

                var j = (int)((newPos.X - canvasLeftPadding) / (croppedImageWidth + croppedImagePadding));
                var i = (int)((newPos.Y - canvasTopPadding) / (croppedImageHeight + croppedImagePadding));

                this.Title = $"{i} - {j} {newPos.X} - {newPos.Y}";

                if (newPos.X < leftBorder || newPos.X > rightBorder || newPos.Y < topBorder || newPos.Y > bottomBorder)
                {
                    return;
                }
                Canvas.SetLeft(selectedPiece.image, newPos.X - canvasLeftPadding);
                Canvas.SetTop(selectedPiece.image, newPos.Y - canvasTopPadding);
                newi = i;
                newj = j;
            }
        }

        private void Container_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isStart || isEnded)
                return;

            isDragging = false;
            if (newi > 2 || newi < 0 || newj > 2 || newj < 0)
            {
                return;
            }
            if (scrambledList[newi][newj] == null)
            {
                selectedPiece.originalPos_X = selectedPiece.newPos_X;
                selectedPiece.originalPos_Y = selectedPiece.newPos_Y;
                selectedPiece.newPos_X = newj * (croppedImageWidth + croppedImagePadding);
                selectedPiece.newPos_Y = newi * (croppedImageHeight + croppedImagePadding);
                Canvas.SetLeft(selectedPiece.image, selectedPiece.newPos_X);
                Canvas.SetTop(selectedPiece.image, selectedPiece.newPos_Y);
                scrambledList[newi][newj] = selectedPiece;
                selectedPiece = null;
            }
            else
            {
                if (selectedPiece != null)
                {
                    Canvas.SetLeft(selectedPiece.image, selectedPiece.newPos_X);
                    Canvas.SetTop(selectedPiece.image, selectedPiece.newPos_Y);
                    scrambledList[(int)(selectedPiece.newPos_Y / (croppedImageHeight + croppedImagePadding))][(int)(selectedPiece.newPos_X / (croppedImageWidth + croppedImagePadding))] = selectedPiece;
                    selectedPiece = null;
                    return;
                }
            }
            checkWinningState();
        }

        private void GetNullPosition(ref int null_X, ref int null_Y)
        {
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (scrambledList[i][j] == null)
                    {
                        //Debug.WriteLine($"{i} {j}");
                        null_X = i;
                        null_Y = j;
                    }
                }
            }
            return;
        }

        //ADD REFERENCE System.Windows.Forms
        System.Windows.Forms.Timer timerX;
        bool isStart = false;

        private void StartGame_Clicked(object sender, RoutedEventArgs e)
        {
            openButton.IsEnabled = false;

            TimerTextBox.IsReadOnly = true;
            StartButton.Content = "Pause";
            isStart = !isStart;

            if (isStart)
            {
                timerX = new System.Windows.Forms.Timer();

                timerX.Interval = 1000;
                timerX.Tick += new EventHandler(timeX_Tick);
                timerX.Enabled = true;
            }
            else
            {
                StartButton.Content = "Start";
                timerX.Stop();
            }
        }

        public int BaseScore { get; set; }

        int OrigTime;
        private void timeX_Tick(object sender, EventArgs e)
        {
            var tempOrigTime = OrigTime;
            if (OrigTime > 0)
            {
                OrigTime--;
                var tempVal = BaseScore / tempOrigTime;
                BaseScore -= tempVal;
                TimerTextBox.Text = OrigTime / 3600 + ":" + OrigTime / 60 + ":" + ((OrigTime % 60) >= 10 ? (OrigTime % 60).ToString() : "0" + OrigTime % 60);
                BaseScoreTextBlock.Text = BaseScore.ToString();
            }
            else
            {
                timerX.Stop();
                StartButton.Content = "Start";
                TimerTextBox.IsReadOnly = false;
                MessageBox.Show("TIME UP!!");
                MessageBox.Show("Please press New Game to play again");
                isEnded = true;
                StartButton.IsEnabled = false;
                SaveButton.IsEnabled = false;
            }
        }

        private bool isAbleToMove(int hori_X, int verti_Y)
        {
            int null_X = 0;
            int null_Y = 0;
            GetNullPosition(ref null_X, ref null_Y);
            //Debug.WriteLine($"{null_X} {null_Y}");
            //Debug.WriteLine($"{hori_X} {verti_Y}");
            if (hori_X > 2 || verti_Y > 2)
                return false;

            if ((hori_X + 1 == null_X && verti_Y == null_Y)
                || (hori_X - 1 == null_X && verti_Y == null_Y)
                || (hori_X == null_X && verti_Y + 1 == null_Y)
                || (hori_X == null_X && verti_Y - 1 == null_Y))
            {
                return true;
            }
            return false;
        }

        string imageSource;
        FileInfo savePathFile = new FileInfo("./SaveGame.txt");
        BindingList<PuzzlePiece> savedPuzzlePieceList = new BindingList<PuzzlePiece>();
        string saveLoc = "./saveGame.txt";
        private void SaveGameButton_Click(object sender, RoutedEventArgs e)
        {
            isStart = false;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)| *.txt";
            saveFileDialog.DefaultExt = "*.txt";
            saveFileDialog.OverwritePrompt = true;
            //TODO: get preset name/location from user (save to saveLoc)


            if (saveFileDialog.ShowDialog() == true)
            {

                saveLoc = saveFileDialog.FileName;
                var doc = new XmlDocument();

                var root = doc.CreateElement("Game");
                root.SetAttribute("IsStart", isStart.ToString());
                root.SetAttribute("Timer", TimerTextBox.Text.ToString());
                root.SetAttribute("Score", BaseScoreTextBlock.Text.ToString());
                root.SetAttribute("ImageSource", imageSource);

                var state = doc.CreateElement("State");
                root.AppendChild(state);

                int null_X = 0;
                int null_Y = 0;
                GetNullPosition(ref null_X, ref null_Y);

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var piece = doc.CreateElement("Piece");
                        if (i != null_X || j != null_Y)
                        {
                            piece.SetAttribute("Tag", $"{scrambledList[i][j].numTag.cord_X},{scrambledList[i][j].numTag.cord_Y}");
                            piece.SetAttribute("Original_Position", $"{scrambledList[i][j].originalPos_X},{scrambledList[i][j].originalPos_Y}");
                            piece.SetAttribute("New_Position", $"{scrambledList[i][j].newPos_X},{scrambledList[i][j].newPos_Y}");
                            state.AppendChild(piece);
                        }
                        else
                        {
                            piece.SetAttribute("Tag", $"{null_X},{null_Y}");
                            piece.SetAttribute("Original_Position", $"null");
                            piece.SetAttribute("New_Position", $"null");
                            state.AppendChild(piece);
                        }
                    }

                }

                doc.AppendChild(root);

                doc.Save(saveFileDialog.FileName);
                MessageBox.Show("Save successfully");
            }
            isStart = true;
        }

        private void LoadGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                clearCanvas();
                var LoadFileDialog = new OpenFileDialog();
                LoadFileDialog.Filter = "Text file (*.txt)| *.txt";
                LoadFileDialog.DefaultExt = "*.txt";

                if (LoadFileDialog.ShowDialog() == true)
                {
                    var doc = new XmlDocument();
                    doc.Load(LoadFileDialog.FileName);
                    var root = doc.DocumentElement;
                    puzzlePieceList = new List<List<PuzzlePiece>>();

                    TimerTextBox.Text = root.Attributes["Timer"].Value;
                    OrigTime = (int)System.TimeSpan.Parse(root.Attributes["Timer"].Value).TotalSeconds;
                    BaseScoreTextBlock.Text = root.Attributes["Score"].Value;
                    BaseScore = int.Parse(root.Attributes["Score"].Value);

                    imageSource = root.Attributes["ImageSource"].Value;

                    var coreImage = new BitmapImage();
                    coreImage.BeginInit();
                    coreImage.UriSource = new Uri(imageSource);
                    coreImage.DecodePixelHeight = 420;
                    coreImage.DecodePixelWidth = 420;
                    coreImage.EndInit();

                    HintImage.Source = coreImage;

                    var coreImageWidth = 420;
                    croppedImageWidth = (int)coreImageWidth / 3;
                    croppedImageHeight = (int)(coreImageWidth * coreImage.Height / coreImage.Width) / 3;

                    for (int i = 0; i < 3; i++)
                    {
                        var list = new List<PuzzlePiece>();
                        for (int j = 0; j < 3; j++)
                        {
                            if (i != 2 || j != 2)
                            {
                                var croppedImage = new CroppedBitmap(coreImage, new Int32Rect(
                                        (int)(j * coreImage.Width / 3), (int)(i * coreImage.Height / 3),
                                        (int)coreImage.Width / 3, (int)coreImage.Height / 3));
                                var imagePiece = new PuzzlePiece() { image = new Image() { Source = croppedImage, Width = croppedImageWidth, Height = croppedImageHeight } };

                                imagePiece.originalPos_X = j * (croppedImageWidth + croppedImagePadding);
                                imagePiece.originalPos_Y = i * (croppedImageHeight + croppedImagePadding);
                                imagePiece.numTag = new DogTag();
                                imagePiece.numTag.cord_X = i;
                                imagePiece.numTag.cord_Y = j;
                                list.Add(imagePiece);
                            }
                        }
                        puzzlePieceList.Add(list);
                    }
                    int k = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            var tag = (root.FirstChild.ChildNodes[k].Attributes["Tag"].Value).Split(',');
                            var originalPos = root.FirstChild.ChildNodes[k].Attributes["Original_Position"].Value.Split(',');
                            var newPos = root.FirstChild.ChildNodes[k].Attributes["New_Position"].Value.Split(',');

                            int tag_X = int.Parse(tag[0]);
                            int tag_Y = int.Parse(tag[1]);
                            if (originalPos[0] == "null")
                            {
                                scrambledList[i][j] = null;
                            }
                            else
                            {
                                scrambledList[i][j] = puzzlePieceList[tag_X][tag_Y];
                                scrambledList[i][j].originalPos_X = int.Parse(originalPos[0]);
                                scrambledList[i][j].originalPos_Y = int.Parse(originalPos[1]);
                                scrambledList[i][j].newPos_X = int.Parse(newPos[0]);
                                scrambledList[i][j].newPos_Y = int.Parse(newPos[1]);
                                container.Children.Add(scrambledList[i][j].image);
                                Canvas.SetLeft(scrambledList[i][j].image, scrambledList[i][j].newPos_X);
                                Canvas.SetTop(scrambledList[i][j].image, scrambledList[i][j].newPos_Y);
                            }
                            k++;
                        }
                    }

                    //ENABLE BUTTON
                    if (puzzlePieceList.Count != 0)
                    {
                        StartButton.IsEnabled = true;
                        HintButton.IsEnabled = true;
                        SaveButton.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void clearCanvas()
        {
            for (int index = container.Children.Count - 1; index >= 0; index--)
            {
                if (container.Children[index] is Image)
                {
                    container.Children.RemoveAt(index);
                }

            }
        }

        public class ToolBar : System.Windows.Controls.ToolBar
        {
            public override void OnApplyTemplate()
            {
                base.OnApplyTemplate();

                var overflowPanel = base.GetTemplateChild("PART_ToolBarOverflowPanel") as ToolBarOverflowPanel;
                if (overflowPanel != null)
                {
                    overflowPanel.Background = OverflowPanelBackground ?? Background;
                    overflowPanel.Margin = new Thickness(0);
                }
            }

            public Brush OverflowPanelBackground
            {
                get;
                set;
            }
        }
        /// <summary>
        /// TitleBar_MouseDown - Drag if single-click, resize if double-click
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                {
                    //AdjustWindowSize();
                }
                else
                {
                    Application.Current.MainWindow.DragMove();
                }
        }


        /// <summary>
        /// Minimized Button_Clicked
        /// </summary>
        private void MinimizeButton_Clicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HintButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (HintImage.Visibility == Visibility.Collapsed)
            {
                HintImage.Visibility = Visibility.Visible;
            }
            else
            {
                HintImage.Visibility = Visibility.Collapsed;
            }
        }

        private void NewGame_Clicked(object sender, RoutedEventArgs e)
        {
            if (puzzlePieceList.Count == 0)
                return;

            openButton.IsEnabled = true;
            HintButton.IsEnabled = false;
            HintImage.Source = null;
            BaseScore = 1000;
            BaseScoreTextBlock.Text = BaseScore.ToString();
            SaveButton.IsEnabled = false;
            isStart = false;
            OrigTime = 200;
            TimerTextBox.Text = "00:03:20";
            if (timerX != null)
            {
                timerX.Stop();
            }      
            StartButton.IsEnabled = false;
            StartButton.Content = "Start";
            puzzlePieceList.Clear();
            scrambledList.Clear();
            for (int i = 0; i < 3; i++)
            {
                var list = new List<PuzzlePiece>();
                for (int j = 0; j < 3; j++)
                {
                    var image = new PuzzlePiece();
                    list.Add(image);
                }
                scrambledList.Add(list);
            }
            scrambledList[2][2] = null;
            leftBorder = canvasLeftPadding;
            rightBorder = container.Width + canvasLeftPadding;
            topBorder = canvasTopPadding + TitleBar.Height;
            bottomBorder = container.Height + canvasTopPadding;
            clearCanvas();
            isEnded = false;
        }
    }
}