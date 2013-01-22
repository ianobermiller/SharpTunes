using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.WindowsAPICodePack.Shell;
using Newtonsoft.Json;

namespace SharpTunes
{
    public class Library
    {
        private static string dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SharpTunes");
        private static string libraryCachePath = Path.Combine(dataFolder, "library.json");
        private static string coverArtFolder = Path.Combine(dataFolder, "coverart");
        private static JsonSerializer serializer = new JsonSerializer();

        public static ObservableCollection<MediaFile> GetMedia()
        {
            if (File.Exists(libraryCachePath))
            {
                using (var file = File.OpenRead(libraryCachePath))
                using (var sr = new StreamReader(file))
                using (var jtr = new JsonTextReader(sr))
                {
                    return new ObservableCollection<MediaFile>(serializer.Deserialize<List<MediaFile>>(jtr));
                }
            }
            else
            {
                var library = RefreshLibrary();
                if (!Directory.Exists(Path.GetDirectoryName(libraryCachePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(libraryCachePath));
                }
                using (var file = File.Open(libraryCachePath, FileMode.Create))
                using (var sw = new StreamWriter(file))
                using (var jtr = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jtr, library);
                }
                return new ObservableCollection<MediaFile>(library);
            }
        }

        private static List<MediaFile> RefreshLibrary()
        {
            var media = new List<MediaFile>();
            using (ShellLibrary shellLibrary = ShellLibrary.Load("Music", true))
            {
                var mp3s = shellLibrary.SelectMany((ShellFileSystemFolder f) => Directory.EnumerateFiles(f.Path, "*.mp3", SearchOption.AllDirectories));

                foreach (var path in mp3s)
                {
                    try
                    {
                        using (var file = TagLib.File.Create(path))
                        {
                            var song = new MediaFile()
                            {
                                Title = file.Tag.Title,
                                Artist = file.Tag.FirstAlbumArtist ?? file.Tag.FirstPerformer,
                                Album = file.Tag.Album,
                                Path = path
                            };
                            media.Add(song);
                        }
                    }
                    catch
                    {
                        var song = new MediaFile()
                        {
                            Path = path,
                            HasError = true
                        };
                        media.Add(song);
                    }
                }
            }
            return media;
        }

        public static async Task<string> GetAlbumArt(MediaFile media)
        {
            var filePath = Path.Combine(coverArtFolder, MakePathSafe(media.Artist) + "_-_" + MakePathSafe(media.Album) + ".jpg");
            if (!File.Exists(filePath))
            {
                var coverArtUrl = await GetAlbumArtFromLastFm(media);
                coverArtUrl = coverArtUrl ?? GetAlbumArtFromInteractivePlaylist(media);
                if (!string.IsNullOrWhiteSpace(coverArtUrl))
                {
                    var bytes = await new HttpClient().GetByteArrayAsync(coverArtUrl);
                    if (!Directory.Exists(coverArtFolder))
                    {
                        Directory.CreateDirectory(coverArtFolder);
                    }
                    File.WriteAllBytes(filePath, bytes);
                }
            }
            
            return filePath;
        }

        private static string GetAlbumArtFromInteractivePlaylist(MediaFile media)
        {
            // ipfam is media, ipfal is large
            return "http://interactiveplaylist.com/ipfam?album=" +
                StripAndEncode(media.Album) + "&artist=" +
                StripAndEncode(media.Artist);
        }

        private static async Task<string> GetAlbumArtFromLastFm(MediaFile media)
        {
            var client = new HttpClient();
            var url = "http://ws.audioscrobbler.com/2.0/?method=album.search&album=" +
                StripAndEncode(media.Artist) + "+" +
                StripAndEncode(media.Album) +
                "&api_key=009a482cfc59173fb361faa0b5c49b06&format=json";
            var json = await client.GetStringAsync(url);
            dynamic result = JsonConvert.DeserializeObject(json);
            try
            {
                return result.results.albummatches.album[0].image[1]["#text"];
            }
            catch
            {
                return null;
            }
        }
        
        private static string MakePathSafe(string text)
        {
            return new Regex(@"[^\w]").Replace(text, "_");
        }

        private static string StripAndEncode(string text)
        {
            return HttpUtility.UrlEncode(new Regex(@"[^\w\s]{2,}").Replace(text, " "));
        }
    }
}
