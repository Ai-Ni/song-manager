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
            settings.LoadSettings();
            tb_display_progress.Text = null;

            try
            {
                UpdateDestinationDirView(settings.IsWorkWithSourceFileOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show("OOPS! " + ex.Message);
            }
        }

        /// <summary>
        /// Control function.
        /// </summary>
        /// <param name="fileList">List with mp3 files for prociding.</param>
        /// <param name="progress">Value for progressbar.</param>
        /// <param name="prgrss">Value for label.</param>
        /// <returns></returns>
        private async Task<int> ProceedFileAsync(string[] fileList, IProgress<int> progress, IProgress<int> prgrss)
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
                
                var i = 0;
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
                        ProceedFile(file);
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
                        continue;
                    }

                    catch (Exception ex)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            lb_log.Items.Add("Failed! Reason: " + ex.Message);
                        });
                        failedCount++;
                        progress.Report(((successCount + failedCount) * 100 / totalCount));
                        prgrss.Report(((successCount + failedCount) * 100 / totalCount));
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

        /// <summary>
        /// Fills tags, creates copy (if required)
        /// </summary>
        /// <param name="file">Full path to mp3 file.</param>
        private void ProceedFile(string file)
        {
            string songName = null, performer = null, fullName;
            
            DetermineSongName(file, settings.STYLE, ref performer, ref songName);

            if (!settings.IsWorkWithSourceFileOnly)
            {
                fullName = performer + " - " + songName + ".mp3";
                System.IO.File.Copy(file, settings.destinationAudioDirectory + @"\" + fullName, true);

                if (settings.IsRemoveSourceFile)
                    System.IO.File.Delete(file);

                try
                {
                    EditTags(settings.destinationAudioDirectory + @"\" + fullName, performer, songName, GetPICTURE_ACTIONS(), null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return;
            }

            try
            {
                EditTags(file, performer, songName, GetPICTURE_ACTIONS(), null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Returns song; name and performers via ref variables.
        /// </summary>
        /// <param name="fileName">Full path to mp3 file.</param>
        /// <param name="STYLE">Songs name style.</param>
        /// <param name="performer">Performer.</param>
        /// <param name="song">Songs name.</param>
        private void DetermineSongName(string fileName, Settings.SONG_NAME_STYLE STYLE, ref string performer, ref string song)
        {
            switch(STYLE)
            {
                case Settings.SONG_NAME_STYLE.HYPHEN:
                    if (!fileName.Contains("-"))
                        throw new Exception("There is no hyphen in file name");

                    string[] temp = fileName.Remove(0, fileName.LastIndexOf(@"\") + 1).Split('-');
                    performer = temp[0].Remove(temp[0].Length - 1, 1);
                    song = temp[1].Remove(0, 1);
                    break;

                case Settings.SONG_NAME_STYLE.BRACKETS:

                int start = fileName.IndexOf("《");
                int end = fileName.IndexOf("》");

                if (end <= 0)
                    throw new Exception("There are no brackets in file name");

                song = fileName.Substring(start + 1, end - start - 1);
                performer = "Feng Timo";
                    break;
               
            }
        }

        /// <summary>
        /// Fills songs tags.
        /// </summary>
        /// <param name="path">Full path to mp3 file.</param>
        /// <param name="performer">Performer.</param>
        /// <param name="songName">Songs name.</param>
        /// <param name="ACTION">Action with image - none, set random or upload.</param>
        /// <param name="imagePath">Full path to image file(if set upload action).</param>
        public void EditTags(string path, string performer, string songName, PICTURE_ACTIONS ACTION, string imagePath=null)
        {
            TagLib.File file = TagLib.File.Create(path);
            file.Tag.Performers = new string[1] { performer };
            file.Tag.Title = songName;

            switch (ACTION)
            {
                case PICTURE_ACTIONS.NONE:break;
                case PICTURE_ACTIONS.RANDOM: file.Tag.Pictures = new Picture[] { new Picture(GetRandomImage()) }; break;
                case PICTURE_ACTIONS.UPLOAD: file.Tag.Pictures = new Picture[] { new Picture(imagePath) }; break;
            }
            file.Save();
        }

        /// <summary>
        /// Returns random image from folder.
        /// </summary>
        /// <returns></returns>
        public string GetRandomImage()
        {
            string[] fileList = GetFileListByType(settings.imgDirectory, FILETYPE.IMAGE);
            Random r = new Random();

            return fileList[r.Next(0, fileList.Length)];
        }

        /// <summary>
        /// Returns list of certain files in the directory.
        /// </summary>
        /// <param name="directory">Full path to folder with files.</param>
        /// <param name="ft">Type of file to search.</param>
        /// <returns></returns>
        public string[] GetFileListByType(string directory, FILETYPE ft)
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

        /// <summary>
        /// Loads output files list to listbox.
        /// </summary>
        /// <param name="IsWorkWithSourceFileOnly"></param>
        public void UpdateDestinationDirView(bool IsWorkWithSourceFileOnly = false)
        {
            lb_dest_dir.Items.Clear();
            string[] fileList;

            try
            {
                if (IsWorkWithSourceFileOnly)
                {
                    fileList = GetFileListByType(settings.sourceAudioDirectory, FILETYPE.SOUND);
                }

                else
                {
                    fileList = GetFileListByType(settings.destinationAudioDirectory, FILETYPE.SOUND);
                }

                if (fileList.Length > 0)
                {
                    foreach (string file in fileList)
                        lb_dest_dir.Items.Add(file.Remove(0, file.LastIndexOf(@"\") + 1));
                    lb_dest_dir.SelectedIndex = 0;
                    DisplayTags(IsWorkWithSourceFileOnly);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("It's impossible to update song list. Reason: " + ex.Message);
            }
        }

        /// <summary>
        /// Displays tags(peroformer, song, image).
        /// </summary>
        /// <param name="IsWorkWithSourceFileOnly"></param>
        private void DisplayTags(bool IsWorkWithSourceFileOnly = false)
        {
            TagLib.File file;
            try
            {
                if (IsWorkWithSourceFileOnly)
                {
                    file = TagLib.File.Create(settings.sourceAudioDirectory +
                       @"\" + lb_dest_dir.SelectedItem.ToString());
                }

                else
                {
                    file = TagLib.File.Create(settings.destinationAudioDirectory +
                       @"\" + lb_dest_dir.SelectedItem.ToString());
                }

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
                int done = await ProceedFileAsync(GetFileListByType(settings.sourceAudioDirectory, FILETYPE.SOUND),
                    new Progress<int>(percent => prgrsbar_procidingProcess.Value = percent),
                    new Progress<int>(percent => tb_display_progress.Text = percent + "%"));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Prociding process had failed! Reason: " + ex.Message);
            }
            UpdateDestinationDirView(settings.IsWorkWithSourceFileOnly);
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
                DisplayTags(settings.IsWorkWithSourceFileOnly);
            }
        }

        private void btn_save_tags_Click(object sender, RoutedEventArgs e)
        {
            if (settings.IsWorkWithSourceFileOnly)
                EditTags(settings.sourceAudioDirectory + @"\" + lb_dest_dir.SelectedItem.ToString(),
              tb_performer.Text, tb_song.Text, GetPICTURE_ACTIONS(), tag_img_path);
            else
                EditTags(settings.destinationAudioDirectory + @"\" + lb_dest_dir.SelectedItem.ToString(),
              tb_performer.Text, tb_song.Text, GetPICTURE_ACTIONS(), tag_img_path);
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
            DisplayTags(settings.IsWorkWithSourceFileOnly);
        }

        private PICTURE_ACTIONS GetPICTURE_ACTIONS()
        {
            PICTURE_ACTIONS STATUS = PICTURE_ACTIONS.NONE;

            this.Dispatcher.Invoke(() =>
            {
                if (rb_pic_rng.IsChecked.Value)
                    STATUS = PICTURE_ACTIONS.RANDOM;

                if (rb_pic_upload.IsChecked.Value)
                    STATUS = PICTURE_ACTIONS.UPLOAD;
            });

            return STATUS;
        }

        public enum PICTURE_ACTIONS
        {
            NONE,
            RANDOM,
            UPLOAD
        }
    }
}
