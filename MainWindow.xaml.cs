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

        class PuzzlePiece
        {
            public Image image { get; set; }
            public int originalPos_X { get; set; }
            public int originalPos_Y { get; set; }
            public int newPos_X { get; set; }
            public int newPos_Y { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
        }

        List<List<PuzzlePiece>> puzzlePieceList = new List<List<PuzzlePiece>>();
        List<List<PuzzlePiece>> scrambledList = new List<List<PuzzlePiece>>();
        Image[,] images = new Image[3, 3];
        int croppedImageWidth;
        int croppedImageHeight;
        int croppedImagePadding = 5;

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                puzzlePieceList = new List<List<PuzzlePiece>>();
                container.Children.Clear();
                var coreImage = new BitmapImage(new Uri(screen.FileName));
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
                            //container.Children.Add(imagePiece.image);
                            imagePiece.originalPos_X = j * (croppedImageWidth + croppedImagePadding);
                            imagePiece.originalPos_Y = i * (croppedImageHeight + croppedImagePadding);
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
            isDragging = false;
            if (selectedPiece != null)
            {
                Canvas.SetLeft(selectedPiece.image, newj * (croppedImageWidth + croppedImagePadding));
                Canvas.SetTop(selectedPiece.image, newi * (croppedImageHeight + croppedImagePadding));
                scrambledList[newi][newj] = selectedPiece;
                selectedPiece = null;
            }
            else
            {
                return;
            }
        }

        //ADD REFERENCE System.Windows.Forms
        System.Windows.Forms.Timer timerX;
        private void StartGame_Clicked(object sender, RoutedEventArgs e)
        {
            timerX = new System.Windows.Forms.Timer();

            timerX.Interval = 1000;
            timerX.Tick += new EventHandler(timeX_Tick);
            timerX.Enabled = true;
        }

        int OrigTime = 180;
        private void timeX_Tick(object sender, EventArgs e)
        {
            if (OrigTime > 0)
            {
                OrigTime--;
                TimerTextBlock.Text = OrigTime / 60 + ":" + ((OrigTime % 60) >= 10 ? (OrigTime % 60).ToString() : "0" + OrigTime % 60);
            }
            else
            {
                timerX.Stop();
            }
        }

        private void PauseButton_Clicked(object sender, RoutedEventArgs e)
        {
            timerX.Stop();
        }
    }
}
