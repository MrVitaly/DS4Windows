﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Vapour.Shared.Common.Services;
using Vapour.Shared.Common.Telemetry;
using Vapour.Shared.Common.Util;
using Vapour.Shared.Configuration.Profiles.Schema;

namespace Vapour.Shared.Configuration.Profiles.Services;

/// <summary>
///     Single point of truth for managing profiles.
/// </summary>
public sealed class ProfilesService : IProfilesService
{
    private readonly ActivitySource
        _activitySource = new(TracingSources.AssemblyName);

    private readonly IGlobalStateService _global;

    private readonly ILogger<ProfilesService> _logger;

    private Dictionary<Guid, IProfile> _availableProfiles;
    private Dictionary<string, IProfile> _activeProfiles;

    public ProfilesService(
        ILogger<ProfilesService> logger,
        IGlobalStateService global
    )
    {
        using Activity activity = _activitySource.StartActivity(
            $"{nameof(ProfilesService)}:Constructor");

        _logger = logger;
        _global = global;
    }

    public Dictionary<Guid, IProfile> AvailableProfiles { get { return _availableProfiles; } }
    public event EventHandler<ProfileChangedEventArgs> OnActiveProfileChanged;

    public void DeleteProfile(Guid profileId)
    {
        if (_availableProfiles.ContainsKey(profileId))
        {
            DeleteProfile(_availableProfiles[profileId]);
        }
    }

