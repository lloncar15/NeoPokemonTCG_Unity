using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using GimGim.Serialization;
using SimpleJSON;

namespace GimGim.Data {
    /// <summary>
    /// Loads all the data needed for the game and caches it for later use.
    /// </summary>
    public class ProfilesController {
        private Dictionary<Type, Dictionary<int, Profile>> _profilesForTypes = new();
        public Dictionary<Type, Dictionary<int, Profile>> ProfilesForTypes => _profilesForTypes;
        
        private static ProfilesController _instance;
        
        private bool _isLoaded = false;

        /// <summary>
        /// Gets the singleton instance of the event system.
        /// </summary>
        public static ProfilesController Instance {
            get { return _instance ??= new ProfilesController(); }
        }

        public void LoadPokemonProfiles() {
            ProfilesLoader.ProfilesFilenames filenames =
                new ProfilesLoader.ProfilesFilenames("setProfiles", 
                    "deckProfiles", 
                    "cardProfiles");
            ProfilesLoader loader = new ProfilesLoader(filenames);
            _profilesForTypes = loader.LoadPokemonProfiles();
            _isLoaded = true;
        }
        
        #region Profile Getters
        public static T GetProfile<T>(int profileId) where T : Profile {
            return Instance._GetProfile<T>(profileId);
        }
        public static List<T> GetAllProfiles<T>() where T : Profile {
            return Instance._GetAllProfiles<T>();
        }
        public static Profile GetProfile(Type type, int profileId) {
            return Instance._GetProfile(type, profileId);
        }
        
        private T _GetProfile<T>(int profileId) where T : Profile {
            return _GetProfile<T>(typeof(T), profileId);
        }
        private T _GetProfile<T>(Type type, int profileId) where T : Profile {
            EnsureLoaded();
            
            _profilesForTypes.TryGetValue(type, out Dictionary<int, Profile> profilesForType);
            return profilesForType?.GetValueOrDefault(profileId) as T;
        }
        private Profile _GetProfile(Type type, int profileId) {
            EnsureLoaded();
            
            _profilesForTypes.TryGetValue(type, out Dictionary<int, Profile> profilesForType);
            return profilesForType?.GetValueOrDefault(profileId);
        }
        private List<T> _GetAllProfiles<T>() where T : Profile {
            EnsureLoaded();
                
            List<T> profiles = new();
            if (_profilesForTypes.TryGetValue(typeof(T), out Dictionary<int, Profile> profilesForType)) {
                profiles = profilesForType.Values.Select(profile => profile as T).ToList();
            }

            return profiles;
        }
        
        private void EnsureLoaded() {
            if (!_isLoaded)
                throw new InvalidOperationException("Profiles must be loaded by calling LoadPokemonProfiles() before accessing profiles.");
        }
        
        #endregion
    }
}