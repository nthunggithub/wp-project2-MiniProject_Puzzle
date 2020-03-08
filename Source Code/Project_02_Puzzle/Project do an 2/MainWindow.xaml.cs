using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Project_do_an_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer timeDown;
        TimeSpan time;
        public MainWindow()
        {
            InitializeComponent();
            time = TimeSpan.FromSeconds(1);
            timeDown = new DispatcherTimer
            {
                Interval = time
            };
            timeDown.Tick += new EventHandler(TimeDown_Tick);
            timeDown.Start();
        }
        bool isplay = true;
        int seconds = 300;
        private void TimeDown_Tick(object sender, EventArgs e)
        {
            timer.Text = SecondToTime(seconds);
            if (seconds == 0)
            {
                timeDown.Stop();
                isplay = false;
                MessageBox.Show("You lose!");
            }
            seconds--;
        }
        
            
        private string SecondToTime(int seconds)
        {
             int h = 0;
             int m = Math.Abs((seconds % 3600) / 60);
             int s = seconds - (m * 60);
            return $"{h}:{m}:{s}";
        }

        int startX = 370;
        int startY = 70;
        int rows = 3;
        int cols = 3;
        int width = 110;
        int height = 110;
        int preImageStartX = 10;
        int preImageStartY = 70;
        List<Image> piecesImage = new List<Image>();

        string filename = "";

        
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            
            var screen = new OpenFileDialog();

            if(screen.ShowDialog() == true)
            {
                filename = screen.FileName;

                var source = new BitmapImage(new Uri(screen.FileName, UriKind.Absolute));

                previewImage.Width = 300;
                previewImage.Height = 240;

                previewImage.Source = source;

                Canvas.SetLeft(previewImage, preImageStartX);
                Canvas.SetTop(previewImage, preImageStartY);

                for(int i=0; i<rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (!((i == 2) && (j == 2)))
                        {
                            var h = (int)source.Height / 3;
                            var w = (int)source.Width / 3;

                            var rect = new Int32Rect(j * w, i * h, w, h);
                            var cropBitmap = new CroppedBitmap(source, rect);

                            var cropImage = new Image();
                            cropImage.Stretch = Stretch.Fill;
                            cropImage.Width = width;
                            cropImage.Height = height;
                            cropImage.Source = cropBitmap;
                            //canvas.Children.Add(cropImage);
                            //Canvas.SetLeft(cropImage, startX + j * (width + 2));
                            //Canvas.SetTop(cropImage, startY + i * (height + 2));

                            
                            cropImage.Tag = new Tuple<int, int, int, int>(i, j,-1,-1);

                            piecesImage.Add(cropImage);
                        }
                    }
                }

                var indexs = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
                var rng = new Random();

                for(int i=0; i<rows; i++)
                {
                    for(int j=0; j<rows; j++)
                    {
                        if (!(i == 2 && j == 2))
                        {
                            int index = rng.Next(indexs.Count);

                            var img = piecesImage[indexs[index]];

                            canvas.Children.Add(img);

                            var tag = img.Tag as Tuple<int, int, int, int>;
                            img.Tag = new Tuple<int, int,int, int>(tag.Item1,tag.Item2, i, j);

                            Canvas.SetLeft(img, startX + j * (width + 2));
                            Canvas.SetTop(img, startY + i * (height + 2));

                            img.MouseLeftButtonDown += Img_MouseLeftButtonDown; ;
                            img.PreviewMouseLeftButtonUp += Img_PreviewMouseLeftButtonUp;
                            

                            indexs.RemoveAt(index);


                        }
                    }
                }
            }

        }

        

        private void Img_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            _isDragging = false;
            var position = e.GetPosition(this);

            int x = (int)(position.X - startX) / (width + 2) * (width + 2) + startX;
            int y = (int)(position.Y - startY) / (height + 2) * (height + 2) + startY;

            int n = (y - startY) / height;
            int m = (x - startX) / width;

            var curTag = _selectedBitmap.Tag as Tuple<int, int, int, int>;

            //tra lai vi tri ban dau
            if ((Math.Abs(n - curTag.Item3) + Math.Abs(m - curTag.Item4)) > 1)
            {
                Canvas.SetLeft(_selectedBitmap, startX + curTag.Item4 * (width + 2));
                Canvas.SetTop(_selectedBitmap, startY + curTag.Item3 * (height + 2));

                return;
            }



            if (IsMoveToBlank(n, m))
            {
                //thay doi vi tri hien tai cua pieceImage
                var tag = _selectedBitmap.Tag as Tuple<int, int, int, int>;
                _selectedBitmap.Tag = new Tuple<int, int, int, int>(tag.Item1, tag.Item2, n, m);

                Canvas.SetLeft(_selectedBitmap, x);
                Canvas.SetTop(_selectedBitmap, y);
            }
            else
            {
                Canvas.SetLeft(_selectedBitmap, startX + curTag.Item4 * (width + 2));
                Canvas.SetTop(_selectedBitmap, startY + curTag.Item3 * (height + 2));
            }

            if (CheckWin())
            {
                MessageBox.Show("You win!");
            }

        }

        private bool CheckWin()
        {
            for(int i=0; i< piecesImage.Count; i++)
            {
                var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;
                if (tag.Item1 != tag.Item3 || tag.Item2 != tag.Item4)
                    return false;
            }
            isplay = false;
            return true;
        }

        private bool IsMoveToBlank(int x, int y)
        {
            if (x >= rows || x < 0 || y >= cols || y < 0)
                return false;
            for(int i=0;i< piecesImage.Count; i++)
            {
                var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;

                if (tag.Item3 == x && tag.Item4 == y)
                    return false;
            }
            return true;
        }

        bool _isDragging = false;
        Image _selectedBitmap = null;
        Point _lastPosition;
        private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = e.Source as Image;
            if (image != null)
            {
                _isDragging = true;
                _selectedBitmap = image;
                _lastPosition = e.GetPosition(this);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            int i = ((int)position.Y - startY) / height;
            int j = ((int)position.X - startX) / width;

            //this.Title = $"{position.X} - {position.Y}, a[{i}][{j}]";
            if (isplay == true)
            {
                if (_isDragging)
                {
                    var dx = position.X - _lastPosition.X;
                    var dy = position.Y - _lastPosition.Y;

                    var lastLeft = Canvas.GetLeft(_selectedBitmap);
                    var lastTop = Canvas.GetTop(_selectedBitmap);
                    Canvas.SetLeft(_selectedBitmap, lastLeft + dx);
                    Canvas.SetTop(_selectedBitmap, lastTop + dy);

                    _lastPosition = position;
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (isplay == true)
            {
                if (e.Key == Key.Up)
                {

                    for (int i = 0; i < piecesImage.Count; i++)
                    {
                        var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;

                        if (UpBlank(tag.Item3, tag.Item4))
                        {
                            Canvas.SetLeft(piecesImage[i], startX + (tag.Item4) * (width + 2));
                            Canvas.SetTop(piecesImage[i], startY + (tag.Item3 - 1) * (height + 2));

                            piecesImage[i].Tag = new Tuple<int, int, int, int>(tag.Item1, tag.Item2, tag.Item3 - 1, tag.Item4);
                            break;
                        }
                    }
                }
                else if (e.Key == Key.Down)
                {
                    for (int i = 0; i < piecesImage.Count; i++)
                    {
                        var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;

                        if (DownBlank(tag.Item3, tag.Item4))
                        {
                            Canvas.SetLeft(piecesImage[i], startX + (tag.Item4) * (width + 2));
                            Canvas.SetTop(piecesImage[i], startY + (tag.Item3 + 1) * (height + 2));

                            piecesImage[i].Tag = new Tuple<int, int, int, int>(tag.Item1, tag.Item2, tag.Item3 + 1, tag.Item4);
                            break;
                        }
                    }
                }
                else if (e.Key == Key.Left)
                {
                    for (int i = 0; i < piecesImage.Count; i++)
                    {
                        var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;

                        if (LeftBlank(tag.Item3, tag.Item4))
                        {
                            Canvas.SetLeft(piecesImage[i], startX + (tag.Item4 - 1) * (width + 2));
                            Canvas.SetTop(piecesImage[i], startY + (tag.Item3) * (height + 2));

                            piecesImage[i].Tag = new Tuple<int, int, int, int>(tag.Item1, tag.Item2, tag.Item3, tag.Item4 - 1);
                            break;
                        }
                    }
                }
                else if (e.Key == Key.Right)
                {
                    for (int i = 0; i < piecesImage.Count; i++)
                    {
                        var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;

                        if (RightBlank(tag.Item3, tag.Item4))
                        {
                            Canvas.SetLeft(piecesImage[i], startX + (tag.Item4 + 1) * (width + 2));
                            Canvas.SetTop(piecesImage[i], startY + (tag.Item3) * (height + 2));

                            piecesImage[i].Tag = new Tuple<int, int, int, int>(tag.Item1, tag.Item2, tag.Item3, tag.Item4 + 1);
                            break;
                        }
                    }
                }
                if (CheckWin())
                {
                    MessageBox.Show("You win!");
                    isplay = false;
                }
            }
        }

        private bool RightBlank(int item3, int item4)
        {
            if ((item4 + 1) > (rows-1))
                return false;
            for (int i = 0; i < piecesImage.Count; i++)
            {
                var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;

                if (tag.Item3 == item3 && tag.Item4 == (item4 + 1))
                {
                    return false;
                }
            }
            return true;
        }
        private bool LeftBlank(int item3, int item4)
        {
            if ((item4 - 1) < 0)
                return false;
            for (int i = 0; i < piecesImage.Count; i++)
            {
                var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;

                if (tag.Item3 == item3 && tag.Item4 == (item4 - 1))
                {
                    return false;
                }
            }
            return true;
        }
        private bool DownBlank(int item3, int item4)
        {
            if ((item3 + 1) > (cols -1))
                return false;
            for (int i = 0; i < piecesImage.Count; i++)
            {
                var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;

                if (tag.Item3 == (item3 + 1) && tag.Item4 == item4)
                {
                    return false;
                }
            }
            return true;
        }

        private bool UpBlank(int item3, int item4)
        {
            if ((item3 - 1) < 0)
                return false;
            for(int i=0; i<piecesImage.Count; i++)
            {
                var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;
                
                if(tag.Item3 == (item3-1) && tag.Item4 == item4)
                {
                    return false;
                }
            }
            return true;
        }

       

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var writer = new StreamWriter("save.txt");
            writer.WriteLine(filename);
            writer.WriteLine(isplay);
            writer.WriteLine(seconds);
            
            timeDown.Stop();

                
            for (int i=0; i< piecesImage.Count; i++)
            {
                var tag = piecesImage[i].Tag as Tuple<int, int, int, int>;

                writer.WriteLine($"{tag.Item1} {tag.Item2} {tag.Item3} {tag.Item4}");
            }

            writer.Close();
            MessageBox.Show("Saved successfully!");
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();

            if(screen.ShowDialog() == true)
            {
                
                var filenameload = screen.FileName;
                var reader = new StreamReader(filenameload);
                
                var firstLine = reader.ReadLine();
                isplay = bool.Parse(reader.ReadLine());
                seconds = int.Parse(reader.ReadLine());
                timeDown.Start();

               

                RemoveImageGame();

                piecesImage.Clear();


                var source = new BitmapImage(new Uri(firstLine, UriKind.Absolute));

                previewImage.Width = 300;
                previewImage.Height = 240;

                previewImage.Source = source;

                Canvas.SetLeft(previewImage, preImageStartX);
                Canvas.SetTop(previewImage, preImageStartY);

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (!((i == 2) && (j == 2)))
                        {
                            var h = (int)source.Height / 3;
                            var w = (int)source.Width / 3;

                            var rect = new Int32Rect(j * w, i * h, w, h);
                            var cropBitmap = new CroppedBitmap(source, rect);

                            var cropImage = new Image();
                            cropImage.Stretch = Stretch.Fill;
                            cropImage.Width = width;
                            cropImage.Height = height;
                            cropImage.Source = cropBitmap;
                            //canvas.Children.Add(cropImage);
                            //Canvas.SetLeft(cropImage, startX + j * (width + 2));
                            //Canvas.SetTop(cropImage, startY + i * (height + 2));


                            cropImage.Tag = new Tuple<int, int, int, int>(i, j, -1, -1);

                            piecesImage.Add(cropImage);
                        }
                    }
                }

             
                    for (int i = 0; i < piecesImage.Count; i++)
                    {
                        var token = reader.ReadLine().Split(new string[] { " " }, StringSplitOptions.None);

                        int size = token.Length;
                        int[] arr = new int[size];
                        for (int j = 0; j < size; j++)
                        {
                            arr[j] = int.Parse(token[j]);
                        }
                        if (arr.Length == 4)
                            piecesImage[i].Tag = new Tuple<int, int, int, int>(arr[0], arr[1], arr[2], arr[3]);


                        //int index = rng.Next(piecesImage.Count);
                        //int index = arr[0] * rows + arr[1];

                        var img = piecesImage[i];

                        canvas.Children.Add(img);



                        Canvas.SetLeft(img, startX + arr[3] * (width + 2));
                        Canvas.SetTop(img, startY + arr[2] * (height + 2));

                        img.MouseLeftButtonDown += Img_MouseLeftButtonDown; ;
                        img.PreviewMouseLeftButtonUp += Img_PreviewMouseLeftButtonUp;



                    }
                if (isplay == false)
                {
                    MessageBox.Show("You lose!");

                }

            }
            
        }

        private void RemoveImageGame()
        {
            
            for (int i = 0; i < piecesImage.Count; i++)
            {
                canvas.Children.Remove(piecesImage[i]);
            }
        }


       

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
           
            RemoveImageGame();
            previewImage.Source = null;



        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                filename = screen.FileName;

                var source = new BitmapImage(new Uri(screen.FileName, UriKind.Absolute));

                previewImage.Width = 300;
                previewImage.Height = 240;

                previewImage.Source = source;

                Canvas.SetLeft(previewImage, preImageStartX);
                Canvas.SetTop(previewImage, preImageStartY);

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (!((i == 2) && (j == 2)))
                        {
                            var h = (int)source.Height / 3;
                            var w = (int)source.Width / 3;

                            var rect = new Int32Rect(j * w, i * h, w, h);
                            var cropBitmap = new CroppedBitmap(source, rect);

                            var cropImage = new Image();
                            cropImage.Stretch = Stretch.Fill;
                            cropImage.Width = width;
                            cropImage.Height = height;
                            cropImage.Source = cropBitmap;
                            //canvas.Children.Add(cropImage);
                            //Canvas.SetLeft(cropImage, startX + j * (width + 2));
                            //Canvas.SetTop(cropImage, startY + i * (height + 2));


                            cropImage.Tag = new Tuple<int, int, int, int>(i, j, -1, -1);

                            piecesImage.Add(cropImage);
                        }
                    }
                }

                var indexs = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
                var rng = new Random();

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        if (!(i == 2 && j == 2))
                        {
                            int index = rng.Next(indexs.Count);

                            var img = piecesImage[indexs[index]];

                            canvas.Children.Add(img);

                            var tag = img.Tag as Tuple<int, int, int, int>;
                            img.Tag = new Tuple<int, int, int, int>(tag.Item1, tag.Item2, i, j);

                            Canvas.SetLeft(img, startX + j * (width + 2));
                            Canvas.SetTop(img, startY + i * (height + 2));

                            img.MouseLeftButtonDown += Img_MouseLeftButtonDown; ;
                            img.PreviewMouseLeftButtonUp += Img_PreviewMouseLeftButtonUp;


                            indexs.RemoveAt(index);


                        }
                    }
                }
            }
        }
    }
}
