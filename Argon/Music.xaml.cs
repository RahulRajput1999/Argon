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
        List<string> musicFormat = new List<string>() { ".mp3", ".wav"};
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
            foreach(var i in musicFormat)
                picker.FileTypeFilter.Add(i);
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                mediaElement.Source = MediaSource.CreateFromStorageFile(file);
                mediaElement.AutoPlay = true;
                mediaElement.MediaPlayer.Play();
            }
        }

        private async void AddLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            foreach(var i in musicFormat)
                picker.FileTypeFilter.Add(i);
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                string token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                int tmp = int.Parse(local.Values["CountMusic"].ToString());
                local.Values["CountMusic"] = tmp + 1;
                string foldnm = "music" + local.Values["CountMusic"].ToString();
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
                if (musicFormat.FindIndex(x => x.Equals(f.FileType, StringComparison.OrdinalIgnoreCase)) != -1)
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

            int count = int.Parse(local.Values["CountMusic"].ToString());
            for (int i = 1; i <= count; i++)
            {
                string foldnm = "music" + i.ToString();
                string token = local.Values[foldnm].ToString();
                sf = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                fileList = await sf.GetFilesAsync();
                foreach (StorageFile f in fileList)
                {
                    if (musicFormat.FindIndex(x => x.Equals(f.FileType, StringComparison.OrdinalIgnoreCase)) != -1)
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
                                MediaFile o1 = new VideoFile();
                                VideoProperties videoProperties = await f.Properties.GetVideoPropertiesAsync();
                                Image i1 = new Image();
                                i1.Source = bitmapImage;
                                o1.Thumb = i1;
                                o1.Title = f.Name;
                                if (videoProperties.Title != "")
                                {
                                    o1.Title = videoProperties.Title;
                                }
                                o1.Name = f.Name;
                                o1.Path = f.Path;
                                SongList.Items.Add(o1);
                            }
                        }
                    }
                }
            }
        }

        private async void SongList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Windows.Media.Playback.MediaPlaybackState i;
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
