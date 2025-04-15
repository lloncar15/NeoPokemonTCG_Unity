using System.Collections.Generic;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Data {
    public abstract class CardProfile : Profile {
        private int _setId;
        
        private string _name;
        private SuperType _superType;
        private SubType _subType;
        private Rarity _rarity;
        private List<Dictionary<string, string>> _images = new List<Dictionary<string, string>>();
        private int _hp;

        public override bool Decode(IDecoder decoder) {
            bool success = true;
            
            success &= base.Decode(decoder);
            success &= decoder.Get("name", ref _name);
            success &= decoder.Get("superType", ref _superType);
            success &= decoder.Get("subType", ref _subType);
            success &= decoder.Get("rarity", ref _rarity);
            success &= decoder.Get("images", ref _images);
            success &= decoder.Get("hp", ref _hp);

            int setId = 0;
            success &= decoder.Get("setId", ref setId);
            _setId = setId - Id;
            
            return success;
        }
    }
}