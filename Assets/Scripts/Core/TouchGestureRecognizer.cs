using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfNeon.Core
{
    /// <summary>
    /// Turns raw touches into the few gestures a blind player can make reliably on a phone:
    /// taps, double taps and swipes with one, two or three fingers, plus a one-finger
    /// touch-and-hold that walks toward the side of the screen it is held on.
    /// Plain C# (no MonoBehaviour) so EditMode tests can feed it fake touches;
    /// TouchControls feeds it real ones. Screen Y grows upward, as in the Input System.
    /// </summary>
    public class TouchGestureRecognizer
    {
        public enum Kind { Tap, DoubleTap, Swipe, HoldStart, HoldMove, HoldEnd }
        public enum Dir { None, Up, Down, Left, Right }

        public readonly struct Gesture
        {
            public readonly Kind kind;
            public readonly int fingers;
            public readonly Dir dir;     // swipe direction, or Left/Right for walking
            public Gesture(Kind kind, int fingers, Dir dir) { this.kind = kind; this.fingers = fingers; this.dir = dir; }
            public override string ToString() => $"{kind} {fingers} {dir}";
        }

        public event Action<Gesture> OnGesture;

        public float HoldSeconds = 0.3f;        // still this long with one finger = walk
        public float TapMaxSeconds = 0.35f;
        public float DoubleTapWindow = 0.3f;    // only multi-finger taps wait for a second tap
        public float SwipeFraction = 0.08f;     // of the screen's short side
        public float SlopFraction = 0.03f;      // movement still counted as "still"

        private readonly Dictionary<int, (Vector2 start, Vector2 pos)> _fingers = new Dictionary<int, (Vector2, Vector2)>();
        private float _screenWidth = 1f, _shortSide = 1f;
        private float _start;
        private int _maxFingers, _primary = -1;
        private bool _moved, _holding;
        private Dir _holdDir;
        private Vector2 _primaryStart, _primaryEnd;

        private int _pendingFingers;            // a multi-finger tap waiting to see if a second one follows
        private float _pendingTime;

        public bool IsHolding => _holding;

        public void SetScreen(float width, float height)
        {
            _screenWidth = Mathf.Max(1f, width);
            _shortSide = Mathf.Max(1f, Mathf.Min(width, height));
        }

        public void Down(int id, Vector2 pos, float time)
        {
            if (_fingers.Count == 0)
            {
                _start = time; _maxFingers = 0; _primary = id; _moved = false; _holding = false;
                _primaryStart = _primaryEnd = pos;
            }
            _fingers[id] = (pos, pos);
            _maxFingers = Mathf.Max(_maxFingers, _fingers.Count);
            // A second finger landing turns a walk into a multi-finger gesture.
            if (_holding && _fingers.Count > 1) EndHold();
        }

        public void Move(int id, Vector2 pos, float time)
        {
            if (!_fingers.TryGetValue(id, out var f)) return;
            _fingers[id] = (f.start, pos);
            if (id == _primary) _primaryEnd = pos;
            if (_holding)
            {
                var d = SideOf(pos);
                if (d != _holdDir) { _holdDir = d; Raise(Kind.HoldMove, 1, d); }
                return;
            }
            if ((pos - f.start).magnitude > SlopFraction * _shortSide) _moved = true;
        }

        public void Up(int id, Vector2 pos, float time)
        {
            if (!_fingers.ContainsKey(id)) return;
            Move(id, pos, time);
            _fingers.Remove(id);
            if (_fingers.Count > 0) return;

            if (_holding) { EndHold(); return; }

            var delta = _primaryEnd - _primaryStart;
            if (delta.magnitude >= SwipeFraction * _shortSide)
            {
                FlushPending();
                Raise(Kind.Swipe, _maxFingers, Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                    ? (delta.x > 0 ? Dir.Right : Dir.Left)
                    : (delta.y > 0 ? Dir.Up : Dir.Down));
                return;
            }
            if (time - _start > TapMaxSeconds) return;   // a slow press that never became a walk

            if (_maxFingers == 1) { FlushPending(); Raise(Kind.Tap, 1, Dir.None); return; }   // strikes stay instant
            if (_pendingFingers == _maxFingers && time - _pendingTime <= DoubleTapWindow)
            {
                _pendingFingers = 0;
                Raise(Kind.DoubleTap, _maxFingers, Dir.None);
                return;
            }
            FlushPending();
            _pendingFingers = _maxFingers;
            _pendingTime = time;
        }

        /// <summary>Call every frame: starts walks and releases single multi-finger taps.</summary>
        public void Tick(float time)
        {
            if (_pendingFingers > 0 && time - _pendingTime > DoubleTapWindow) FlushPending();
            if (!_holding && _fingers.Count == 1 && _maxFingers == 1 && !_moved && time - _start >= HoldSeconds
                && _fingers.TryGetValue(_primary, out var f))
            {
                _holding = true;
                _holdDir = SideOf(f.pos);
                Raise(Kind.HoldStart, 1, _holdDir);
            }
        }

        /// <summary>Drops every touch (app paused, screen reader toggled) and ends any walk.</summary>
        public void Cancel()
        {
            _fingers.Clear();
            _pendingFingers = 0;
            if (_holding) EndHold();
        }

        private void EndHold() { _holding = false; Raise(Kind.HoldEnd, 1, Dir.None); }

        private void FlushPending()
        {
            if (_pendingFingers == 0) return;
            int n = _pendingFingers;
            _pendingFingers = 0;
            Raise(Kind.Tap, n, Dir.None);
        }

        private Dir SideOf(Vector2 pos) => pos.x < _screenWidth * 0.5f ? Dir.Left : Dir.Right;

        private void Raise(Kind kind, int fingers, Dir dir) => OnGesture?.Invoke(new Gesture(kind, fingers, dir));
    }
}
