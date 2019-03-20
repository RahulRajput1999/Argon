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
using Windows.Media.Playlists;
using System.Diagnostics;
using Windows.Media.Playback;

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
        List<Model.Playlist> PlaylistsList = new List<Model.Playlist>();
        Model.Playlist clickedPlaylist;
        IReadOnlyList<StorageFile> fileList;
        List<AudioFile> songFileList = new List<AudioFile>();
        List<AudioFile> queue = new List<AudioFile>();
        int currentPlaying;
        public Music()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            local = ApplicationData.Current.LocalSettings;
            
            LoadAudios();
            LoadPlaylists();
            LoadPlaylistsView();
        }

        private async void LoadPlaylistsView()
        {
            PlaylistList1.Items.Clear();
            StorageFolder sf = KnownFolders.MusicLibrary;
            fileList = await sf.GetFilesAsync();
            List<Windows.Media.Playlists.Playlist> playlists = new List<Windows.Media.Playlists.Playlist>();
            foreach (StorageFile sfl in fileList)
            {
                if (sfl.FileType == ".wpl")
                {
                    Model.Playlist playlist = new Model.Playlist();
                    playlist.Name = sfl.Name;
                    playlist.Path = sfl.Path;
                    Windows.Media.Playlists.Playlist plst = await Windows.Media.Playlists.Playlist.LoadAsync(sfl);
                    playlists.Add(plst);
                    PlaylistList1.Items.Add(playlist);
                }
            }
        }

        private async void LoadPlaylists()
        {
            StorageFolder sf = KnownFolders.MusicLibrary;
            IReadOnlyList<StorageFile> fileList = await sf.GetFilesAsync();
            List<Windows.Media.Playlists.Playlist> playlists = new List<Windows.Media.Playlists.Playlist>();
            foreach (StorageFile sfl in fileList)
            {
                if (sfl.FileType == ".wpl")
                {
                    Model.Playlist playlist = new Model.Playlist();
                    playlist.Name = sfl.Name;
                    playlist.Path = sfl.Path;
                    PlaylistsList.Add(playlist);
                }
            }
           
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

        public void LoadAlbums()
        {
            AlbumListView.Items.Clear();
            if (songFileList != null)
            {
                var albumList = songFileList.GroupBy(x => x.Album).Select(g => g.First());
                foreach (var x in albumList)
                {
                    AlbumListView.Items.Add(x.Album);
                }
            }
            else
            {
                Debug.WriteLine("Got null as filelist");
            }
            
        }

        public void LoadArtists()
        {
            ArtistListView.Items.Clear();
            if (songFileList != null)
            {
                var artistList = songFileList.GroupBy(x => x.Artist).Select(g => g.First());
                foreach (var x in artistList)
                {
                    ArtistListView.Items.Add(x.Artist);
                }
            }
            else
            {
                Debug.WriteLine("Got null as filelist");
            }

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
                            AudioFile o1 = new AudioFile();
                            Image i = new Image();
                            MusicProperties musicProperties = await f.Properties.GetMusicPropertiesAsync();
                            i.Source = bitmapImage;
                            o1.Thumb = i;
                            o1.Title = f.Name;
                            if (musicProperties.Title != "")
                            {
                                o1.Title = musicProperties.Title;
                                o1.Album = musicProperties.Album;
                                o1.Artist = musicProperties.Artist;
                            }
                            o1.Name = f.Name;
                            o1.Path = "MusicLibrary";
                            SongList.Items.Add(o1);
                            songFileList.Add(o1);
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
            LoadAlbums();
            LoadArtists();

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
            queue.Clear();
            foreach(var af in songFileList)
            {
                queue.Add(af);
            }
            currentPlaying = queue.IndexOf((AudioFile)e.ClickedItem);
            mediaElement.AutoPlay = true;
            mediaElement.MediaPlayer.Play();
        }

        private void MusicNavigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

        }

        private async void NewPlaylist_click(object sender, RoutedEventArgs e)
        {
            var buttonTag = ((Button)sender).Tag.ToString();
            Debug.WriteLine(buttonTag);
            ContentDialogResult result = await NewPlayListDialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                Windows.Media.Playlists.Playlist playlist = new Windows.Media.Playlists.Playlist();
                StorageFolder sf = KnownFolders.MusicLibrary;
                string name = PlaylistName.Text;
                NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting;
                PlaylistFormat format = PlaylistFormat.WindowsMedia;
                StorageFile storageFile = await sf.GetFileAsync(buttonTag);
                playlist.Files.Add(storageFile);
                try
                {
                    StorageFile savedFile = await playlist.SaveAsAsync(sf, name, collisionOption, format);
                    LoadPlaylistsView();
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.StackTrace);
                }
            }
        }

        private async void ExistingPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var buttonTag = ((Button)sender).Tag.ToString();
            Debug.WriteLine("Inside dialog:");
            PlaylistList.Items.Clear();
            foreach (var v in PlaylistsList)
            {
                PlaylistList.Items.Add(v);
                Debug.WriteLine(v.Name);
            }
            Debug.WriteLine("Ends for loop:");
            ExistingPlayListDialog.IsPrimaryButtonEnabled = false;
            ContentDialogResult result = await ExistingPlayListDialog.ShowAsync();
            
            if (result == ContentDialogResult.Primary)
            {
                Windows.Media.Playlists.Playlist playlist = new Windows.Media.Playlists.Playlist();
                StorageFolder sf = KnownFolders.MusicLibrary;
                IReadOnlyList<StorageFile> fileList = await sf.GetFilesAsync();
                var playlistFile = fileList.Where(f => f.Name == clickedPlaylist.Name).FirstOrDefault();
                playlist = await Windows.Media.Playlists.Playlist.LoadAsync(playlistFile);
                NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting;
                PlaylistFormat format = PlaylistFormat.WindowsMedia;
                StorageFile storageFile = await sf.GetFileAsync(buttonTag);
                playlist.Files.Add(storageFile);
                try
                {
                    StorageFile savedFile = await playlist.SaveAsAsync(sf, playlistFile.Name.Replace(".wpl",""), collisionOption, format);
                    Debug.WriteLine("Edited successfully");
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.StackTrace);
                    Debug.WriteLine("Something went wrong:" + error.StackTrace);
                }

            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            queue.Clear();
            var buttonTag = ((Button)sender).Tag.ToString();
            if (fileList != null)
            {
                var fileToPlay = fileList.Where(f => f.Name == buttonTag).FirstOrDefault();
                if (fileToPlay != null)
                {
                    Windows.Media.Playlists.Playlist playlist = await Windows.Media.Playlists.Playlist.LoadAsync(fileToPlay);
                    MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
                    foreach (var f in playlist.Files)
                    {
                        var af = songFileList.Where(sf => sf.Name == f.Name).FirstOrDefault();
                        queue.Add(af);
                        mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromStorageFile(f)));
                    }
                    if (mediaPlaybackList.Items.Count != 0)
                    {
                        mediaElement.Source = mediaPlaybackList;
                        mediaElement.MediaPlayer.Play();
                    }
                }
            }
        }

        private async void PlaylistList_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlaylistSongs.Items.Clear();
            const ThumbnailMode thumbnailMode = ThumbnailMode.MusicView;
            Model.Playlist playlistToShow = (Model.Playlist)e.ClickedItem;
            var fileToShow = fileList.Where(f => f.Name == playlistToShow.Name).FirstOrDefault();
            Windows.Media.Playlists.Playlist playlist = await Windows.Media.Playlists.Playlist.LoadAsync(fileToShow);
            foreach (var s in playlist.Files)
            {
                const uint size = 100;
                using (StorageItemThumbnail thumbnail = await s.GetThumbnailAsync(thumbnailMode, size))
                {
                    // Also verify the type is ThumbnailType.Image (album art) instead of ThumbnailType.Icon 
                    // (which may be returned as a fallback if the file does not provide album art) 
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                        Model.MediaFile o1 = new Model.AudioFile();
                        Image i = new Image();
                        MusicProperties musicProperties = await s.Properties.GetMusicPropertiesAsync();
                        i.Source = bitmapImage;
                        o1.Thumb = i;
                        o1.Title = s.Name;
                        if (musicProperties.Title != "")
                        {
                            o1.Title = musicProperties.Title;
                        }
                        o1.Name = s.Name;
                        o1.Path = "MusicLibrary";
                        PlaylistSongs.Items.Add(o1);
                    }
                }
            }
            SonglistView.Title = fileToShow.Name.Replace(".wpl", "");
            SonglistView.IsPrimaryButtonEnabled = true;
            SonglistView.PrimaryButtonText = "Save";
            PlaylistSongs.CanDragItems = true;
            PlaylistSongs.CanReorderItems = true;
            PlaylistSongs.AllowDrop = true;
            ContentDialogResult contentDialogResult = await SonglistView.ShowAsync();
            if (contentDialogResult == ContentDialogResult.Primary)
            {
                playlist.Files.Clear();
                StorageFolder sf = KnownFolders.MusicLibrary;
                NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting;
                PlaylistFormat format = PlaylistFormat.WindowsMedia;
                foreach (Model.MediaFile item in PlaylistSongs.Items)
                {
                    StorageFile storageFile = await sf.GetFileAsync(item.Name);
                    playlist.Files.Add(storageFile);
                    Debug.WriteLine(item.Name);
                }
                StorageFile savedFile = await playlist.SaveAsAsync(sf, fileToShow.Name.Replace(".wpl", ""), collisionOption, format);
            }
            else if (contentDialogResult == ContentDialogResult.Secondary)
            {

                if (fileToShow != null)
                {
                    Windows.Media.Playlists.Playlist playlistToPlay = await Windows.Media.Playlists.Playlist.LoadAsync(fileToShow);
                    MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
                    foreach (var f in playlist.Files)
                    {
                        mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromStorageFile(f)));
                    }
                    if (mediaPlaybackList.Items.Count != 0)
                    {
                        mediaElement.Source = mediaPlaybackList;
                        mediaElement.MediaPlayer.Play();
                    }
                }
            }
        }

        private async void AlbumListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlaylistSongs.Items.Clear();
            string albumName= (string)e.ClickedItem;
            SonglistView.Title = albumName;
            SonglistView.IsPrimaryButtonEnabled = false;
            var listToShow = songFileList.Where(x => x.Album == albumName);
            List<StorageFile> filesToPlay = new List<StorageFile>();
            StorageFolder musicFolder = KnownFolders.MusicLibrary;
            foreach(AudioFile af in listToShow)
            {
                PlaylistSongs.Items.Add(af);
                StorageFile sf = await musicFolder.GetFileAsync(af.Name);
                filesToPlay.Add(sf);
            }
            PlaylistSongs.CanDragItems = false;
            PlaylistSongs.CanReorderItems = false;
            PlaylistSongs.AllowDrop = false;
            ContentDialogResult result = await SonglistView.ShowAsync();
            if(result == ContentDialogResult.Secondary)
            {
                MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
                foreach (var f in filesToPlay)
                {
                    mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromStorageFile(f)));
                }
                if (mediaPlaybackList.Items.Count != 0)
                {
                    mediaElement.Source = mediaPlaybackList;
                    mediaElement.MediaPlayer.Play();
                }
            }
        }

        private async void ArtistListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlaylistSongs.Items.Clear();
            string artistName = (string)e.ClickedItem;
            SonglistView.Title = artistName;
            SonglistView.IsPrimaryButtonEnabled = false;
            var listToShow = songFileList.Where(x => x.Artist == artistName);
            List<StorageFile> filesToPlay = new List<StorageFile>();
            StorageFolder musicFolder = KnownFolders.MusicLibrary;
            foreach (AudioFile af in listToShow)
            {
                PlaylistSongs.Items.Add(af);
                StorageFile sf = await musicFolder.GetFileAsync(af.Name);
                filesToPlay.Add(sf);
            }
            PlaylistSongs.CanDragItems = false;
            PlaylistSongs.CanReorderItems = false;
            PlaylistSongs.AllowDrop = false;
            ContentDialogResult result = await SonglistView.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
                foreach (var f in filesToPlay)
                {
                    mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromStorageFile(f)));
                }
                if (mediaPlaybackList.Items.Count != 0)
                {
                    mediaElement.Source = mediaPlaybackList;
                    mediaElement.MediaPlayer.Play();
                }
            }
        }

        private async void QueueButton_Click(object sender, RoutedEventArgs e)
        {
            PlaylistSongs.Items.Clear();
            foreach(AudioFile af in queue)
            {
                PlaylistSongs.Items.Add(af);
            }
            PlaylistSongs.SelectedIndex = currentPlaying;
            PlaylistSongs.CanDragItems = true;
            PlaylistSongs.CanReorderItems = true;
            PlaylistSongs.AllowDrop = true;
            SonglistView.IsPrimaryButtonEnabled = true;
            SonglistView.PrimaryButtonText = "Save";
            ContentDialogResult result = await SonglistView.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                queue.Clear();
                foreach(AudioFile af in PlaylistSongs.Items)
                {
                    queue.Add(af);
                }
            }
        }

        private void PlaylistList_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            clickedPlaylist = (Model.Playlist)e.ClickedItem;
            ExistingPlayListDialog.IsPrimaryButtonEnabled = true;
        }
    }
}
