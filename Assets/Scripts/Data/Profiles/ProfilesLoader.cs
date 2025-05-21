using System;
using System.Collections.Generic;
using GimGim.Serialization;
using SimpleJSON;
using UnityEngine;

namespace GimGim.Data {
    /// <summary>
    /// Loads all the profiles from a JSON file.
    /// </summary>
    public class ProfilesLoader {
        public ProfilesFilenames Filenames;

        public ProfilesLoader(ProfilesFilenames filenames) {
            Filenames = filenames;
        }

        /// <summary>
        /// Loads all required profiles for Pokemon TCG. Set profiles should be loaded first to be able to populate
        /// card and deck ids into the set profile when loading them.
        /// </summary>
        public Dictionary<Type, Dictionary<int, Profile>> LoadPokemonProfiles() {
            Dictionary<Type, Dictionary<int, Profile>> profilesForTypes = new();
            LoadSetProfiles(ref profilesForTypes);
            LoadDeckProfiles(ref profilesForTypes);
            LoadCardProfiles(ref profilesForTypes);

            return profilesForTypes;
        }
         /// <summary>
        /// Loads the set profiles and adds them to the profile's dictionary.
        /// </summary>
        public void LoadSetProfiles(ref Dictionary<Type, Dictionary<int, Profile>> profilesForTypes) {
            string json = GetJsonStringForFile(Filenames.SetProfilesFilename);
            
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
            profilesForTypes[typeof(SetProfile)] = setProfiles;
        }

        /// <summary>
        /// Loads the deck profiles and adds them to the profiles dictionary while adding deck profile ids into corresponding sets.
        /// </summary>
        public void LoadDeckProfiles(ref Dictionary<Type, Dictionary<int, Profile>> profilesForTypes) {
            string json = GetJsonStringForFile(Filenames.DeckProfilesFilename);
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
                            profilesForTypes.TryGetValue(typeof(SetProfile), out var setProfiles);
                            SetProfile setProfile = setProfiles?.GetValueOrDefault(profile.SetId) as SetProfile;
                            setProfile?.AddDeckProfile(profile.Id);
                        }
                    }
                    catch (Exception e) {
                        Debug.Log($"ProfilesController - Could not load deck profiles - {e}");
                    }
                }
            }
            profilesForTypes[typeof(DeckProfile)] = deckProfiles;
        }

        /// <summary>
        /// Loads the card profiles and adds them to the profiles dictionary while adding card profile ids into corresponding sets.
        /// </summary>
        public void LoadCardProfiles(ref Dictionary<Type, Dictionary<int, Profile>> profilesForTypes) {
            string json = GetJsonStringForFile(Filenames.CardProfilesFilename);
            Dictionary<int, Profile> cardProfiles = new();
            JsonDecoder decoder = new JsonDecoder(json);
            JSONArray decoderCurrentNode = decoder.CurrentNode as JSONArray;
            if (decoderCurrentNode != null) {
                for (int i = 0; i < decoderCurrentNode.Count; i++) {
                    try {
                        string superType = decoderCurrentNode[i]["supertype"];
                        CardProfile profile = PokemonProfileFactory.CreateCardProfile(superType);
                        if (decoder.Get(i, ref profile)) {
                            cardProfiles.Add(i, profile);
                            // Add the card profile id to the corresponding set profile
                            profilesForTypes.TryGetValue(typeof(SetProfile), out var setProfiles);
                            SetProfile setProfile = setProfiles?.GetValueOrDefault(profile.SetId) as SetProfile;
                            setProfile?.AddCardProfile(profile.Id);
                        }
                    }
                    catch (Exception e) {
                        Debug.Log($"ProfilesController - Could not load card profile {decoderCurrentNode[i]["id"]} - {e}");
                    }
                }
            }
            profilesForTypes[typeof(CardProfile)] = cardProfiles;
        }
        
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
        
        public struct ProfilesFilenames {
            public string SetProfilesFilename;
            public string DeckProfilesFilename;
            public string CardProfilesFilename;

            public ProfilesFilenames(string setProfilesFilename, string deckProfilesFilename, string cardProfilesFilename) {
                SetProfilesFilename = setProfilesFilename;
                DeckProfilesFilename = deckProfilesFilename;
                CardProfilesFilename = cardProfilesFilename;
            }
        }
        
        #endregion
    }
}