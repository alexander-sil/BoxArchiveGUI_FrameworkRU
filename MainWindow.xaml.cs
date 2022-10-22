using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Shapes;

namespace BoxArchiveGUI
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

        private void DecompressButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.AddExtension = true;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;
            dialog.Title = "Выбор коробки";
            dialog.Filter = "Файлы коробок (*.box)|*.BOX";

            dialog.InitialDirectory = Directory.GetCurrentDirectory();

            string filename;

            if (dialog.ShowDialog() == true)
            {
                filename = dialog.FileName;

                Logic.UnboxBoxFile(filename);
                Logic.OpenCans();
                Logic.UnpackInts();

                if (Directory.Exists("input")) { Directory.Delete("input", true); }
                if (Directory.Exists("canned")) { Directory.Delete("canned", true); }

                MessageBoxResult answer = MessageBox.Show("Показать распакованные файлы?", "", MessageBoxButton.YesNo);

                if (answer == MessageBoxResult.Yes)
                {
                    Process.Start("C:\\Windows\\explorer.exe", $"\"{System.IO.Path.Combine(Directory.GetCurrentDirectory(), "output")}\"");
                }
            }

            if (Directory.Exists("unboxed")) { Directory.Delete("unboxed", true); }
            if (Directory.Exists("uncanned")) { Directory.Delete("uncanned", true); }
        }

        private void CompressButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1();
            window.Show();
        }
    }
}
