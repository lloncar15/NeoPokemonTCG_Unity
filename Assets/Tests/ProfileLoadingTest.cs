using System;
using System.Collections.Generic;
using System.Linq;
using GimGim.Data;
using NUnit.Framework;

public class ProfileLoadingTest
{
    private ProfilesLoader _loader;
    private Dictionary<Type, Dictionary<int, Profile>> _profilesForTypes = new();
    
    [SetUp]
    public void Setup()
    {
        ProfilesLoader.ProfilesFilenames filenames =
            new ProfilesLoader.ProfilesFilenames("TestData/setTestProfiles", 
                "TestData/deckTestProfiles", 
                "TestData/cardTestProfiles");
        _loader = new ProfilesLoader(filenames);

        _profilesForTypes = new Dictionary<Type, Dictionary<int, Profile>>();
    }

    [Test]
    public void TestLoadingSetProfiles() {
        _loader.LoadSetProfiles(ref _profilesForTypes);
        
        List<SetProfile> setProfiles = new();
        if (_profilesForTypes.TryGetValue(typeof(SetProfile), out Dictionary<int, Profile> profilesForType)) {
            setProfiles = profilesForType.Values.Select(profile => profile as SetProfile).ToList();
        }
        
        Assert.AreEqual(2, setProfiles.Count);
        Assert.AreEqual("Base", setProfiles[0].Name);
    }
    
    [Test]
    public void TestLoadingDeckProfiles() {
        _loader.LoadSetProfiles(ref _profilesForTypes);
        _loader.LoadDeckProfiles(ref _profilesForTypes);

        List<DeckProfile> setProfiles = new();
        if (_profilesForTypes.TryGetValue(typeof(DeckProfile), out Dictionary<int, Profile> profilesForType)) {
            setProfiles = profilesForType.Values.Select(profile => profile as DeckProfile).ToList();
        }
        
        Assert.AreEqual(2, _profilesForTypes.Count);
        Assert.AreEqual(4, setProfiles.Count);
    }

    [Test]
    public void TestLoadingCardProfiles() {
        _profilesForTypes = _loader.LoadPokemonProfiles();
        
        List<CardProfile> cardProfiles = new();
        if (_profilesForTypes.TryGetValue(typeof(CardProfile), out Dictionary<int, Profile> profilesForType)) {
            cardProfiles = profilesForType.Values.Select(profile => profile as CardProfile).ToList();
        }
        Assert.AreEqual(3, _profilesForTypes.Count);
        Assert.AreEqual(104, cardProfiles.Count);
    }
}
