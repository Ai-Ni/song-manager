using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Song_Manager
{
    /// <summary>
    /// Class with apps settings.
    /// </summary>
    [DataContract]
    class Settings
    {
        /// <summary>
        /// Path to files for prociding.
        /// </summary>
        [DataMember]
        public string sourceAudioDirectory { get; set; }

        /// <summary>
        /// Output path.
        /// </summary>
        [DataMember]
        public string destinationAudioDirectory { get; set; }

        /// <summary>
        /// Path to folder with images.
        /// </summary>
        [DataMember]
        public string imgDirectory { get; set; }

        /// <summary>
        /// Remove source file after prociding
        /// </summary>
        [DataMember]
        public bool IsRemoveSourceFile { get; set; }

        /// <summary>
        /// Make changes in source files without creating new files.
        /// </summary>
        [DataMember]
        public bool IsWorkWithSourceFileOnly { get; set; }

        /// <summary>
        /// Style of the song naming - brackets, hyphen
        /// </summary>
        [DataMember]
        public SONG_NAME_STYLE STYLE { get; set; }

        public Settings() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceAudioDirectory"></param>
        /// <param name="destinationAudioDirectory"></param>
        /// <param name="imgDirectory"></param>
        /// <param name="IsRemoveSourceFile"></param>
        /// <param name="IsWorkWithSourceFileOnly"></param>
        /// <param name="STYLE"></param>
        public Settings(string sourceAudioDirectory, string destinationAudioDirectory,
            string imgDirectory, bool IsRemoveSourceFile, bool IsWorkWithSourceFileOnly, SONG_NAME_STYLE STYLE)
        {
            this.sourceAudioDirectory = sourceAudioDirectory;
            this.destinationAudioDirectory = destinationAudioDirectory;
            this.imgDirectory = imgDirectory;
            this.IsRemoveSourceFile = IsRemoveSourceFile;
            this.IsWorkWithSourceFileOnly = IsWorkWithSourceFileOnly;
            this.STYLE = STYLE;
        }

        /// <summary>
        /// Loads settings from json file
        /// </summary>
        public void LoadSettings()
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Settings));
            try
            {
                using (FileStream fs = new FileStream("settings.json", FileMode.Open))
                {
                    Settings settings = (Settings)jsonFormatter.ReadObject(fs);

                    sourceAudioDirectory = settings.sourceAudioDirectory;
                    destinationAudioDirectory = settings.destinationAudioDirectory;
                    imgDirectory = settings.imgDirectory;
                    IsRemoveSourceFile = settings.IsRemoveSourceFile;
                    IsWorkWithSourceFileOnly = settings.IsWorkWithSourceFileOnly;
                    STYLE = settings.STYLE;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't read settings from file. Reason: " + ex.Message);
            }
        }

        /// <summary>
        /// Save settings to json file.
        /// </summary>
        public void SetSettings()
        {
            try
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Settings));

                using (FileStream fs = new FileStream("settings.json", FileMode.Create))
                {
                    jsonFormatter.WriteObject(fs, this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't save settings to file. Reason: " + ex.Message);

            }
        }

        /// <summary>
        /// Returns all settings.
        /// </summary>
        /// <param name="_Source_Audio_Directory"></param>
        /// <param name="_Destination_Audio_Directory"></param>
        /// <param name="_Img_Directory"></param>
        /// <param name="_IsRemoveSourceFile"></param>
        /// <param name="_IsWorkWithSourceFileOnly"></param>
        /// <param name="_STYLE"></param>
        public void GetSettings(ref string sourceAudioDirectory, ref string destinationAudioDirectory,
            ref string imgDirectory, ref bool IsRemoveSourceFile, ref bool IsWorkWithSourceFileOnly,
            ref SONG_NAME_STYLE STYLE)
        {
            sourceAudioDirectory = this.sourceAudioDirectory;
            destinationAudioDirectory = this.destinationAudioDirectory;
            imgDirectory = this.imgDirectory;
            IsRemoveSourceFile = this.IsRemoveSourceFile;
            IsWorkWithSourceFileOnly = this.IsWorkWithSourceFileOnly;
            STYLE = this.STYLE;
        }

        public enum SONG_NAME_STYLE
        {
            HYPHEN,
            BRACKETS
        }
    }
}
