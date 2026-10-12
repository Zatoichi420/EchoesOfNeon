using System.Collections;
using EchoesOfNeon.SideScroll;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EchoesOfNeon.Tests
{
    /// <summary>Marcus going down must never cost progress: Cookie pulls him up if she can,
    /// otherwise responders carry him back to the last rest point.</summary>
    public class ReviveTests
    {
        private GameObject _marcus, _cookie;
        private SideScrollPlayer _pl;
        private SideScrollCookie _ck;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _marcus = new GameObject("Marcus", typeof(CharacterController), typeof(SideScrollPlayer));
            _cookie = new GameObject("Cookie", typeof(SideScrollCookie));
            _pl = _marcus.GetComponent<SideScrollPlayer>();
            _ck = _cookie.GetComponent<SideScrollCookie>();
            _ck.leader = _marcus.transform;
            _pl.cookie = _ck;
            yield return null;
        }

        [TearDown]
        public void TearDown() { Object.Destroy(_marcus); Object.Destroy(_cookie); }

        [UnityTest]
        public IEnumerator CookiePullsMarcusUp_WhenAvailable()
        {
            _pl.TakeUnavoidable(1000f);
            Assert.IsTrue(_pl.IsDead);
            Assert.IsTrue(_pl.IsReviving);
            for (int i = 0; i < 50; i++) _pl.Tick(0.1f);   // 5 s > reviveSeconds
            Assert.IsFalse(_pl.IsDead);
            Assert.AreEqual(0, _pl.Respawns, "A Cookie revive is not a respawn");
            Assert.Greater(_pl.Health, 0f);
            Assert.IsFalse(_ck.CanRevive, "Cookie is spent after reviving");
            yield return null;
        }

        [UnityTest]
        public IEnumerator WhenCookieIsSpent_RescueRespawnsAtRestPoint_AndRestoresBoth()
        {
            _pl.TakeUnavoidable(1000f);
            for (int i = 0; i < 50; i++) _pl.Tick(0.1f);   // Cookie revives, becomes spent
            _pl.TakeUnavoidable(1000f);                    // down again straight away
            Assert.IsFalse(_pl.IsReviving, "Spent Cookie cannot revive");
            for (int i = 0; i < 30; i++) _pl.Tick(0.1f);   // > respawnDelay
            Assert.IsFalse(_pl.IsDead);
            Assert.AreEqual(1, _pl.Respawns);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BothDown_FallsBackToRescue_AndCookieGetsUp()
        {
            _ck.TakeSplash(1000f, _cookie.transform.position.x);
            Assert.IsTrue(_ck.IsDown);
            _pl.TakeUnavoidable(1000f);
            Assert.IsFalse(_pl.IsReviving, "A downed Cookie cannot revive");
            for (int i = 0; i < 30; i++) _pl.Tick(0.1f);
            Assert.IsFalse(_pl.IsDead);
            Assert.IsFalse(_ck.IsDown, "The rescue restores Cookie too");
            yield return null;
        }

        [UnityTest]
        public IEnumerator DistantHit_DoesNotHurtCookie()
        {
            _ck.TakeSplash(1000f, _cookie.transform.position.x + 50f);
            Assert.IsFalse(_ck.IsDown);
            yield return null;
        }
    }
}
