using System.Collections.Generic;
using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Data {
    public abstract class CardProfile : Profile {
        private int _setId;
        private string _name;
        private string _setCode;
        private SuperType _superType;
        private List<SubType> _subTypes = new List<SubType>();
        private Rarity _rarity;
        private Dictionary<string, string> _images = new Dictionary<string, string>();
        private int _hp;
        
        public int SetId => _setId;
        public string Name => _name;
        public string SetCode => _setCode;
        public SuperType SuperType => _superType;
        public List<SubType> SubTypes => _subTypes;
        public Rarity Rarity => _rarity;
        public Dictionary<string, string> Images => _images;
        public int Hp => _hp;
        
        private const int SetBaseId = 1000;

        public override bool Decode(IDecoder decoder) {
            bool success = true;
            
            success &= base.Decode(decoder);
            success &= decoder.Get("name", ref _name);
            success &= decoder.Get("setCode", ref _setCode);
            success &= decoder.Get("supertype", ref _superType);
            success &= decoder.Get("subtypes", ref _subTypes);
            success &= decoder.Get("rarity", ref _rarity);
            success &= decoder.Get("images", ref _images);
            success &= decoder.Get("hp", ref _hp);

            _setId = (Id / SetBaseId) * SetBaseId;
            
            return success;
        }
    }
}