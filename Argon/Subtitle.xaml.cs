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
using SubLib;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using System.Text;
using Windows.Data.Json;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Argon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Subtitle : Page
    {
        private const int MaxAttempts = 10;

        private OSIntermediary messenger = new OSIntermediary();
        private string currentPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
        private List<string> languages = new List<string>();
        private bool isConfigRead = false;
        private ObservableCollection<SubtitleEntry> collection = new ObservableCollection<SubtitleEntry>();
        public Subtitle()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<SubtitleEntry> Collection
        {
            get
            {
                return this.collection;
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

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add("*");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string hex;
                double length;
                this.Collection.Clear();

                Stream stream = await file.OpenStreamForReadAsync();
                byte[] result = ComputeMovieHash(stream);
                hex = ToHexadecimal(result);
                var properties = await file.GetBasicPropertiesAsync();
                length = properties.Size;

                await Task.Run(() => this.messenger.OSLogIn());

                SearchSubtitlesResponse ssre = null;
                SubInfo subtitle = null;
                await Task.Run(() => this.messenger.SearchOS(hex, length, "all", ref ssre));
                foreach(SubInfo info in ssre.data)
                {
                    if (info.LanguageName == "English")
                        subtitle = info;
                }

                byte[] subtitleStream = null;
                if(subtitle != null)
                {
                    await Task.Run(() => this.messenger.DownloadSubtitle(int.Parse(subtitle.IDSubtitleFile), ref subtitleStream));
                    StorageFolder sf = await file.GetParentAsync();
                    string subtitlename = file.Name.Substring(0, file.Name.LastIndexOf('.'));
                    StorageFile sfile = await sf.CreateFileAsync(subtitlename + ".srt");
                    await FileIO.WriteBytesAsync(sfile, subtitleStream);
                }
                else
                {
                    Debug.WriteLine("Subtitle not found");
                }
                
                
            }
        }
    }
}
