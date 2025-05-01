using System.Collections.Generic;
using GimGim.Data;
using NUnit.Framework;
using UnityEngine;

public class ProfileLoadingTest
{
    private ProfilesController _profilesController;
    
    [SetUp]
    public void Setup()
    {
        _profilesController = new GameObject().AddComponent<ProfilesController>();
        _profilesController.setProfilesFilename = "TestData/setTestProfiles";
        _profilesController.deckProfilesFilename = "TestData/deckTestProfiles";
        _profilesController.cardProfilesFilename = "TestData/cardTestProfiles";
    }

    [Test]
    public void TestLoadingSetProfiles() {
        _profilesController.LoadSetProfiles();

        List<SetProfile> setProfiles = _profilesController.GetAllProfiles<SetProfile>();
        Assert.AreEqual(2, setProfiles.Count);
        Assert.AreEqual("Base", setProfiles[0].Name);
    }
    
    [Test]
    public void TestLoadingDeckProfiles() {
        _profilesController.LoadSetProfiles();
        _profilesController.LoadDeckProfiles();

        List<DeckProfile> deckProfiles = _profilesController.GetAllProfiles<DeckProfile>();
        Assert.AreEqual(2, _profilesController.ProfilesForTypes.Count);
        Assert.AreEqual(4, deckProfiles.Count);
    }

    [Test]
    public void TestLoadingCardProfiles() {
        _profilesController.LoadPokemonProfiles();
        
        List<CardProfile> cardProfiles = _profilesController.GetAllProfiles<CardProfile>();
        Assert.AreEqual(3, _profilesController.ProfilesForTypes.Count);
    }
}
