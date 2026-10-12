using System.Collections;
using EchoesOfNeon.SideScroll;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EchoesOfNeon.Tests
{
    public class WindUpTelegraphSceneTests
    {
        [UnityTest]
        public IEnumerator EveryEnemyGetsATelegraph_HiddenUntilWindUp_AndWithoutACollider()
        {
            var e = new GameObject("Enemy", typeof(CharacterController), typeof(SideScrollEnemy));
            yield return null;
            var t = e.GetComponent<WindUpTelegraph>();
            Assert.IsNotNull(t, "SideScrollEnemy must add its visual telegraph");
            Assert.IsFalse(t.IsShowing);
            var marker = e.transform.Find("WindUpTelegraph");
            Assert.IsNotNull(marker);
            Assert.IsNull(marker.GetComponent<Collider>(), "Marker must not block sonar or movement");
            Object.Destroy(e);
        }
    }
}
