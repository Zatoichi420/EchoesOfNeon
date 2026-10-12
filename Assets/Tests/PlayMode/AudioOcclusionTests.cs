using System.Collections;
using EchoesOfNeon.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EchoesOfNeon.Tests
{
    public class AudioOcclusionTests
    {
        private GameObject _ear, _src, _wall, _body;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _ear = new GameObject("Ear"); _ear.transform.position = Vector3.zero;
            AudioOcclusion.Ear = _ear.transform;
            _src = new GameObject("Source", typeof(AudioSource)); _src.transform.position = new Vector3(10f, 0f, 0f);
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            AudioOcclusion.Ear = null;
            foreach (var g in new[] { _ear, _src, _wall, _body }) if (g != null) Object.Destroy(g);
        }

        private static void Settle(AudioOcclusion o) { Physics.SyncTransforms(); for (int i = 0; i < 60; i++) o.Tick(0.05f); }

        [UnityTest]
        public IEnumerator WallBetween_Muffles_ThenClearsWhenRemoved()
        {
            var o = AudioOcclusion.Attach(_src.GetComponent<AudioSource>());
            Settle(o);
            Assert.IsFalse(o.IsBlocked); Assert.Greater(o.Cutoff, 15000f);

            _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _wall.transform.position = new Vector3(5f, 0f, 0f); _wall.transform.localScale = new Vector3(0.5f, 4f, 4f);
            Settle(o);
            Assert.IsTrue(o.IsBlocked); Assert.Less(o.Cutoff, 2000f);

            Object.DestroyImmediate(_wall); _wall = null;
            Settle(o);
            Assert.IsFalse(o.IsBlocked); Assert.Greater(o.Cutoff, 15000f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Characters_NeverOcclude()
        {
            var o = AudioOcclusion.Attach(_src.GetComponent<AudioSource>());
            _body = new GameObject("Guard", typeof(CharacterController));
            _body.transform.position = new Vector3(5f, 0f, 0f);
            Settle(o);
            Assert.IsFalse(o.IsBlocked, "Another character between Marcus and a sound must not muffle it");
            yield return null;
        }
    }
}
