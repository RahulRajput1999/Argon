using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Argon.Model;
using Windows.Storage;
using Windows.UI.Core;
using Windows.Media.Core;
using Windows.System.Display;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AudioPlayer : Page
    {
        public AudioPlayer()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += Back_Pressed;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            mediaElement.Source = null;
            base.OnNavigatedTo(e);
            MediaFile file;
            StorageFile storageFile;
            StorageFolder storageFolder;
            if (e.Parameter.GetType() == typeof(MediaFile) || e.Parameter.GetType() == typeof(VideoFile) || e.Parameter.GetType() == typeof(AudioFile))
            {
                file = (MediaFile)e.Parameter;
                if (file.Path == "MusicLibrary")
                {
                    storageFolder = KnownFolders.MusicLibrary;
                    storageFile = await storageFolder.GetFileAsync(file.Name);
                }
                else if (file.Path == "VideoLibrary")
                {
                    storageFolder = KnownFolders.VideosLibrary;
                    storageFile = await storageFolder.GetFileAsync(file.Name);
                }
                else
                {
                    storageFile = await StorageFile.GetFileFromPathAsync(file.Path);
                }
                Console.WriteLine(storageFile.Path);


            }
            else
            {
                storageFile = (StorageFile)e.Parameter;
            }
            mediaElement.Source = MediaSource.CreateFromStorageFile(storageFile);
            mediaElement.AutoPlay = true;
            mediaElement.MediaPlayer.Play();

        }

        private void Back_Pressed(object sender, BackRequestedEventArgs e)
        {
            //appDisplayRequest.RequestRelease();
            //mediaElement.Source = null;
        }
    }
}
