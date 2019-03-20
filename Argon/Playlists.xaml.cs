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
using Windows.Media.Playlists;
using Windows.Storage;
using Windows.Storage.Search;
using System.Diagnostics;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Playlists : Page
    {
        public static readonly string[] validExtensions = new string[] { ".mp3", ".wav" };
        IReadOnlyList<StorageFile> fileList;
        public Playlists()
        {
            this.InitializeComponent();
            LoadPlaylists();
        }

        private void ClosePopupClicked(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
        }

        private async void CreateClicked(object sender, RoutedEventArgs e)
        {
            if(StandardPopup.IsOpen) { StandardPopup.IsOpen = false;}
            Playlist playlist = new Playlist();
            StorageFolder sf = KnownFolders.MusicLibrary;
            string name = PlayListName.Text;
            NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting;
            PlaylistFormat format = PlaylistFormat.WindowsMedia;
            try
            {
                StorageFile savedFile = await playlist.SaveAsAsync(sf, name, collisionOption, format);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.StackTrace);
            }

        }

        private async void LoadPlaylists()
        {
            PlaylistList.Items.Clear();
            StorageFolder sf = KnownFolders.MusicLibrary;
            fileList = await sf.GetFilesAsync();
            List<Playlist> playlists = new List<Playlist>();
            foreach (StorageFile sfl in fileList)
            {
                Debug.WriteLine(sfl.FileType);
                if(sfl.FileType == ".wpl")
                {
                    Playlist plst = new Playlist();
                    Model.Playlist playlist = new Model.Playlist();
                    playlist.Name = sfl.Name;
                    playlist.Path = sfl.Path;
                    plst = await Playlist.LoadAsync(sfl);
                    playlists.Add(plst);
                    PlaylistList.Items.Add(playlist);
                }
            }
            Debug.WriteLine("Well its done");
        }
        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!StandardPopup.IsOpen)
            {
                StandardPopup.IsOpen = true;
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var buttonTag = ((Button)sender).Tag.ToString();
            if(fileList != null)
            {
                var fileToPlay = fileList.Where(f => f.Name == buttonTag).FirstOrDefault();
                if(fileToPlay != null)
                {
                    Playlist playlist = await Playlist.LoadAsync(fileToPlay);
                    MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
                    foreach(var f in playlist.Files)
                    {
                        mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromStorageFile(f)));
                    }
                    if(mediaPlaybackList.Items.Count != 0)
                    {
                        mediaElement.Source = mediaPlaybackList;
                        mediaElement.MediaPlayer.Play();
                    }
                }
            }
            
            
        }

        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AppendButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void PlaylistList_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlaylistSongs.Items.Clear();
            const ThumbnailMode thumbnailMode = ThumbnailMode.MusicView;
            Model.Playlist playlistToShow = (Model.Playlist)e.ClickedItem;
            var fileToShow = fileList.Where(f => f.Name == playlistToShow.Name).FirstOrDefault();
            Playlist playlist = await Playlist.LoadAsync(fileToShow);
            foreach(var s in playlist.Files)
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
            PlaylistView.Title = fileToShow.Name.Replace(".wpl","");
            ContentDialogResult contentDialogResult = await PlaylistView.ShowAsync();
            if(contentDialogResult == ContentDialogResult.Primary)
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
            } else if(contentDialogResult == ContentDialogResult.Secondary)
            {
                
                if (fileToShow != null)
                {
                    Playlist playlistToPlay = await Playlist.LoadAsync(fileToShow);
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

        private async void Append_Button_Click(object sender, RoutedEventArgs e)
        {
            var buttonTag = ((Button)sender).Tag.ToString();
            if (fileList != null)
            {
                var fileToPlay = fileList.Where(f => f.Name == buttonTag).FirstOrDefault();
                if (fileToPlay != null)
                {
                    Playlist playlist = await Playlist.LoadAsync(fileToPlay);
                    FileOpenPicker picker = CreateFilePicker(validExtensions);
                    IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();

                    if (files.Count > 0)
                    {
                        foreach (StorageFile file in files)
                        {
                            playlist.Files.Add(file);
                        }

                        if (await TrySavePlaylistAsync(playlist))
                        {
                            //rootPage.NotifyUser(files.Count + " files added to playlist.", NotifyType.StatusMessage);
                        }
                    }
                }
            }
        }
        public static FileOpenPicker CreateFilePicker(string[] extensions)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;

            foreach (string extension in extensions)
            {
                picker.FileTypeFilter.Add(extension);
            }

            return picker;
        }
        async public Task<bool> TrySavePlaylistAsync(Playlist playlist)
        {
            try
            {
                await playlist.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async void Shuffle_Button_Click(object sender, RoutedEventArgs e)
        {
            var buttonTag = ((Button)sender).Tag.ToString();
            if (fileList != null)
            {
                var fileToPlay = fileList.Where(f => f.Name == buttonTag).FirstOrDefault();
                if (fileToPlay != null)
                {
                    Playlist playlist = await Playlist.LoadAsync(fileToPlay);
                    MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
                    foreach (var item in playlist.Files)
                    {
                        mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromStorageFile(item)));
                    }
                    mediaPlaybackList.ShuffleEnabled = true;
                    IReadOnlyList<MediaPlaybackItem> items = mediaPlaybackList.ShuffledItems;
                    MediaPlaybackList newplaybacklist = new MediaPlaybackList();
                    foreach (var item in items)
                    {
                        newplaybacklist.Items.Add(item);
                    }
                    if (newplaybacklist.Items.Count != 0)
                    {
                        mediaElement.Source = newplaybacklist;
                        mediaElement.MediaPlayer.Play();
                    }
                }
            }
        }
    }
}
