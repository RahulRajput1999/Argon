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
using System.Diagnostics;
using SubLib;
using System.Collections.ObjectModel;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    class Comparator : IComparer<VideoFile>
    {
        public int Compare(VideoFile x, VideoFile y)
        {
            if(x==null || y == null)
            {
                return 0;
            }
            return x.Title.CompareTo(y.Title);
        }
    }
    class SubtitleComparator : IComparer<SubInfo>
    {
        public int Compare(SubInfo x, SubInfo y)
        {
            if (x == null || y == null)
            {
                return 0;
            }
            return x.LanguageName.CompareTo(y.LanguageName);
        }
    }
    public sealed partial class Videos : Page
    {
        ApplicationDataContainer local;
        List<string> videoFormat = new List<string>() { ".mov", ".mp4", ".mkv" };
        List<string> autoList = new List<string>();
        List<VideoFile> videoFiles = new List<VideoFile>();
        private const int MaxAttempts = 10;

        private OSIntermediary messenger = new OSIntermediary();
        private string currentPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
        private List<string> languages = new List<string>();
        private bool isConfigRead = false;

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
            if (file != null)
            {
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
                LoadVideos();
            }
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
                            videoFiles.Add((VideoFile)o1);
                            autoList.Add(o1.Title);
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
            Comparator c = new Comparator();
            Debug.WriteLine("\n\nBefore Sorting the list.\n\n");
            videoFiles.Sort(c);
            Debug.WriteLine("\n\nAfter sorting the list and before adding to file holder");
            foreach (VideoFile vf in videoFiles)
            {
                FileHolder.Items.Add(vf);
                Debug.WriteLine("Adding : " + vf.Title);
            }
            Debug.WriteLine("Done");
            VideoProgressRing.IsActive = false;
        }

        private void FileHolder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Image img = (Image)sender;
            VideoBackGround.Source = img.Source;
        }

        private void VideoSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var Auto = (AutoSuggestBox)sender;
            var Suggestion = autoList.Where(p => p.Contains(Auto.Text, StringComparison.OrdinalIgnoreCase)).ToArray();
            Auto.ItemsSource = Suggestion;
        }

        private void VideoSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var Auto = (AutoSuggestBox)sender;
            var Suggestion = autoList.Where(p => p.Contains(Auto.Text, StringComparison.OrdinalIgnoreCase)).ToArray();
            Auto.ItemsSource = Suggestion;
        }

        private void VideoSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string item = (string)args.SelectedItem;
            var vf = videoFiles.Where(s => s.Title == item).FirstOrDefault();
            var page = grid.Parent as Videos;
            var parent = page.Parent as Frame;
            var superNav = parent.Parent as NavigationView;
            var superGrid = superNav.Parent as Grid;
            var superPage = superGrid.Parent as MainPage;
            var superParent = superPage.Parent as Frame;
            if (superParent != null)
            {
                superParent.Navigate(typeof(Player), vf);
            }
        }

        private static byte[] ComputeMovieHash(Stream input)
        {
            long lhash;
            long streamsize;
            streamsize = input.Length;
            lhash = streamsize;

            long i = 0;
            byte[] buffer = new byte[sizeof(long)];
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }

            input.Position = Math.Max(0, streamsize - 65536);
            i = 0;
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }
            input.Close();
            byte[] result = BitConverter.GetBytes(lhash);
            Array.Reverse(result);
            return result;
        }

        private static string ToHexadecimal(byte[] bytes)
        {
            StringBuilder hexBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                hexBuilder.Append(bytes[i].ToString("x2"));
            }
            return hexBuilder.ToString();
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            string path = ((MenuFlyoutItem)sender).Tag.ToString();
            Debug.WriteLine(((MenuFlyoutItem)sender).Tag);
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            if (file != null)
            {
                string hex;
                double length;

                Stream stream = await file.OpenStreamForReadAsync();
                byte[] result = ComputeMovieHash(stream);
                hex = ToHexadecimal(result);
                var properties = await file.GetBasicPropertiesAsync();
                length = properties.Size;

                await Task.Run(() => this.messenger.OSLogIn());

                SearchSubtitlesResponse ssre = null;
                await Task.Run(() => this.messenger.SearchOS(hex, length, "all", ref ssre));
                //List<string> subtitles = new List<string>();
                List<SubInfo> subtitles = new List<SubInfo>();
                List<string> subInfo = new List<string>();
                foreach (SubInfo info in ssre.data)
                {
                    subtitles.Add(info);
                }
                SubtitleComparator c = new SubtitleComparator();
                subtitles.Sort(c);
                SubtitleList.Items.Clear();
                foreach(var x in subtitles)
                {
                    SubtitleList.Items.Add(x);
                }
                SubtitleDialog.IsPrimaryButtonEnabled = false;
                ContentDialogResult DialogResult = await SubtitleDialog.ShowAsync();
                if(DialogResult == ContentDialogResult.Primary)
                {
                    SubInfo selectedSubtitle = (SubInfo)SubtitleList.SelectedItem;
                    byte[] subtitleStream = null;
                    if (selectedSubtitle != null)
                    {
                        await Task.Run(() => this.messenger.DownloadSubtitle(int.Parse(selectedSubtitle.IDSubtitleFile), ref subtitleStream));
                        StorageFolder sf = await file.GetParentAsync();
                        string subtitlename = file.Name.Substring(0, file.Name.LastIndexOf('.'));
                        StorageFile sfile = await sf.CreateFileAsync(subtitlename + ".srt",CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(sfile, subtitleStream);
                    }
                    else
                    {
                        Debug.WriteLine("Subtitle not found");
                    }

                }
            }
        }

        private void SubtitleList_ItemClick(object sender, ItemClickEventArgs e)
        {
            SubtitleDialog.IsPrimaryButtonEnabled = true;
        }
    }
}
