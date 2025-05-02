using System;
using GimGim.Enums;
using UnityEngine;

namespace GimGim.Data {
    /// <summary>
    /// Creates instances of set profiles and card profiles from a given json.
    /// </summary>
    public static class PokemonProfileFactory {
        public static SetProfile CreateSetProfile() {
            return new SetProfile();
        }

        public static DeckProfile CreateDeckProfile() {
            return new DeckProfile();
        }

        /// <summary>
        /// Creates an instance of a card profile based on the superType.
        /// </summary>
        public static CardProfile CreateCardProfile(string superType) {
            if (Enum.TryParse(superType, out SuperType type)) {
                switch (type) {
                    case SuperType.Energy:
                        return new EnergyProfile();
                    case SuperType.Pokemon:
                        return new PokemonProfile();
                    case SuperType.Trainer:
                        return new TrainerProfile();
                }
            }

            Debug.Log($"PokemonProfileFactory - Could not parse superType: {superType}");
            return null;
        }
    }
}