using FileScanner.Commands;
using FileScanner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace FileScanner.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string selectedFolder;
        private ObservableCollection<string> folderItems = new ObservableCollection<string>();
        private ObservableCollection<Items> items = new ObservableCollection<Items>();
         
        public DelegateCommand<string> OpenFolderCommand { get; private set; }
        public DelegateCommand<string> ScanFolderCommand { get; private set; }

        public DelegateCommand<string> ScanFolderAsyncCommand { get; private set; }

        public ObservableCollection<string> FolderItems { 
            get => folderItems;
            set 
            { 
                folderItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Items> Items
        {
            get => items;
            set
            {
                items = value;
                OnPropertyChanged();
            }
        }

        public string SelectedFolder
        {
            get => selectedFolder;
            set
            {
                selectedFolder = value;
                OnPropertyChanged();
                ScanFolderCommand.RaiseCanExecuteChanged();
                ScanFolderAsyncCommand.RaiseCanExecuteChanged();
            }
        }

        public MainViewModel()
        {
            OpenFolderCommand = new DelegateCommand<string>(OpenFolder);
            ScanFolderCommand = new DelegateCommand<string>(ScanFolder, CanExecuteScanFolder);
            ScanFolderAsyncCommand = new DelegateCommand<string>(ScanFolderAsync, CanExecuteScanFolder);
        }

        private bool CanExecuteScanFolder(string obj)
        {
            return !string.IsNullOrEmpty(SelectedFolder);
        }

        private void OpenFolder(string obj)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    SelectedFolder = fbd.SelectedPath;
                }
            }
        }

        private void ScanFolder(string dir) 
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            try 
            {
                FolderItems = new ObservableCollection<string>(GetDirs(dir));

                foreach (var item in Directory.EnumerateFiles(dir, "*"))
                {
                    Items temp = new Items() { Item = item, Image = "/Images/file.png", Image2 = "/Images/folder.bmp" };

                    Items.Add(temp);
                }
            }
            catch(UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied to one of the scanned directories. Select another directories to scan.");
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            MessageBox.Show($"Total execution time : {elapsedMs}");
            
        }
        private async void ScanFolderAsync(string dir)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            await Task.Run(() =>
            {
                try
                {
                    FolderItems = new ObservableCollection<string>(GetDirs(dir));

                    foreach (var item in Directory.EnumerateFiles(dir, "*"))
                    {
                        Items temp = new Items() { Item = item, Image = "/Images/file.png", Image2 = "/Images/folder.bmp" };

                        App.Current.Dispatcher.BeginInvoke(
                            (Action)delegate ()
                            {
                                Items.Add(temp);
                            });
                        
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access denied to one of the scanned directories. Select another directories to scan.");
                }
            });
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            MessageBox.Show($"Total execution time : {elapsedMs}");

        }
        IEnumerable<string> GetDirs(string dir)
        {
            foreach (var d in Directory.EnumerateDirectories(dir, "*"))
            {
                yield return d;

                foreach (var f in Directory.EnumerateFiles(d, "*"))
                {
                    yield return f;
                }
            }
        }

        ///TODO : Tester avec un dossier avec beaucoup de fichier
        ///TODO : Rendre l'application asynchrone
        ///TODO : Ajouter un try/catch pour les dossiers sans permission
    }
}
