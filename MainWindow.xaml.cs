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

        }
        List<PuzzlePiece> puzzlePieceList= new List<PuzzlePiece>();
        Image[,] images = new Image[3, 3];
        int croppedImageWidth;
        int croppedImageHeight;
        int croppedImagePadding = 5;

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                var coreImage = new BitmapImage(new Uri(screen.FileName));
                var coreImageWidth = 300;
                croppedImageWidth = (int)coreImageWidth / 3;
                croppedImageHeight = (int)(coreImageWidth * coreImage.Height / coreImage.Width) / 3;
                
                var rng = new Random();
                var pool = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
                var pooli = new List<int> { 0, 0, 0, 1, 1, 1, 2, 2 };
                var poolj = new List<int> { 0, 1, 2, 0, 1, 2, 0, 1 };

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (i != 2 || j != 2)
                        {
                            // Set vi tri
                            var k = rng.Next(pool.Count); // Chon ngau nhien mot chi muc trong pool

                            var croppedImage = new CroppedBitmap(coreImage, new Int32Rect(
                                (int)(pooli[k] * coreImage.Width / 3), (int)(poolj[k] * coreImage.Height / 3),
                                (int)coreImage.Width / 3, (int)coreImage.Height / 3));
                            var imagePiece = new PuzzlePiece() { image = new Image() { Source = croppedImage, Width = croppedImageWidth, Height = croppedImageHeight} };
                            // Tao giao dien
                            var imageView = new Image();
                            imageView.Source = croppedImage;
                            imageView.Width = croppedImageWidth;
                            imageView.Height = croppedImageHeight;
                            container.Children.Add(imagePiece.image);

                            //
                            Canvas.SetLeft(imagePiece.image, j * (croppedImageWidth + croppedImagePadding));
                            Canvas.SetTop(imagePiece.image, i * (croppedImageHeight + croppedImagePadding));

                            images[i, j] = imagePiece.image;

                            pool.RemoveAt(k);
                            pooli.RemoveAt(k);
                            poolj.RemoveAt(k);
                        }
                    }
                }
            }
        }

        bool isDragging = false;
        Image selectedImage = null;
        int newi = -1;
        int newj = -1;

        private void Container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var position = e.GetPosition(this);
            //if(position.X)
            //
            var j = (int)(position.X / (croppedImageWidth + croppedImagePadding));
            var i = (int)(position.Y / (croppedImageHeight + croppedImagePadding));

            this.Title = $"{i} - {j}";

            selectedImage = images[i, j];
            images[i, j] = null;
        }

        private void Container_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var newPos = e.GetPosition(this);
                // Toa do moi
                //
                var j = (int)(newPos.X / (croppedImageWidth + croppedImagePadding));
                var i = (int)(newPos.Y / (croppedImageHeight + croppedImagePadding));

                this.Title = $"{i} - {j}";

                //
                newi = i;
                newj = j;

                Canvas.SetLeft(selectedImage, newPos.X);
                Canvas.SetTop(selectedImage, newPos.Y);
            }
        }

        private void Container_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            if (selectedImage != null && images[newi, newj] == null)
            { 
                Canvas.SetLeft(selectedImage, newj * (croppedImageWidth + croppedImagePadding));
                Canvas.SetTop(selectedImage, newi * (croppedImageHeight + croppedImagePadding));
                images[newi, newj] = selectedImage;
            }
            else
            {
                return;
            }
        }
    }
}
