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
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Videos : Page
    {
        ApplicationDataContainer local;
        List<string> videoFormat = new List<string>() { ".mov", ".mp4", ".mkv" };
        public Videos()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            local = ApplicationData.Current.LocalSettings;
            LoadVideos();
            
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            foreach(var i in videoFormat)
                picker.FileTypeFilter.Add(i);
            StorageFile file = await picker.PickSingleFileAsync();
            var page = grid.Parent as Videos;
            var parent = page.Parent as Frame;
            var superNav = parent.Parent as NavigationView;
            var superGrid = superNav.Parent as Grid;
            var superPage = superGrid.Parent as MainPage;
            var superParent = superPage.Parent as Frame;
            if (file != null && superParent != null)
            {
                superParent.Navigate(typeof(Player), file);
            }
        }

        private async void AddLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            foreach(var i in videoFormat)
                picker.FileTypeFilter.Add(i);
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                string token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                int tmp = int.Parse(local.Values["CountVideo"].ToString());
                local.Values["CountVideo"] = tmp + 1;
                string foldnm = "video" + local.Values["CountVideo"].ToString();
                local.Values[foldnm] = token;
            }
            LoadVideos();
        }

        private void FileHolder_ItemClick(object sender, ItemClickEventArgs e)
        {
            MediaFile file = (MediaFile)e.ClickedItem;
            var page = grid.Parent as Videos;
            var parent = page.Parent as Frame;
            var superNav = parent.Parent as NavigationView;
            var superGrid = superNav.Parent as Grid;
            var superPage = superGrid.Parent as MainPage;
            var superParent = superPage.Parent as Frame;
            if (superParent != null)
            {
                superParent.Navigate(typeof(Player), file);
            }
        }

        public async Task LoadFromFolder(StorageFolder storageFolder)
        {
            IReadOnlyList<StorageFile> fileList = await storageFolder.GetFilesAsync();
            const ThumbnailMode thumbnailMode = ThumbnailMode.MusicView;
            foreach (StorageFile f in fileList)
            {
                if (videoFormat.FindIndex(x => x.Equals(f.FileType, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    const uint size = 100;
                    using (StorageItemThumbnail thumbnail = await f.GetThumbnailAsync(thumbnailMode, size))
                    {
                        // Also verify the type is ThumbnailType.Image (album art) instead of ThumbnailType.Icon 
                        // (which may be returned as a fallback if the file does not provide album art) 
                        if (thumbnail != null && (thumbnail.Type == ThumbnailType.Image || thumbnail.Type == ThumbnailType.Icon))
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
                                o1.Title = videoProperties.Title;
                            o1.Name = f.Name;
                            o1.Path = f.Path;
                            FileHolder.Items.Add(o1);
                        }
                    }
                }
            }
            IReadOnlyList<StorageFolder> folderList = await storageFolder.GetFoldersAsync();
            foreach(var i in folderList)
            {
                await LoadFromFolder(i);
            }
        }

        public async void LoadVideos()
        {
            local.Values["lastState"] = "video";
            FileHolder.Items.Clear();
            StorageFolder sf = KnownFolders.VideosLibrary;
            await LoadFromFolder(sf);

            int count = int.Parse(local.Values["CountVideo"].ToString());
            for (int i = 1; i <= count; i++)
            {
                string foldnm = "video" + i.ToString();
                string token = local.Values[foldnm].ToString();
                sf = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                await LoadFromFolder(sf);
            }
        }
    }
}
