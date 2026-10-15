using System.Collections.Generic;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using NUnit.Framework;
using UnityEngine;
using G = EchoesOfNeon.Core.TouchGestureRecognizer;

namespace EchoesOfNeon.Tests.EditMode
{
    public class TouchGestureTests
    {
        // A 1000 x 500 screen: swipes need 40 px (8% of 500), "still" allows 15 px.
        private G _r;
        private List<G.Gesture> _seen;

        [SetUp]
        public void SetUp()
        {
            _r = new G();
            _r.SetScreen(1000, 500);
            _seen = new List<G.Gesture>();
            _r.OnGesture += g => _seen.Add(g);
        }

        private void Tap(int id, float x, float t) { _r.Down(id, new Vector2(x, 250), t); _r.Up(id, new Vector2(x, 250), t + 0.1f); }

        [Test]
        public void OneFingerTapStrikesAtOnce()
        {
            Tap(0, 500, 0f);
            Assert.AreEqual(1, _seen.Count);
            Assert.AreEqual(G.Kind.Tap, _seen[0].kind);
            Assert.AreEqual(InputManager.TouchCommand.Strike, TouchControls.Map(_seen[0]));
        }

        [Test]
        public void SwipesReadTheirDirection()
        {
            _r.Down(0, new Vector2(500, 200), 0f); _r.Up(0, new Vector2(500, 300), 0.15f);
            _r.Down(0, new Vector2(500, 300), 1f); _r.Up(0, new Vector2(500, 200), 1.15f);
            _r.Down(0, new Vector2(500, 250), 2f); _r.Up(0, new Vector2(400, 250), 2.15f);
            CollectionAssert.AreEqual(new[] { G.Dir.Up, G.Dir.Down, G.Dir.Left }, _seen.ConvertAll(g => g.dir));
            Assert.AreEqual(InputManager.TouchCommand.Jump, TouchControls.Map(_seen[0]));
            Assert.AreEqual(InputManager.TouchCommand.Fend, TouchControls.Map(_seen[1]));
            Assert.AreEqual(InputManager.TouchCommand.EchoStepLeft, TouchControls.Map(_seen[2]));
        }

        [Test]
        public void HoldingWalksTowardThatSideAndStopsOnRelease()
        {
            _r.Down(0, new Vector2(100, 250), 0f);
            _r.Tick(0.2f);
            Assert.IsEmpty(_seen, "too early to walk");
            _r.Tick(0.35f);
            Assert.AreEqual(G.Kind.HoldStart, _seen[0].kind);
            Assert.AreEqual(G.Dir.Left, _seen[0].dir);
            _r.Move(0, new Vector2(900, 250), 1f);     // slides to the right half
            Assert.AreEqual(G.Kind.HoldMove, _seen[1].kind);
            Assert.AreEqual(G.Dir.Right, _seen[1].dir);
            _r.Up(0, new Vector2(900, 250), 2f);
            Assert.AreEqual(G.Kind.HoldEnd, _seen[2].kind);
            Assert.AreEqual(3, _seen.Count, "releasing a walk is not also a tap or swipe");
        }

        [Test]
        public void TwoFingerTapWaitsThenSonar()
        {
            _r.Down(0, new Vector2(400, 250), 0f); _r.Down(1, new Vector2(600, 250), 0.02f);
            _r.Up(0, new Vector2(400, 250), 0.1f); _r.Up(1, new Vector2(600, 250), 0.12f);
            Assert.IsEmpty(_seen, "waits to see if it is a double tap");
            _r.Tick(0.5f);
            Assert.AreEqual(1, _seen.Count);
            Assert.AreEqual(2, _seen[0].fingers);
            Assert.AreEqual(InputManager.TouchCommand.Sonar, TouchControls.Map(_seen[0]));
        }

        [Test]
        public void ThreeFingerDoubleTapReadsTheControls()
        {
            for (int n = 0; n < 2; n++)
            {
                float t = n * 0.2f;
                for (int id = 0; id < 3; id++) _r.Down(id, new Vector2(300 + id * 200, 250), t);
                for (int id = 0; id < 3; id++) _r.Up(id, new Vector2(300 + id * 200, 250), t + 0.08f);
            }
            _r.Tick(1f);
            Assert.AreEqual(1, _seen.Count);
            Assert.AreEqual(G.Kind.DoubleTap, _seen[0].kind);
            Assert.AreEqual(InputManager.TouchCommand.Hint, TouchControls.Map(_seen[0]));
        }

        [Test]
        public void SecondFingerEndsAWalk()
        {
            _r.Down(0, new Vector2(800, 250), 0f);
            _r.Tick(0.4f);
            _r.Down(1, new Vector2(200, 250), 0.5f);
            Assert.AreEqual(G.Kind.HoldEnd, _seen[_seen.Count - 1].kind);
        }

        [Test]
        public void EveryCommandHasAGestureAndTheGuideMentionsPause()
        {
            var reached = new HashSet<InputManager.TouchCommand>();
            foreach (G.Kind k in new[] { G.Kind.Tap, G.Kind.DoubleTap, G.Kind.Swipe })
                for (int f = 1; f <= 3; f++)
                    foreach (G.Dir d in System.Enum.GetValues(typeof(G.Dir)))
                        if (TouchControls.Map(new G.Gesture(k, f, d)) is InputManager.TouchCommand c) reached.Add(c);
            foreach (InputManager.TouchCommand c in System.Enum.GetValues(typeof(InputManager.TouchCommand)))
                if (c != InputManager.TouchCommand.Pause) Assert.IsTrue(reached.Contains(c), c + " has no gesture");
            StringAssert.Contains("Pause", TouchControls.Guide);
        }
    }
}
