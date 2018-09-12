using Microsoft.WindowsAPICodePack.Dialogs;
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
using TagLib;


namespace Song_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal Settings settings { get; set; }
        string tag_img_path;
        public MainWindow()
        {
            InitializeComponent();

            settings = new Settings();
            settings.loadSettings();
            tb_display_progress.Text = null;

            try
            {
                UpdateDestinationDirView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("OOPS! " + ex.Message);
            }
        }

        private async Task<int> RenameFilesAsync(string[] fileList, IProgress<int> progress, IProgress<int> prgrss)
        {
            int successCount = 0, failedCount = 0;
            int totalCount = fileList.Length;

            int processCount = await Task.Run<int>(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    lb_log.Items.Clear();
                    lb_log.Items.Add("Started prociding of " + fileList.Length.ToString() + " audio files:");
                });
                
                Int32 i = 0;
                foreach (string file in fileList)
                {
                    i++;
                    this.Dispatcher.Invoke(() =>
                    {
                        lb_log.Items.Add("\nFile [" + i.ToString() + "/" + fileList.Length.ToString()
                        + "]: " + file + "\nProciding...");
                    });

                    try
                    {
                        renameFile(file);
                    }
                    catch (ArgumentNullException ex)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            lb_log.Items.Add("Failed! Reason: " + ex.Message + "\nFile name might be incorrect.");
                        });
                        failedCount++;
                        progress.Report(((successCount + failedCount) * 100 / totalCount));
                        prgrss.Report(((successCount + failedCount) * 100 / totalCount));
                       // MessageBox.Show(ex.Message);
                        continue;
                    }

                    catch (IOException ex)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            lb_log.Items.Add("Failed! Reason: " + ex.Message);
                        });
                        failedCount++;
                        progress.Report(((successCount + failedCount) * 100 / totalCount));
                        prgrss.Report(((successCount + failedCount) * 100 / totalCount));
                        MessageBox.Show(ex.Message);
                        continue;
                    }

                    if (progress != null)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            string t = lb_log.Items[lb_log.Items.Count-1].ToString() + " Successfull!";
                            lb_log.Items.RemoveAt(lb_log.Items.Count - 1);
                            lb_log.Items.Add(t);
                        });
                        successCount++;
                        progress.Report(((successCount + failedCount) * 100 / totalCount));
                        prgrss.Report(((successCount + failedCount) * 100 / totalCount));
                    }

                }

                return successCount + failedCount;

            });

            MessageBox.Show("Done!\nTotal success = " + successCount.ToString() + "\nTotal fails = " + failedCount.ToString());

            tb_display_progress.Text = null;
            prgrsbar_procidingProcess.Value = 0;

            return processCount;
        }

        public void renameFile(string file)
        {
            string songName, fullName;
            songName = DetermineSongName(file);

            fullName = "Feng Timo - " + songName + ".mp3";
            System.IO.File.Copy(file, settings.destinationAudioDirectory + @"\" + fullName, true);

            if (settings.IsRemoveSourceFile)
                System.IO.File.Delete(file);

            EditTags(settings.destinationAudioDirectory + @"\" + fullName, "Feng Timo", songName, null);
        }

        public string DetermineSongName(string fileName)
        {
            int start = fileName.IndexOf("《");
            int end = fileName.IndexOf("》");

            return end <= 0 || !fileName.Contains(".mp3") ? throw new ArgumentNullException() : fileName.Substring(start + 1, end - start - 1);
        }

        public void EditTags(string path, string performer, string songName, string imagePath)
        {
            TagLib.File file = TagLib.File.Create(path);
            file.Tag.Performers = new string[1] { performer };
            file.Tag.Title = songName;
            file.Tag.Pictures = new Picture[] { new Picture(imagePath == null ? GetRandomImage() : imagePath) };

            file.Save();
        }

        public string GetRandomImage()
        {
            string[] fileList = GetFilesListByType(settings.imgDirectory, FILETYPE.IMAGE);
            Random r = new Random();

            return fileList[r.Next(0, fileList.Length)];
        }

        public string[] GetFilesListByType(string directory, FILETYPE ft)
        {
            string[] fileList = null;
            try
            {
                fileList = Directory.GetFiles(directory);
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception("One or few directories names are incorrect!");
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new Exception("One or few directories names are incorrect!");
            }
            List<string> tempList = new List<string>(fileList);
            List<string> bufferList = new List<string>();

            foreach (string elem in tempList)
                bufferList.Add(elem);

            switch (ft)
            {
                case FILETYPE.SOUND:
                    {
                        foreach (string elem in tempList)
                            if (!elem.Contains(".mp3"))
                                bufferList.Remove(elem);
                        break;
                    }

                case FILETYPE.IMAGE:
                    {
                        foreach (string elem in tempList)
                            if (!elem.Contains(".jpg") && !elem.Contains(".png") && !elem.Contains(".jpeg"))
                                bufferList.Remove(elem);
                        break;
                    }

                default: break;
            }

            return bufferList.ToArray();
        }

        public void UpdateDestinationDirView()
        {
            lb_dest_dir.Items.Clear();
            string[] fileList = GetFilesListByType(settings.destinationAudioDirectory, FILETYPE.SOUND);
            if (fileList.Length > 0)
            {
                foreach (string file in fileList)
                    lb_dest_dir.Items.Add(file.Remove(0, file.LastIndexOf(@"\") + 1));
                lb_dest_dir.SelectedIndex = 0;
                DisplayTags();
            }
        }

        private void DisplayTags()
        {
            TagLib.File file;
            try
            {
                file = TagLib.File.Create(settings.destinationAudioDirectory +
                   @"\" + lb_dest_dir.SelectedItem.ToString());

                tb_performer.Text = file.Tag.Performers[0];
                tb_song.Text = file.Tag.Title;

                MemoryStream ms = new MemoryStream(file.Tag.Pictures[0].Data.Data);

                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                img_tag_image.Source = bitmap;
            }
            catch (NullReferenceException ex)
            {
                return;
            }
            catch (FileNotFoundException ex)
            {
                return;
            }
            catch (IndexOutOfRangeException ex)
            {
                return;
            }
        }

        private async void btn_execute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int done = await RenameFilesAsync(GetFilesListByType(settings.sourceAudioDirectory, FILETYPE.SOUND),
                    new Progress<int>(percent => prgrsbar_procidingProcess.Value = percent),
                    new Progress<int>(percent => tb_display_progress.Text = percent + "%"));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Prociding process had failed! Reason: " + ex.Message);
            }
            UpdateDestinationDirView();
        }

        private async Task<Int32> TagAll(IProgress<int> progress, IProgress<int> prgrss)
        {
            string[] fileList = Directory.GetFiles(settings.sourceAudioDirectory); 
            string _performer, _song, full;
            Int32 count = fileList.Length;
            int processCount = await Task.Run<int>(() =>
            {
                Int32 i = 0;
                foreach (string file in fileList)
                {
                    i++;
                    _song = file.Split('-')[1].Remove(0, 1).Split('.')[0];
                    _performer = file.Split('-')[0];
                    _performer = _performer.Remove(_performer.Length - 1).Remove(0, _performer.LastIndexOf('\\') + 1);
                    full = _performer + " - " + _song + ".mp3";
                    System.IO.File.Copy(file, settings.destinationAudioDirectory + @"\" + full, true);

                    TagLib.File _file = TagLib.File.Create(settings.destinationAudioDirectory + @"\" + full);
                    _file.Tag.Performers = new string[1] { _performer };
                    _file.Tag.Title = _song;


                    _file.Save();


                    if (progress != null)
                    {
                        progress.Report(i * 100 / count);
                        prgrss.Report(i * 100 / count);
                    }
                    
                }
                return 0;
            });
            return 0;
        }

        private void btn_Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            Window w = new Window();
            sw.Owner = this;
            sw.ShowDialog();
        }

        private void lb_dest_dir_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString().Equals("Up") || e.Key.ToString().Equals("Down"))
            {
                DisplayTags();
            }
        }

        private void btn_save_tags_Click(object sender, RoutedEventArgs e)
        {
            EditTags(settings.destinationAudioDirectory + @"\" + lb_dest_dir.SelectedItem.ToString(),
              tb_performer.Text, tb_song.Text, tag_img_path);
        }

        private void img_tag_image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.Filters.Add(
                new CommonFileDialogFilter("Image files","*.jpg, *.jpeg, *.jpe, *.jfif, *.png"));
            tag_img_path = dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;

            if (tag_img_path==null)
                return;

            img_tag_image.Source = new BitmapImage(new Uri(tag_img_path));
        }

        private void lb_dest_dir_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DisplayTags();
        }
    }
}