    /// <summary>
    ///     Delete a profile from <see cref="AvailableProfiles" /> and from disk.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to delete.</param>
    public void DeleteProfile(IProfile profile)
    {
        if (profile is null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        var profilesChanged = new List<ProfileChangedEventArgs>();
        var defaultProfile = _availableProfiles[VapourProfile.DefaultProfileId];
        foreach (var item in _activeProfiles.Where(i => i.Value.Id == profile.Id).ToList())
        {
            var profileChangedEventArgs = new ProfileChangedEventArgs(
                item.Key, 
                GetActiveProfile(item.Key), 
                defaultProfile);
            profilesChanged.Add(profileChangedEventArgs);
            _activeProfiles[item.Key] = defaultProfile;
        }

        //TODO: switch this off of whether or not the profile was global
        string profilePath = profile.GetAbsoluteFilePath(_global.LocalProfilesDirectory);

        //
        // Does nothing if it doesn't exist anymore for whatever reason
        // 
        File.Delete(profilePath);

        _availableProfiles.Remove(profile.Id);

        if (profilesChanged.Any())
        {
            SaveActiveProfiles();
            foreach (var profileChanged in profilesChanged)
            {
                OnActiveProfileChanged?.Invoke(this, profileChanged);
            }
        }
    }

    /// <summary>
    ///     Refreshes all <see cref="AvailableProfiles" /> from compatible profile files found in profile directory.
    /// </summary>
    public void LoadAvailableProfiles()
    {
        if (!Directory.Exists(_global.LocalProfilesDirectory))
        {
            Directory.CreateDirectory(_global.LocalProfilesDirectory);
        }

        if (!File.Exists(_global.LocalDefaultProfileLocation))
        {
            PersistProfile(VapourProfile.CreateDefaultProfile(), _global.LocalProfilesDirectory);
        }

        string[] profiles = Directory
            .GetFiles(_global.LocalProfilesDirectory, $"*{VapourProfile.FileExtension}",
                SearchOption.TopDirectoryOnly);

        if (!profiles.Any())
        {
            throw new Exception("Something bad here");
        }

        _availableProfiles.Clear();

        foreach (string file in profiles)
        {
            _logger.LogDebug("Processing profile {Profile}", file);

            string stream = File.ReadAllText(file);
            VapourProfile profile = JsonSerializer.Deserialize<VapourProfile>(stream);

            if (profile is null)
            {
                _logger.LogWarning("Profile {Path} couldn't be deserialized, skipping",
                    file);
                continue;
            }

            if (_availableProfiles.ContainsKey(profile.Id))
            {
                _logger.LogWarning("Profile \"{Name}\" with ID {Id} already loaded, skipping",
                    profile.DisplayName, profile.Id);
                continue;
            }

            _availableProfiles.Add(profile.Id, profile);
        }
    }

    /// <summary>
    ///     Persists all <see cref="AvailableProfiles" /> to profile files in profile directory.
    /// </summary>
    public void SaveAvailableProfiles()
    {
        string directory = _global.LocalProfilesDirectory;

        //
        // Does nothing if the path already exists
        // 
        Directory.CreateDirectory(directory);

        foreach (IProfile profile in _availableProfiles.Values)
        {
            PersistProfile(profile, directory);
        }
    }

    /// <summary>
    ///     Performs all tasks necessary to get the service ready to operate.
    /// </summary>
    public void Initialize()
    {
        _availableProfiles = new Dictionary<Guid, IProfile>();

        //
        // Get all the necessary info restored from disk
        // 
        LoadAvailableProfiles();
        LoadActiveProfiles();
    }

    /// <summary>
    ///     Performs tasks prior to app shutdown.
    /// </summary>
    public void Shutdown()
    {
        _availableProfiles.Clear();
        _activeProfiles.Clear();
    }

    /// <summary>
    ///     Adds a pre-existing or new <see cref="VapourProfile" /> to <see cref="AvailableProfiles" /> and persists it to
    ///     disk.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to save.</param>
    public void CreateOrUpdateProfile(IProfile profile = default)
    {
        profile ??= VapourProfile.CreateNewProfile();

        if (!_availableProfiles.ContainsKey(profile.Id))
        {
            _availableProfiles.Add(profile.Id, profile);
        }
        else
        {
            _availableProfiles[profile.Id] = profile;
        }

        PersistProfile(profile, _global.LocalProfilesDirectory);
    }

    public IProfile CreateNewProfile(int index = default)
    {
        VapourProfile newProfile = VapourProfile.CreateNewProfile(index);
        return newProfile;
    }

    public void SetProfile(string controllerKey, Guid profileId)
    {
        if (_availableProfiles.ContainsKey(profileId))
        {
            var profile = _availableProfiles[profileId];
            IProfile previousProfile = null;
            if (_activeProfiles.ContainsKey(controllerKey))
            {
                if (_activeProfiles[controllerKey].Id == profileId)
                {
                    //setting the same profile do not do anything
                    return;
                }

                previousProfile = _activeProfiles[controllerKey];
                _activeProfiles[controllerKey] = _availableProfiles[profileId];
            }
            else
            {
                _activeProfiles.Add(controllerKey, profile);
            }

            SaveActiveProfiles();
            OnActiveProfileChanged?.Invoke(this, 
                new ProfileChangedEventArgs(controllerKey, previousProfile, profile));
        }
    }

    public IProfile GetActiveProfile(string controllerKey)
    {
        if (_activeProfiles.ContainsKey(controllerKey))
        {
            if (_availableProfiles.ContainsKey(_activeProfiles[controllerKey].Id))
            {
                return _activeProfiles[controllerKey];
            }
            else
            {
                _activeProfiles[controllerKey] = _availableProfiles[VapourProfile.DefaultProfileId];
                SaveActiveProfiles();
                return _activeProfiles[controllerKey];
            }
        }
        else
        {
            _activeProfiles.Add(controllerKey, _availableProfiles[VapourProfile.DefaultProfileId]);
            SaveActiveProfiles();
            return _activeProfiles[controllerKey];
        }
    }

    /// <summary>
    ///     Persist the <see cref="VapourProfile" /> to disk.
    /// </summary>
    /// <param name="profile">The <see cref="VapourProfile" /> to persist.</param>
    /// <param name="directory">The parent directory where the file will be generated (or overwritten, if existent).</param>
    private void PersistProfile(IProfile profile, string directory)
    {
        string profilePath = profile.GetAbsoluteFilePath(directory);

        _logger.LogDebug("Persisting profile {Profile} to file {File}",
            profile, profilePath);

        string profileData = JsonSerializer.Serialize(profile);

        if (File.Exists(profilePath))
        {
            File.Delete(profilePath);
        }

        FileStream file = File.Create(profilePath);
        file.Dispose();
        File.WriteAllText(profilePath, profileData);
    }

    private void LoadActiveProfiles()
    {
        if (File.Exists(_global.LocalActiveProfilesLocation))
        {
            var data = File.ReadAllText(_global.LocalActiveProfilesLocation);
            var activeProfiles = JsonSerializer.Deserialize<Dictionary<string, Guid>>(data)
                .Where(i => _availableProfiles.ContainsKey(i.Value))
                .ToDictionary(i => i.Key, i => _availableProfiles[i.Value]);
            
            _activeProfiles = activeProfiles;
        }
        else
        {
            _activeProfiles = new Dictionary<string, IProfile>();
        }
    }

    private void SaveActiveProfiles()
    {
        var activeProfileKeys = _activeProfiles.ToDictionary(i => i.Key, i => i.Value.Id);
        string data = JsonSerializer.Serialize(activeProfileKeys);
        if (File.Exists(_global.LocalActiveProfilesLocation))
        {
            File.Delete(_global.LocalActiveProfilesLocation);
        }

        FileStream file = File.Create(_global.LocalActiveProfilesLocation);
        file.Dispose();
        File.WriteAllText(_global.LocalActiveProfilesLocation, data);
    }
}