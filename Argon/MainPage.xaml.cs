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
using System.Threading;
using Windows.ApplicationModel.Activation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationDataContainer local;
        List<string> videoFormat = new List<string>() { ".mov", ".mp4" };
        public MainPage()
        {
            this.InitializeComponent();
            local = ApplicationData.Current.LocalSettings;
            NavigationPanel.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.CacheSize = 2;
            
            if (local.Values["CountMusic"] == null || local.Values["CountVideo"] == null)
            {
                local.Values["CountMusic"] = 0;
                local.Values["CountVideo"] = 0;
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Clear();
            }
            
            if (local.Values["lastState"] == null)
            {
                local.Values["lastState"] = "video";
                ContentFrame.Navigate(typeof(Videos), Frame);
            }
            else
            {
                if((String)local.Values["lastState"] == "video")
                {
                    NavigationPanel.SelectedItem = NavigationPanel.MenuItems[0];
                    //ContentFrame.Navigate(typeof(Videos), Frame);
                    //LoadVideos();
                }
                else
                {
                    NavigationPanel.SelectedItem = NavigationPanel.MenuItems[1];
                    //ContentFrame.Navigate(typeof(Music), Frame);
                    //LoadAudios();
                }
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                var preNavPageType = ContentFrame.CurrentSourcePageType;
                var _page = typeof(SettingsPage);
                if(!(_page is null) && !Type.Equals(preNavPageType, _page))
                {
                    ContentFrame.Navigate(_page);
                }
            }
            else
            {
                string item = ((NavigationViewItem)args.SelectedItem).Tag.ToString();
                if (item == "1")
                {
                    var preNavPageType = ContentFrame.CurrentSourcePageType;
                    var _page = typeof(Videos);
                    if (!(_page is null) && !Type.Equals(preNavPageType, _page))
                    {
                        ContentFrame.Navigate(_page);
                    }
                }
                else if (item == "2")
                {
                    var preNavPageType = ContentFrame.CurrentSourcePageType;
                    var _page = typeof(Music);
                    if (!(_page is null) && !Type.Equals(preNavPageType, _page))
                    {
                        ContentFrame.Navigate(_page);
                    }
                }
                else if (item == "3")
                {
                    var preNavPageType = ContentFrame.CurrentSourcePageType;
                    var _page = typeof(Subtitle);
                    if (!(_page is null) && !Type.Equals(preNavPageType, _page))
                    {
                        ContentFrame.Navigate(_page);
                    }
                }
            }
            
        }
    }

}
