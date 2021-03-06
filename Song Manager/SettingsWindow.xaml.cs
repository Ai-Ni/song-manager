﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;


namespace Song_Manager
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        string source_Audio_Directory, destination_Audio_Directory, img_Directory;
        bool IsRemoveSourceFile, IsWorkWithSourceFileOnly;
        Settings.SONG_NAME_STYLE STYLE;
        MainWindow mv;

        public SettingsWindow()
        {
            InitializeComponent();

            InitializeMainWindow(ref mv);
            InitializeElements();
        }

        private void InitializeMainWindow(ref MainWindow mw)
        {
            mv = (MainWindow)Application.Current.MainWindow;
            mv.settings.GetSettings(ref source_Audio_Directory, ref destination_Audio_Directory, ref img_Directory,
                ref IsRemoveSourceFile, ref IsWorkWithSourceFileOnly, ref STYLE);
        }

        private void InitializeElements()
        {
            tb_Source_Audio_Directory.IsReadOnly = true;
            tb_Destinaton_Audio_Directory.IsReadOnly = true;
            tb_Image_Directory.IsReadOnly = true;

            tb_Source_Audio_Directory.Text = source_Audio_Directory;
            tb_Destinaton_Audio_Directory.Text = destination_Audio_Directory;
            tb_Image_Directory.Text = img_Directory;
            cb_Remove.IsChecked = IsRemoveSourceFile;
            cb_Source.IsChecked = IsWorkWithSourceFileOnly;

            switch (STYLE)
            {
                case Settings.SONG_NAME_STYLE.HYPHEN: rb_style_hyphen.IsChecked = true; break;
                case Settings.SONG_NAME_STYLE.BRACKETS: rb_style_brackets.IsChecked = true; break;
            }
        }

        private string BrowseFolder()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;
        }

        private void btn_source_audio_directory_Click(object sender, RoutedEventArgs e)
        {
            string folder = BrowseFolder();
            tb_Source_Audio_Directory.Text = folder != null ? folder : tb_Source_Audio_Directory.Text;
            mv.settings.sourceAudioDirectory = folder != null ? folder : mv.settings.sourceAudioDirectory;
        }

        private void btn_destination_audio_directory_Click(object sender, RoutedEventArgs e)
        {
            string folder = BrowseFolder();
            tb_Destinaton_Audio_Directory.Text = folder != null ? folder : tb_Destinaton_Audio_Directory.Text;
            mv.settings.destinationAudioDirectory = folder != null ? folder : mv.settings.destinationAudioDirectory;
        }

        private void btn_image_directory_Click(object sender, RoutedEventArgs e)
        {
            string folder = BrowseFolder();
            tb_Image_Directory.Text = folder != null ? folder : tb_Image_Directory.Text;
            mv.settings.imgDirectory = folder != null ? folder : mv.settings.imgDirectory;
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            mv.settings.IsRemoveSourceFile = cb_Remove.IsChecked.Value;
            mv.settings.IsWorkWithSourceFileOnly = cb_Source.IsChecked.Value;
            mv.settings.STYLE = rb_style_hyphen.IsChecked.Value ? Settings.SONG_NAME_STYLE.HYPHEN : Settings.SONG_NAME_STYLE.BRACKETS;
            mv.settings.SetSettings();
            mv.UpdateDestinationDirView(mv.settings.IsWorkWithSourceFileOnly);
            Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
           Close();
        }
    }
}
