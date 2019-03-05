using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using Argon.Model;
using Windows.Media.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Music : Page
    {
        ApplicationDataContainer local;
        public Music()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            local = ApplicationData.Current.LocalSettings;
            LoadAudios();
        }
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add("*");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Frame.Navigate(typeof(Player), file);
            }
        }

        private async void AddLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                string token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                int tmp = int.Parse(local.Values["CountFolder"].ToString());
                local.Values["CountFolder"] = tmp + 1;
                string foldnm = "folder" + local.Values["CountFolder"].ToString();
                local.Values[foldnm] = token;
            }
            LoadAudios();
        }
        public async void LoadAudios()
        {
            local.Values["lastState"] = "audio";
            SongList.Items.Clear();
            StorageFolder sf = KnownFolders.MusicLibrary;
            //StorageFolder sf = await DownloadsFolder.
            IReadOnlyList<StorageFile> fileList = await sf.GetFilesAsync();
            const ThumbnailMode thumbnailMode = ThumbnailMode.MusicView;
            foreach (StorageFile f in fileList)
            {
                const uint size = 100;
                using (StorageItemThumbnail thumbnail = await f.GetThumbnailAsync(thumbnailMode, size))
                {
                    // Also verify the type is ThumbnailType.Image (album art) instead of ThumbnailType.Icon 
                    // (which may be returned as a fallback if the file does not provide album art) 
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                        MediaFile o1 = new AudioFile();
                        Image i = new Image();
                        MusicProperties musicProperties = await f.Properties.GetMusicPropertiesAsync();
                        i.Source = bitmapImage;
                        o1.Thumb = i;
                        o1.Title = f.Name;
                        if (musicProperties.Title != "")
                        {
                            o1.Title = musicProperties.Title;
                        }
                        o1.Name = f.Name;
                        o1.Path = "MusicLibrary";
                        SongList.Items.Add(o1);
                    }
                }
            }
        }

        private async void SongList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Windows.Media.Playback.MediaPlaybackState i;
            if (mediaElement.MediaPlayer != null)
            {
                i = mediaElement.MediaPlayer.PlaybackSession.PlaybackState;
                if (i == Windows.Media.Playback.MediaPlaybackState.Playing)
                {

                }
            }
            StorageFile storageFile;
            StorageFolder storageFolder;
            MediaFile file = (MediaFile)e.ClickedItem;
            if (file.Path == "MusicLibrary")
            {
                storageFolder = KnownFolders.MusicLibrary;
                storageFile = await storageFolder.GetFileAsync(file.Name);
            }
            else
            {
                storageFile = await StorageFile.GetFileFromPathAsync(file.Path);
            }
            Console.WriteLine(storageFile.Path);
            mediaElement.Source = MediaSource.CreateFromStorageFile(storageFile);
            mediaElement.AutoPlay = true;
            mediaElement.MediaPlayer.Play();
        }

        private void MusicNavigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

        }
    }
}
