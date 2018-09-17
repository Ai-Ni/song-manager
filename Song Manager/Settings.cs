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
    [DataContract]
    class Settings
    {
        [DataMember]
        public string sourceAudioDirectory { get; set; }

        [DataMember]
        public string destinationAudioDirectory { get; set; }

        [DataMember]
        public string imgDirectory { get; set; }

        [DataMember]
        public bool IsRemoveSourceFile { get; set; }

        [DataMember]
        public bool IsWorkWithSourceFileOnly { get; set; }

        [DataMember]
        public SONG_NAME_STYLE STYLE { get; set; }

        public Settings()
        {

        }

        public Settings(string _sourceAudioDirectory, string _destinationAudioDirectory,
            string _imgDirectory, bool _IsRemveSourceFiles, bool _IsWorkWithSourceFileOnly, SONG_NAME_STYLE _STYLE)
        {
            sourceAudioDirectory = _sourceAudioDirectory;
            destinationAudioDirectory = _destinationAudioDirectory;
            imgDirectory = _imgDirectory;
            IsRemoveSourceFile = _IsRemveSourceFiles;
            IsWorkWithSourceFileOnly = _IsWorkWithSourceFileOnly;
            STYLE = _STYLE;
        }

        public void loadSettings()
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

        public void setSettings()
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

        public void getSettings(ref string _Source_Audio_Directory, ref string _Destination_Audio_Directory,
            ref string _Img_Directory, ref bool _IsRemoveSourceFile, ref bool _IsWorkWithSourceFileOnly,
            ref SONG_NAME_STYLE _STYLE)
        {
            _Source_Audio_Directory = sourceAudioDirectory;
            _Destination_Audio_Directory = destinationAudioDirectory;
            _Img_Directory = imgDirectory;
            _IsRemoveSourceFile = IsRemoveSourceFile;
            _IsWorkWithSourceFileOnly = IsWorkWithSourceFileOnly;
            _STYLE = STYLE;
        }

        public enum SONG_NAME_STYLE
        {
            HYPHEN,
            BRACKETS
        }
    }
}
