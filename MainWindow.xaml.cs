using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TombaEdit.Logic;

namespace TombaEdit
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static string Version => "TombaEdit - v. 0.1";


        private void ButtonInputFile_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if ((bool) dialog.ShowDialog(this)) TextInputFile.Text = dialog.FileName;
        }

        private void ButtonOutputFile_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            if ((bool) dialog.ShowDialog(this)) TextOutputFile.Text = dialog.FileName;
        }

        private void ButtonPack_OnClick(object sender, RoutedEventArgs e)
        {
            ButtonPack.IsEnabled = false;
            PackProgressBar.IsEnabled = true;
            foreach (var i in Gam.Pack(TextInputFile.Text, TextOutputFile.Text))
            {
                Dispatcher.Invoke(() =>
                {
                    PackProgressBar.Value = i;
                    PackProgressText.Text = $"{i:F2}%";
                });
                Debug.WriteLine(i);
            }
            PackProgressBar.IsEnabled = false;
            RefreshButtonPack();
        }

        private void ClearInputFile_OnClick(object sender, RoutedEventArgs e)
        {
            TextInputFile.Text = "";
        }

        private void ClearOutputFile_OnClick(object sender, RoutedEventArgs e)
        {
            TextOutputFile.Text = "";
        }

        private void CopyOutputFile_OnClick(object sender, RoutedEventArgs e)
        {
            DoCopyOutputFile();
        }

        private void DoCopyOutputFile()
        {
            var outFilename = Path.ChangeExtension(TextInputFile.Text, "gam");
            TextOutputFile.Text = outFilename;
        }

        private void RefreshButtonPack()
        {
            ButtonPack.IsEnabled = TextInputFile.Text.Length != 0 && TextOutputFile.Text.Length != 0;
        }

        private void TextInputFile_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextInputFile.Text.Length != 0 && TextOutputFile.Text.Length == 0) DoCopyOutputFile();
            ClearInputFile.IsEnabled = TextInputFile.Text.Length != 0;
            CopyOutputFile.IsEnabled = TextInputFile.Text.Length != 0;
            RefreshButtonPack();
        }

        private void TextOutputFile_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ClearOutputFile.IsEnabled = TextOutputFile.Text.Length != 0;
            RefreshButtonPack();
        }
    }
}