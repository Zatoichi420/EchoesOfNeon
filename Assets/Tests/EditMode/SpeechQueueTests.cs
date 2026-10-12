using EchoesOfNeon.Accessibility;
using NUnit.Framework;

namespace EchoesOfNeon.Tests
{
    public class SpeechQueueTests
    {
        [Test] public void IdleQueue_SpeaksAtOnce() =>
            Assert.AreEqual("hi", new SpeechQueue().Submit("hi", SpeechPriority.Low, 0f));

        [Test] public void Normal_DoesNotInterruptCritical()
        {
            var q = new SpeechQueue();
            q.Submit("Marcus is down", SpeechPriority.Critical, 0f);
            Assert.IsNull(q.Submit("Menu focus", SpeechPriority.Normal, 0.1f));
            Assert.AreEqual(1, q.PendingCount);
        }

        [Test] public void Critical_InterruptsCritical_AndAnythingElse()
        {
            var q = new SpeechQueue();
            q.Submit("a long normal sentence here", SpeechPriority.Normal, 0f);
            Assert.AreEqual("down", q.Submit("down", SpeechPriority.Critical, 0.1f));
            Assert.AreEqual("both down", q.Submit("both down", SpeechPriority.Critical, 0.2f));
        }

        [Test] public void Normal_InterruptsNormal_SoFocusStaysResponsive()
        {
            var q = new SpeechQueue();
            q.Submit("first item", SpeechPriority.Normal, 0f);
            Assert.AreEqual("second", q.Submit("second", SpeechPriority.Normal, 0.1f));
        }

        [Test] public void Low_WaitsInsteadOfInterrupting_ThenSpeaksWhenFree()
        {
            var q = new SpeechQueue();
            q.Submit("busy busy busy", SpeechPriority.Normal, 0f);
            Assert.IsNull(q.Submit("drop", SpeechPriority.Low, 0.1f));
            Assert.IsNull(q.Tick(0.2f));
            Assert.AreEqual("drop", q.Tick(2f));
        }

        [Test] public void StaleLow_IsDropped()
        {
            var q = new SpeechQueue();
            q.Submit("busy", SpeechPriority.Critical, 0f);
            q.Submit("old news", SpeechPriority.Low, 0.1f);
            Assert.IsNull(q.Tick(10f), "A Low message waiting more than 3s is stale");
        }

        [Test] public void HighestPriorityWaitingSpeaksFirst()
        {
            var q = new SpeechQueue();
            q.Submit("busy busy busy busy", SpeechPriority.Critical, 0f);
            q.Submit("low", SpeechPriority.Low, 0.1f);
            q.Submit("high", SpeechPriority.High, 0.2f);
            Assert.AreEqual("high", q.Tick(3f));
        }

        [Test] public void QueueIsCapped_DroppingLowestFirst()
        {
            var q = new SpeechQueue();
            q.Submit("busy", SpeechPriority.Critical, 0f);
            for (int i = 0; i < 6; i++) q.Submit("m" + i, i == 5 ? SpeechPriority.High : SpeechPriority.Low, 0.1f);
            Assert.AreEqual(SpeechQueue.MaxPending, q.PendingCount);
            Assert.AreEqual("m5", q.Tick(3f));
        }
    }
}
