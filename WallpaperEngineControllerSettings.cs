using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallpaperEngineController
{
    public class WallpaperGameConfiguration
    {
        public string Game { get; set; }
        public string WallpaperEngineProfile { get; set; }
        public bool IsEnabled { get; set; }
        public bool Mute { get; set; }
    }

    public class GameListItem
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public string IconPath { get; set; }
    }

    public class WallpaperEngineControllerSettings : ObservableObject
    {
        public ObservableCollection<WallpaperGameConfiguration> GameConfigurations { get; set; } = new ObservableCollection<WallpaperGameConfiguration>();
    }

    public class WallpaperEngineControllerSettingsViewModel : ObservableObject, ISettings
    {        
        private readonly WallpaperEngineController plugin;
        private WallpaperEngineControllerSettings editingClone { get; set; }

        private WallpaperEngineControllerSettings settings;
        public WallpaperEngineControllerSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GameListItem> games = new ObservableCollection<GameListItem>();
        public ObservableCollection<GameListItem> Games
        {
            get => games;
            set
            {
                games = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> profiles = new ObservableCollection<string>();
        public ObservableCollection<string> Profiles
        {
            get => profiles;
            set
            {
                profiles = value;
                OnPropertyChanged();
            }
        }

        public WallpaperEngineControllerSettingsViewModel(WallpaperEngineController plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<WallpaperEngineControllerSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new WallpaperEngineControllerSettings();
            }
        }

        public void BeginEdit()
        {
            ILogger logger = LogManager.GetLogger();
            string wallpaperEnginePath = "";

            // Build our list of games for the combobox
            games.Clear();
            foreach (Game game in plugin.PlayniteApi.Database.Games)
            {
                GameListItem item = new GameListItem();
                item.Id = game.Id;
                item.Name = game.Name;

                item.IconPath = Path.Combine(plugin.PlayniteApi.Database.GetFileStoragePath(game.Id), Path.GetFileName(game.Icon));
                games.Add(item);

                // If the current game is Wallpaper Engine, get it's install path
                if (game.Name == "Wallpaper Engine")
                {
                    wallpaperEnginePath = game.InstallDirectory;
                }
            }


            // Build our list of Wallpaper Engine profiles for the combobox
            if (wallpaperEnginePath != "")
            {
                profiles.Clear();
                try
                {
                    using (StreamReader file = File.OpenText(Path.Combine(wallpaperEnginePath, "config.json")))
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject json = (JObject)JToken.ReadFrom(reader);
                        JArray wpProfiles = (JArray)json[Environment.UserName]["general"]["profiles"];
                        foreach (var profile in wpProfiles)
                        {
                            profiles.Add((string)profile["name"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to read Wallpaper Engine config {ex.Message}");
                }
            }

            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }

        public RelayCommand AddGameCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.GameConfigurations.Add(new WallpaperGameConfiguration() { IsEnabled = true });
            });
        }

        public RelayCommand<WallpaperGameConfiguration> RemoveGameCommand
        {
            get => new RelayCommand<WallpaperGameConfiguration>((a) =>
            {
                Settings.GameConfigurations.Remove(a);
            });
        }
    }
}