using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Microsoft.Win32;
using FileUtilities.WPF.Models;
using System.Threading;

namespace FileUtilities.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RepeatedFilesFinder _repeatedFilesFinder;
        private TagExtractor _tagExtractor;
        private Thread _currentThread;
        private System.Windows.Forms.FolderBrowserDialog _folderBrowser;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }
        
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
             _repeatedFilesFinder = new RepeatedFilesFinder();
             _repeatedFilesFinder.StatusChanged += _repeatedFilesFinder_StatusChanged;

            _tagExtractor = new TagExtractor();
            _tagExtractor.StatusChanged += _tagExtractor_StatusChanged;

            _folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
        }

        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                browseTextBox.Text = _folderBrowser.SelectedPath;
            }
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(browseTextBox.Text))
                {
                    string directory = browseTextBox.Text;
                    goButton.IsEnabled = false;
                    buttonCancel.IsEnabled = true;

                    if (radioButtonRepeated.IsChecked.GetValueOrDefault())
                    {
                        _currentThread = new Thread(() =>
                        {
                            List<HashedFile> repeatedFiles = _repeatedFilesFinder.GetRepeatedFiles(directory);
                            List<RepeatedFileModel> repeatedFileModels = new List<RepeatedFileModel>();

                            foreach (var repeatedFile in repeatedFiles)
                            {
                                foreach (var file in repeatedFile.FilesWithHash)
                                {
                                    repeatedFileModels.Add(new RepeatedFileModel() { Hash = repeatedFile.ComputedHash, Path = file });
                                }
                            }

                            Dispatcher.Invoke(new Action(() =>
                            {
                                dataGridRepeatedFiles.ItemsSource = repeatedFileModels;
                                goButton.IsEnabled = true;
                                buttonCancel.IsEnabled = false;
                            }));
                        });

                        _currentThread.Start();
                    }
                    else
                    {
                        _currentThread = new Thread(() =>
                        {
                            List<Tag> tagsInFileNames = _tagExtractor.GetTagsInFileNames(directory);

                            Dispatcher.Invoke(new Action(() =>
                            {
                                dataGridTags.ItemsSource = tagsInFileNames;
                                goButton.IsEnabled = true;
                                buttonCancel.IsEnabled = false;
                            }));
                        });

                        _currentThread.Start();
                    }
                }
                else
                {
                    MessageBox.Show("Pasta não informada.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void _repeatedFilesFinder_StatusChanged(string status)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                labelStatus.Content = status;
            }));
        }

        private System.Diagnostics.Process _previousExplorerProcess = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RepeatedFileModel model = ((FrameworkElement)sender).DataContext as RepeatedFileModel;

            if (model != null && System.IO.File.Exists(model.Path))
            {
                if(_previousExplorerProcess != null)
                {
                    if(!_previousExplorerProcess.HasExited)
                    {
                        //_previousExplorerProcess.Kill();   
                    }
                }

                //Open windows explorer
                _previousExplorerProcess = System.Diagnostics.Process.Start("explorer.exe", "/select, " + model.Path);
            }
        }

        private void radioButtonTag_Checked(object sender, RoutedEventArgs e)
        {
            if(radioButtonTag.IsChecked.GetValueOrDefault())
            {
                dataGridTags.Visibility = Visibility.Visible;
                dataGridRepeatedFiles.Visibility = Visibility.Collapsed;
            }
        }

        private void radioButtonRepeated_Checked(object sender, RoutedEventArgs e)
        {
            if (radioButtonRepeated.IsChecked.GetValueOrDefault())
            {
                dataGridRepeatedFiles.Visibility = Visibility.Visible;
                dataGridTags.Visibility = Visibility.Collapsed;
            }
        }

        private string _lastRepeatedHash = null;
        private SolidColorBrush _lastRepeatedFileColor = new SolidColorBrush(Colors.BlanchedAlmond);

        private void dataGridRepeatedFiles_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            RepeatedFileModel model = e.Row.DataContext as RepeatedFileModel;

            if (model != null)
            {
                if (model.Hash != _lastRepeatedHash)
                {
                    if (_lastRepeatedFileColor.Color == Colors.White)
                    {
                        _lastRepeatedFileColor = new SolidColorBrush(Colors.BlanchedAlmond);
                    }
                    else
                    {
                        _lastRepeatedFileColor = new SolidColorBrush(Colors.White);
                    }

                    _lastRepeatedHash = model.Hash;
                }

                e.Row.Background = _lastRepeatedFileColor;
            }
        }


        void _tagExtractor_StatusChanged(string status)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                labelStatus.Content = status;
            }));
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if(_currentThread != null)
            {
                _currentThread.Abort();
                goButton.IsEnabled = true;
                buttonCancel.IsEnabled = false;
            }
        }
    }
}
