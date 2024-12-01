namespace GimGim.Models {
    public abstract class Card {
        private string _id;
        private string _name;
        private SuperType _superType;
        private SubType _subType;
        private int _setNumber;
        private string _setId;
        private Rarity _rarity;
    }
}