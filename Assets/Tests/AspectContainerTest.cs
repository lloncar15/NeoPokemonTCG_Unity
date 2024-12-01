using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GimGim.AspectContainer;
using NUnit.Framework.Internal;

public class AspectContainerTest
{
    private class TestAspect : IAspect {
        public IContainer Container { get; set; }
    }
    
    private class TestAspect2 : IAspect {
        public IContainer Container { get; set; }
    }

    [Test]
    public void TestContainerCanAddAspect() {
        var container = new Container();
        container.AddAspect<TestAspect>();
        Assert.AreEqual(container.Aspects().Count, 1);
    }

    [Test]
    public void TestContainerCanAddMultipleAspects() {
        var container = new Container();
        container.AddAspect<TestAspect>("Test1");
        container.AddAspect<TestAspect>("Test2");
        Assert.AreEqual(container.Aspects().Count, 2);
    }

    [Test]
    public void TestContainerCanAddMultipleTypesOfAspects() {
        var container = new Container();
        container.AddAspect<TestAspect>();
        container.AddAspect<TestAspect2>();
        Assert.AreEqual(container.Aspects().Count, 2);
    }

    [Test]
    public void TestContainerCanGetAspectWithNoKey() {
        var container = new Container();
        var addedAspect = container.AddAspect<TestAspect>();
        var gottenAspect = container.GetAspect<TestAspect>();
        Assert.AreSame(addedAspect, gottenAspect);
    }
    
    [Test]
    public void TestContainerCanGetAspectWithKey() {
        var container = new Container();
        var addedAspect = container.AddAspect<TestAspect>("Test");
        var gottenAspect = container.GetAspect<TestAspect>("Test");
        Assert.AreSame(addedAspect, gottenAspect);
    }

    [Test]
    public void TestContainerCanTryGetMissingAspect() {
        var container = new Container();
        var gottenAspect = container.GetAspect<TestAspect>("Test");
        Assert.IsNull(gottenAspect);
    }
    
    [Test]
    public void TestContainerCanAddPreCreatedAspect() {
        var container = new Container();
        var aspect = new TestAspect();
        container.AddAspect(aspect);
        Assert.AreEqual(container.Aspects().Count, 1);
    }
    
    [Test]
    public void TestContainerCanGetPreCreatedAspect() {
        var container = new Container();
        var aspect = new TestAspect();
        container.AddAspect(aspect);
        var gottenAspect = container.GetAspect<TestAspect>();
        Assert.AreSame(aspect, gottenAspect);
    }

    [Test]
    public void TestAspectTrackItsContainer() {
        var container = new Container();
        var aspect = container.AddAspect<TestAspect>();
        Assert.IsNotNull(aspect.Container);
    }
}
