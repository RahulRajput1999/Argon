﻿using System;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Player : Page
    {
        public Player()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MediaFile file = (MediaFile)e.Parameter;
            StorageFile storageFile;
            StorageFolder storageFolder;
            if (file.Path == "MusicLibrary")
            {
                storageFolder = KnownFolders.MusicLibrary;
            }
            else
            {
                storageFolder = KnownFolders.VideosLibrary;
            }
            storageFile = await storageFolder.GetFileAsync(file.Name);
            var stream = await storageFile.OpenAsync(FileAccessMode.Read);
            mediaElement.SetSource(stream, storageFile.ContentType);
            mediaElement.Play();
             
        }
    }
}
