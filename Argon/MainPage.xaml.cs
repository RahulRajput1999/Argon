using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Argon.Model;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer local;
        public MainPage()
        {
            this.InitializeComponent();
            local = ApplicationData.Current.LocalSettings;
            if (local.Values["CountFolder"] == null)
                local.Values["CountFolder"] = 0;
            if (local.Values["lastState"] == null)
            {
                local.Values["lastState"] = "video";
                LoadVideos();
            }
            else
            {
                if((String)local.Values["lastState"] == "video")
                {
                    NavigationPanel.SelectedItem = NavigationPanel.MenuItems[0];
                    //LoadVideos();
                }
                else
                {
                    NavigationPanel.SelectedItem = NavigationPanel.MenuItems[1];
                    //LoadAudios();
                }
            }
        }

        public async void LoadVideos()
        {
            local.Values["lastState"] = "video";
            FileHolder.Items.Clear();
            StorageFolder sf = KnownFolders.VideosLibrary;
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
                        MediaFile o1 = new VideoFile();
                        VideoProperties videoProperties = await f.Properties.GetVideoPropertiesAsync();
                        Image i = new Image();
                        i.Source = bitmapImage;
                        o1.Thumb = i;
                        o1.Title = f.Name;
                        if (videoProperties.Title != "")
                        {
                            o1.Title = videoProperties.Title;
                        }
                        o1.Name = f.Name;
                        o1.Path = "VideoLibrary";
                        FileHolder.Items.Add(o1);
                    }
                }
            }
        }
        public async void LoadAudios()
        {
            local.Values["lastState"] = "audio";
            FileHolder.Items.Clear();
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
                        FileHolder.Items.Add(o1);
                    }
                }
            }
        }
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VideoLibrary_Click(object sender, RoutedEventArgs e)
        {
            LoadVideos();
        }

        private void AudioLibrary_Click(object sender, RoutedEventArgs e)
        {
            LoadAudios();
        }

        private void FileHolder_ItemClick(object sender, ItemClickEventArgs e)
        {
            MediaFile file = (MediaFile)e.ClickedItem;
            Frame.Navigate(typeof(Player),file);
            
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SideNavEx));
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            string item = ((NavigationViewItem)args.SelectedItem).Tag.ToString();
            if(item == "1")
            {
                LoadVideos();
            }
            else if(item == "2")
            {
                LoadAudios();
            }
        }
    }

}
