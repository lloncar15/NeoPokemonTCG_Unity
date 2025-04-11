using GimGim.Enums;
using GimGim.Serialization;

namespace GimGim.Data {
    public abstract class CardProfile : Profile {
        // Identifiers for a card profile
        private int _setNumber;
        private string _id;
        private string _setId;
        
        private string _name;
        private SuperType _superType;
        private SubType _subType;
        private Rarity _rarity;


        public override void Encode(IEncoder encoder) {
            base.Encode(encoder);
        }

        public override bool Decode(IDecoder decoder) {
            base.Decode(decoder);
            return false;
        }
    }
}