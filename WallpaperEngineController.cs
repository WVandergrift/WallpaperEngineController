using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WallpaperEngineController
{
    public class WallpaperEngineController : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private WallpaperEngineControllerSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("38e2766f-b92e-4910-8c15-f8fb13684c39");

        public WallpaperEngineController(IPlayniteAPI api) : base(api)
        {
            settings = new WallpaperEngineControllerSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {

        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Get the install path for wallpaper engine
            Game wallpaperEngine = this.PlayniteApi.Database.Games.Where(i => i.Name == "Wallpaper Engine").FirstOrDefault();
            if (wallpaperEngine == null)
            {
                logger.Error("Wallpaper engine not found.");
                return;
            }

            // Try to find a Wallpaper Configuration for the game that's starting
            WallpaperGameConfiguration config = settings.Settings.GameConfigurations.Where(i => i.Game == args.Game.Name && i.IsEnabled == true).FirstOrDefault();

            if (config != null)
            {
                logger.Info($"Preparing to set Wallpaper Engine profile to: {config.WallpaperEngineProfile}");                
                System.Diagnostics.Process.Start(Path.Combine(wallpaperEngine.InstallDirectory, "wallpaper32.exe"), $"-control openProfile -profile \"{config.WallpaperEngineProfile}\"");

                // Check to see if we should mute the wallpaper
                string muteSetting = config.Mute ? "mute" : "unmute";           
                System.Diagnostics.Process.Start(Path.Combine(wallpaperEngine.InstallDirectory, "wallpaper32.exe"), $"-control {muteSetting}");

            }            
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new WallpaperEngineControllerSettingsView();
        }
    }
}