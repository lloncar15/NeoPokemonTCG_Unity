using GimGim.Serialization;

namespace GimGim.Data {
    public class TrainerProfile : CardProfile {
        private string _rules; // TODO: create a class for trainer abilities
        
        public string Rules => _rules;
        
        public override bool Decode(IDecoder decoder) {
            bool success = true;
            
            success &= base.Decode(decoder);
            decoder.Get("rules", ref _rules);
            
            return success;
        }
    }
}