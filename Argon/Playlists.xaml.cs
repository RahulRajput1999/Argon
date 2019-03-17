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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Playlists : Page
    {
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

        private void LoadPlaylists()
        {
            
        }
        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!StandardPopup.IsOpen)
            {
                StandardPopup.IsOpen = true;
            }
        }

        private void SongList_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
