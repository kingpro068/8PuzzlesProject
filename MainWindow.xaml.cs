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

namespace _8PuzzleProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        class DogTag
        {
            public int cord_X { get; set; }
            public int cord_Y { get; set; }
        }

        class PuzzlePiece
        {
            public Image image { get; set; }
            public DogTag numTag {get; set;}
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
            OrigTime = (int) System.TimeSpan.Parse(time).TotalSeconds;
            BaseScoreTextBlock.Text = BaseScore.ToString();

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
        }

        List<List<PuzzlePiece>> puzzlePieceList = new List<List<PuzzlePiece>>();
        List<List<PuzzlePiece>> scrambledList = new List<List<PuzzlePiece>>();
        Image[,] images = new Image[3, 3];
        int croppedImageWidth;
        int croppedImageHeight;
        int croppedImagePadding = 5;
        string imageSource;

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                puzzlePieceList = new List<List<PuzzlePiece>>();
                container.Children.Clear();
                var coreImage = new BitmapImage(new Uri(screen.FileName));
                imageSource = screen.FileName;

                var coreImageWidth = 420;
                croppedImageWidth = (int)coreImageWidth / 3;
                croppedImageHeight = (int)(coreImageWidth * coreImage.Height / coreImage.Width) / 3;
                var tag = 0;
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

                            //var croppedImage = new CroppedBitmap(coreImage, new Int32Rect(
                            //        (int)(j * coreImage.DecodePixelWidth / 3), (int)(i * coreImage.PixelHeight / 3),
                            //        (int)coreImage.PixelWidth / 3, (int)coreImage.PixelHeight / 3));

                            var imagePiece = new PuzzlePiece() { image = new Image() { Source = croppedImage, Width = croppedImageWidth, Height = croppedImageHeight } };
                            //container.Children.Add(imagePiece.image);
                            imagePiece.originalPos_X = j * (croppedImageWidth + croppedImagePadding);
                            imagePiece.originalPos_Y = i * (croppedImageHeight + croppedImagePadding);
                            imagePiece.numTag.cord_X = i;
                            imagePiece.numTag.cord_Y = j;
                            list.Add(imagePiece);
                            
                            //Canvas.SetLeft(imagePiece.image, imagePiece.originalPos_X);
                            //Canvas.SetTop(imagePiece.image, imagePiece.originalPos_Y);
                        }
                    }
                    puzzlePieceList.Add(list);
                }

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
                            container.Children.Add(imagePiece.image);
                            imagePiece.newPos_X = poolj[k] * (croppedImageWidth + croppedImagePadding);
                            imagePiece.newPos_Y = pooli[k] * (croppedImageHeight + croppedImagePadding);

                            scrambledList[pooli[k]].Insert(poolj[k], imagePiece);
                            scrambledList[pooli[k]].RemoveAt(poolj[k]+1);

                            //list.Add(imagePiece);
                            Canvas.SetLeft(imagePiece.image, imagePiece.newPos_X);
                            Canvas.SetTop(imagePiece.image, imagePiece.newPos_Y);
                            pool.RemoveAt(k);
                            pooli.RemoveAt(k);
                            poolj.RemoveAt(k);
                        }
                    }
                }
            }
        }
        int canvasLeftPadding = 52;
        int canvasTopPadding = 42;
        bool isDragging = false;
        PuzzlePiece selectedPiece = new PuzzlePiece();
        int newi = -1;
        int newj = -1;
        int leftBorder = 52;
        int rightBorder = 435 + 50;
        int topBorder = 42;
        int bottomBorder = 435 + 40;
        private void Container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!isStart)
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
            var j = (int)((position.X - canvasLeftPadding) / (croppedImageWidth + croppedImagePadding));
            var i = (int)((position.Y - canvasTopPadding) / (croppedImageHeight + croppedImagePadding));

            this.Title = $"{i} - {j}";
            if (!scrambledList[i].Any())
            {
                return;
            }
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
                // Toa do moi
                //
                
                var j = (int)((newPos.X - canvasLeftPadding) / (croppedImageWidth + croppedImagePadding ));
                var i = (int)((newPos.Y - canvasTopPadding) / (croppedImageHeight + croppedImagePadding));

                this.Title = $"{i} - {j} {newPos.X} - {newPos.Y}";
                
                //
                Canvas.SetLeft(selectedPiece.image, newPos.X - canvasLeftPadding);
                Canvas.SetTop(selectedPiece.image, newPos.Y - canvasTopPadding);
                newi = i;
                newj = j;
            }
        }

        private void Container_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isStart)
                return;

            isDragging = false;
            if (scrambledList[newi][newj] == null)
            {
                selectedPiece.newPos_X = newj * (croppedImageWidth + croppedImagePadding);
                selectedPiece.newPos_Y = newi * (croppedImageHeight + croppedImagePadding);
                Canvas.SetLeft(selectedPiece.image, selectedPiece.newPos_X);
                Canvas.SetTop(selectedPiece.image, selectedPiece.newPos_Y);
                
                scrambledList[newi][newj] = selectedPiece;
                selectedPiece = null;
            }
            else
            {
                Canvas.SetLeft(selectedPiece.image, selectedPiece.newPos_X);
                Canvas.SetTop(selectedPiece.image, selectedPiece.newPos_Y);
                return;
            }
        }

        //ADD REFERENCE System.Windows.Forms
        System.Windows.Forms.Timer timerX;
        bool isStart = false;

        private void StartGame_Clicked(object sender, RoutedEventArgs e)
        {
            TimerTextBox.IsReadOnly = true;
            StartButton.Content = "Pause";
            isStart = !isStart;
            
            if(isStart)
            {
                timerX = new System.Windows.Forms.Timer();

                timerX.Interval = 10;
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
            }
        }

        private void GetNullPosition(ref int null_X, ref int null_Y)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (scrambledList[i][j] == null)
                    {
                        null_X = i;
                        null_Y = j;
                    }
                }
            }
            return;
        }

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
                        piece.SetAttribute("Tag", $"{scrambledList[i][j].numTag.cord_X}",{scrambledList[i][j].numTag.cord_Y}");
                        piece.SetAttribute("Tag", $"{scrambledList[i][j].originalPos_X},{scrambledList[i][j].originalPos_Y}");
                        piece.SetAttribute("Tag", $"{scrambledList[i][j].newPos_X},{scrambledList[i][j].newPos_Y}");
                        state.AppendChild(piece);
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
            List<List<PuzzlePiece>> tempScrambledList = new List<List<PuzzlePiece>>();
            var LoadFileDialog = new OpenFileDialog();
            if (LoadFileDialog.ShowDialog() == true)
            {
                var doc = new XmlDocument();
                doc.Load(LoadFileDialog.FileName);
                var root = doc.DocumentElement;
                puzzlePieceList = new List<List<PuzzlePiece>>();
                container.Children.Clear();
                imageSource = root.Attributes["ImageSource"].Value;
                var coreImage = new BitmapImage(new Uri(imageSource));

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
                            imagePiece.image.Tag = new Tuple<int, int>(i, j);
                            list.Add(imagePiece);
                        }
                    }
                    puzzlePieceList.Add(list);
                }

                for (int i = 0; i < 3; i++)
                {
                    var line = root.FirstChild.ChildNodes[i].Attributes["Value"].Value;
                    var tokens = line.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; i < 3; j++)
                    {
                        int posX = int.Parse(imagePos[0]);
                        int posY = int.Parse(imagePos[1]);
                        if (posX == -1)
                        {
                            scrambledList[i][j] = null;
                        }
                        else
                        {
                            scrambledList[i][j] = puzzlePieceList[posX][posY];
                        }
                    }
                }
            }

        }
    }
}
