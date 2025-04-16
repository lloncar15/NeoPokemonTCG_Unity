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
    public class ProfilesController : MonoBehaviour {
        private bool _hasLoadedProfiles = false;

        private readonly Dictionary<Type, Dictionary<int, Profile>> _profilesForTypes = new();
        public void Start() {
            LoadPokemonProfiles();
        }

        /// <summary>
        /// Loads all required profiles.
        /// TODO: Decouple this 
        /// </summary>
        private void LoadPokemonProfiles() {
            LoadSetProfiles();
            LoadDeckProfiles();
            LoadCardProfiles();
        }

        /// <summary>
        /// Loads the set profiles and adds them to the profile's dictionary.
        /// </summary>
        private void LoadSetProfiles() {
            string json = GetJsonStringForFile("setProfiles");
            
            Dictionary<int, Profile> setProfiles = new();
            JsonDecoder decoder = new JsonDecoder(json);
            JSONArray decoderCurrentNode = decoder.CurrentNode as JSONArray;
            if (decoderCurrentNode != null) {
                for (int i = 0; i < decoderCurrentNode.Count; i++) {
                    try {
                        Profile profile = PokemonProfileFactory.CreateSetProfile();
                        if (decoder.Get(i, ref profile)) {
                            setProfiles.Add(i, profile);
                        }
                    }
                    catch (Exception e) {
                        Debug.Log($"ProfilesController - Could not load set profiles - {e}");
                    }
                }
            }
            _profilesForTypes[typeof(SetProfile)] = setProfiles;
        }

        /// <summary>
        /// Loads the deck profiles and adds them to the profiles dictionary while adding deck profile ids into corresponding sets.
        /// </summary>
        private void LoadDeckProfiles() {
            string json = GetJsonStringForFile("deckProfiles");
            Dictionary<int, Profile> deckProfiles = new();
            JsonDecoder decoder = new JsonDecoder(json);
            JSONArray decoderCurrentNode = decoder.CurrentNode as JSONArray;
            if (decoderCurrentNode != null) {
                for (int i = 0; i < decoderCurrentNode.Count; i++) {
                    try {
                        DeckProfile profile = PokemonProfileFactory.CreateDeckProfile();
                        if (decoder.Get(i, ref profile)) {
                            deckProfiles.Add(i, profile);
                            // Add the deck profile id to the corresponding set profile
                            SetProfile setProfile = GetProfile<SetProfile>(profile.SetId);
                            setProfile?.AddDeckProfile(profile.Id);
                        }
                    }
                    catch (Exception e) {
                        Debug.Log($"ProfilesController - Could not load deck profiles - {e}");
                    }
                }
            }
            _profilesForTypes[typeof(DeckProfile)] = deckProfiles;
        }

        private void LoadCardProfiles() {
            string json = GetJsonStringForFile("cardProfiles");
            Dictionary<int, Profile> cardProfiles = new();
            JsonDecoder decoder = new JsonDecoder(json);
            JSONArray decoderCurrentNode = decoder.CurrentNode as JSONArray;
            if (decoderCurrentNode != null) {
                for (int i = 0; i < decoderCurrentNode.Count; i++) {
                    try {
                        string superType = decoderCurrentNode[i]["superType"];
                        CardProfile profile = PokemonProfileFactory.CreateCardProfile(superType);
                        if (decoder.Get(i, ref profile)) {
                            cardProfiles.Add(i, profile);
                            // Add the card profile id to the corresponding set profile
                            SetProfile setProfile = GetProfile<SetProfile>(profile.SetId);
                            setProfile?.AddCardProfile(profile.Id);
                        }
                    }
                    catch (Exception e) {
                        Debug.Log($"ProfilesController - Could not load card profiles - {e}");
                    }
                }
            }
            _profilesForTypes[typeof(CardProfile)] = cardProfiles;
        }
        
        #region Profile Getters

        public T GetProfile<T>(int profileId) where T : Profile {
            return GetProfile<T>(typeof(T), profileId);
        }

        private T GetProfile<T>(Type type, int profileId) where T : Profile {
            _profilesForTypes.TryGetValue(type, out Dictionary<int, Profile> profilesForType);
            return profilesForType?.GetValueOrDefault(profileId) as T;
        }

        public Profile GetProfile(Type type, int profileId) {
            _profilesForTypes.TryGetValue(type, out Dictionary<int, Profile> profilesForType);
            return profilesForType?.GetValueOrDefault(profileId);
        }

        public List<T> GetAllProfiles<T>() where T : Profile {
            List<T> profiles = new();
            if (_profilesForTypes.TryGetValue(typeof(T), out Dictionary<int, Profile> profilesForType)) {
                profiles = profilesForType.Values.Select(profile => profile as T).ToList();
            }

            return profiles;
        }
        
        #endregion
        
        #region Helpers

        /// <summary>
        /// Gets the json string from a specified file in the Resources folder.
        /// </summary>
        /// <param name="fileName">File name of the specified profiles.</param>
        /// <returns>The json string in the file.</returns>
        private string GetJsonStringForFile(string fileName = "") {
            TextAsset jsonFile = Resources.Load<TextAsset>($"Profiles/{fileName}");
            string jsonString = null;
            if (jsonFile is null) {
                Debug.LogError($"Could not load profile file {fileName}");
            }
            else {
                jsonString = jsonFile.text;
            }

            return jsonString;
        }
        
        #endregion
    }
}