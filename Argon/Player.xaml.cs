using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Player : Page
    {
        private DisplayRequest appDisplayRequest = null;
        public Player()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += Back_Pressed;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            appDisplayRequest = new DisplayRequest();
            appDisplayRequest.RequestActive();
            base.OnNavigatedTo(e);
            MediaFile file;
            StorageFile storageFile;
            StorageFolder storageFolder;
            if (e.Parameter.GetType() == typeof(MediaFile) || e.Parameter.GetType() == typeof(VideoFile) || e.Parameter.GetType() == typeof(AudioFile))
            {
                file = (MediaFile)e.Parameter;
                storageFile = await StorageFile.GetFileFromPathAsync(file.Path);
                Console.WriteLine(storageFile.Path);
            }
            else
            {
                storageFile = (StorageFile)e.Parameter;
            }
            bool flag = false;
            TimedTextSource timedsource = null;
            storageFolder =  await storageFile.GetParentAsync();
            string subtitlename = storageFile.Name.Substring(0, storageFile.Name.LastIndexOf('.')) + ".srt";
            try
            {
                StorageFile subtitle = await storageFolder.GetFileAsync(subtitlename);
                var stream = await subtitle.OpenAsync(FileAccessMode.Read);
                timedsource = TimedTextSource.CreateFromStream(stream);
                flag = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            var source = MediaSource.CreateFromStorageFile(storageFile);
            if (flag)
                source.ExternalTimedTextSources.Add(timedsource);

            mediaElement.Source = source;
            
            mediaElement.AutoPlay = true;
            mediaElement.MediaPlayer.Play();

        }

        private void Back_Pressed(object sender, BackRequestedEventArgs e) 
        {
            //appDisplayRequest.RequestRelease();
            mediaElement.Source = null;
        }

    }
}
