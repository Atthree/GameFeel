using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Atthree.GameFeel.Tests
{
    public class GameFeelTests
    {
        readonly List<Object> cleanup = new List<Object>();
        float fixedDelta;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
            fixedDelta = Time.fixedDeltaTime;
            Feel.ShakeIntensity = 1f;
            Feel.FlashEnabled = true;
            Feel.TimeEffectsEnabled = true;
        }

        [TearDown]
        public void TearDown()
        {
            TimeEffects.Cancel();
            Tween.KillAll();
            Time.timeScale = 1f;
            Time.fixedDeltaTime = fixedDelta;
            foreach (var o in cleanup) if (o != null) Object.Destroy(o);
            cleanup.Clear();
        }

        GameObject Make(string name = "obj", PrimitiveType? type = null)
        {
            var go = type.HasValue ? GameObject.CreatePrimitive(type.Value) : new GameObject(name);
            cleanup.Add(go);
            return go;
        }

        static IEnumerator WaitReal(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }

        // ------------------------------------------------------------------ ease
        [Test]
        public void Eases_StartAtZero_EndAtOne()
        {
            foreach (Ease e in System.Enum.GetValues(typeof(Ease)))
            {
                Assert.AreEqual(0f, EaseFunctions.Evaluate(e, 0f), 0.001f, e + " başlangıç");
                Assert.AreEqual(1f, EaseFunctions.Evaluate(e, 1f), 0.001f, e + " bitiş");
            }
            Assert.AreEqual(0.5f, EaseFunctions.Evaluate(Ease.Linear, 0.5f), 0.0001f);
            Assert.Greater(EaseFunctions.Evaluate(Ease.OutBack, 0.7f), 1f, "OutBack hedefi geçmeli");
            Assert.Greater(EaseFunctions.Evaluate(Ease.OutQuad, 0.5f), 0.5f, "Out hızlı başlamalı");
        }

        // ------------------------------------------------------------------ tween
        [UnityTest]
        public IEnumerator Move_ReachesTarget_WithDelay_AndCallsCompleteOnce()
        {
            var go = Make();
            int completed = 0;
            Tween.Move(go.transform, new Vector3(0, 2, 0), 0.3f).SetDelay(0.2f).OnComplete(() => completed++);

            yield return new WaitForSeconds(0.15f);
            Assert.AreEqual(0f, go.transform.position.y, 0.001f, "gecikme sırasında hareket etmemeli");
            yield return new WaitForSeconds(0.45f);
            Assert.AreEqual(2f, go.transform.position.y, 0.001f);
            Assert.AreEqual(1, completed);
        }

        [UnityTest]
        public IEnumerator Loops_Yoyo_EndsAtStart()
        {
            var go = Make();
            Tween.Move(go.transform, new Vector3(1, 0, 0), 0.15f).SetLoops(2, yoyo: true);
            yield return new WaitForSeconds(0.2f);
            Assert.Greater(go.transform.position.x, 0.3f, "dönüş yolunda");
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(0f, go.transform.position.x, 0.001f, "yoyo başa dönmeli");
        }

        [UnityTest]
        public IEnumerator Kill_AndDestroyedOwner_StopSafely()
        {
            var a = Make();
            var b = Make();
            Tween.Move(a.transform, Vector3.one * 5, 1f);
            Tween.Move(b.transform, Vector3.one * 5, 1f);
            yield return new WaitForSeconds(0.1f);

            Tween.Kill(a.transform);
            Vector3 stopped = a.transform.position;
            Object.Destroy(b); // sahibi yok oldu: hata vermemeli
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(stopped, a.transform.position, "durdurulan tween hareket etmemeli");
            Assert.AreEqual(0, Tween.ActiveCount);
        }

        [UnityTest]
        public IEnumerator Unscaled_RunsWhilePaused_ScaledDoesNot()
        {
            var scaled = Make();
            var unscaled = Make();
            Time.timeScale = 0f;
            Tween.Move(scaled.transform, Vector3.up, 0.2f);
            Tween.Move(unscaled.transform, Vector3.up, 0.2f).SetUnscaled();
            yield return WaitReal(0.3f);
            Assert.AreEqual(0f, scaled.transform.position.y, 0.001f, "duraklatılmışken ilerlememeli");
            Assert.AreEqual(1f, unscaled.transform.position.y, 0.001f, "ölçeksiz tween bitmeli");
        }

        [UnityTest]
        public IEnumerator Punch_RepeatedCalls_DoNotDrift()
        {
            var go = Make();
            go.transform.localScale = new Vector3(2, 2, 2);
            for (int i = 0; i < 6; i++)
            {
                Tween.PunchScale(go.transform, new Vector3(0.5f, -0.5f, 0.5f), 0.2f);
                yield return null; // her karede yeni punch, öncekini bitirir
            }
            Assert.AreNotEqual(new Vector3(2, 2, 2), go.transform.localScale, "punch sürerken ölçek değişmeli");
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(new Vector3(2, 2, 2), go.transform.localScale, "ölçek kaymadan başa dönmeli");
        }

        [UnityTest]
        public IEnumerator ShakePosition_ReturnsToStart()
        {
            var go = Make();
            go.transform.localPosition = new Vector3(1, 2, 3);
            Tween.ShakePosition(go.transform, 0.5f, 0.2f);
            yield return null;
            yield return null;
            Assert.AreNotEqual(new Vector3(1, 2, 3), go.transform.localPosition);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(new Vector3(1, 2, 3), go.transform.localPosition);
        }

        // ------------------------------------------------------------------ kamera
        /// <summary>Kamerayı her LateUpdate'te mutlak konuma koyan takip kamerası gibi davranır.</summary>
        class FollowCamera : MonoBehaviour
        {
            public Vector3 position;
            void LateUpdate() => transform.position = position;
        }

        /// <summary>Karenin en sonunda (sarsıntı uygulandıktan sonra) kameranın konumunu kaydeder.</summary>
        [DefaultExecutionOrder(32500)]
        class EndOfFrameProbe : MonoBehaviour
        {
            public Vector3 position;
            public Quaternion rotation;
            void LateUpdate() { position = transform.position; rotation = transform.rotation; }
        }

        [UnityTest]
        public IEnumerator CameraShake_DoesNotDrift_WithOrWithoutFollowScript()
        {
            var camGo = Make("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            camGo.transform.position = new Vector3(0, 5, -10);
            var follow = camGo.AddComponent<FollowCamera>();
            follow.position = new Vector3(0, 5, -10);
            var probe = camGo.AddComponent<EndOfFrameProbe>();

            Feel.Shake(1f);
            var shaker = camGo.GetComponent<CameraShaker>();
            Assert.NotNull(shaker, "Shake kameraya CameraShaker eklemeli");

            float maxOffset = 0f;
            for (int i = 0; i < 10; i++)
            {
                yield return null;
                maxOffset = Mathf.Max(maxOffset, Vector3.Distance(probe.position, follow.position)); // sarsıntılı hali
                follow.position += new Vector3(0.1f, 0, 0); // takip kamerası hareket ediyor
            }
            Assert.Greater(maxOffset, 0.05f, "sarsıntı görünmeli");

            yield return WaitReal(1.2f); // trauma söner
            yield return null;
            Assert.Less(Vector3.Distance(probe.position, follow.position), 0.001f, "takip kamerasıyla kayma olmamalı");

            // Takip script'i olmayan sabit kamera: tam başlangıç konumuna dönmeli.
            Object.Destroy(follow);
            yield return null;
            Vector3 rest = camGo.transform.position;
            Quaternion restRot = camGo.transform.rotation;
            Feel.Shake(1f);
            yield return WaitReal(1.3f);
            yield return null;
            Assert.Less(Vector3.Distance(rest, camGo.transform.position), 0.0001f, "sabit kamera kaymamalı");
            Assert.Less(Quaternion.Angle(restRot, camGo.transform.rotation), 0.01f);
        }

        [Test]
        public void ShakeIntensityZero_DisablesShake()
        {
            var camGo = Make("Main Camera");
            camGo.tag = "MainCamera";
            camGo.AddComponent<Camera>();
            Feel.ShakeIntensity = 0f;
            Feel.Shake(1f);
            var shaker = camGo.GetComponent<CameraShaker>();
            Assert.IsTrue(shaker == null || shaker.Trauma == 0f);
        }

        // ------------------------------------------------------------------ zaman efektleri
        [UnityTest]
        public IEnumerator HitStop_FreezesThenRestores_AndExtends()
        {
            Time.timeScale = 0.8f;
            Feel.HitStop(0.15f);
            Assert.AreEqual(0f, Time.timeScale, "hemen donmalı");
            yield return WaitReal(0.1f);
            Feel.HitStop(0.15f); // üst üste: süre uzar
            yield return WaitReal(0.1f);
            Assert.AreEqual(0f, Time.timeScale, "uzatılmış süre devam etmeli");
            yield return WaitReal(0.15f);
            Assert.AreEqual(0.8f, Time.timeScale, 0.0001f, "eski timeScale'e dönmeli");
            Assert.IsFalse(TimeEffects.IsActive);
        }

        [UnityTest]
        public IEnumerator HitStop_DoesNotOverridePauseMenu()
        {
            Feel.HitStop(0.2f);
            yield return null;
            Time.timeScale = 0.5f; // oyun kendi timeScale'ini ayarladı (ör. pause menüsü)
            yield return WaitReal(0.3f);
            Assert.AreEqual(0.5f, Time.timeScale, 0.0001f, "oyunun ayarına dokunmamalı");
            Assert.IsFalse(TimeEffects.IsActive);
        }

        [UnityTest]
        public IEnumerator SlowMotion_ScalesTimeAndPhysics_ThenRecovers()
        {
            Feel.SlowMotion(0.25f, 0.2f, 0.2f);
            yield return null;
            Assert.AreEqual(0.25f, Time.timeScale, 0.01f);
            Assert.AreEqual(fixedDelta * 0.25f, Time.fixedDeltaTime, 0.0001f, "fizik de ölçeklenmeli");
            yield return WaitReal(0.3f);
            Assert.That(Time.timeScale, Is.InRange(0.25f, 1f), "toparlanıyor");
            yield return WaitReal(0.2f);
            Assert.AreEqual(1f, Time.timeScale, 0.0001f);
            Assert.AreEqual(fixedDelta, Time.fixedDeltaTime, 0.00001f);
        }

        [Test]
        public void TimeEffectsDisabled_DoesNothing()
        {
            Feel.TimeEffectsEnabled = false;
            Feel.HitStop(1f);
            Feel.SlowMotion(0.1f, 1f);
            Assert.AreEqual(1f, Time.timeScale);
        }

        // ------------------------------------------------------------------ flash
        [UnityTest]
        public IEnumerator Flash_TintsThenClears_EvenDuringHitStop()
        {
            var cube = Make(type: PrimitiveType.Cube);
            var renderer = cube.GetComponent<Renderer>();
            var block = new MaterialPropertyBlock();

            Feel.HitStop(0.3f); // flash ölçeksiz zamanla çalışmalı
            Feel.Flash(cube, Color.red, 0.15f);
            renderer.GetPropertyBlock(block, 0);
            Assert.IsFalse(block.isEmpty, "flash sırasında renk bloğu olmalı");

            yield return WaitReal(0.25f);
            renderer.GetPropertyBlock(block, 0);
            Assert.IsTrue(block.isEmpty, "flash bitince renk bloğu temizlenmeli (materyal kopyası yok)");
            Assert.IsFalse(cube.GetComponent<HitFlash>().IsFlashing);
        }

        // ------------------------------------------------------------------ uçuşan yazı
        [UnityTest]
        public IEnumerator FloatingText_SpawnsAndReturnsToPool()
        {
            int before = FloatingText.PooledCount;
            Feel.Text("120", Vector3.zero, Color.yellow, seconds: 0.2f);
            var items = Object.FindObjectsByType<FloatingTextItem>(FindObjectsSortMode.None);
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("120", items[0].GetComponentInChildren<TextMesh>().text);
            yield return WaitReal(0.3f);
            Assert.AreEqual(before + 1, FloatingText.PooledCount, "havuza dönmeli");
            Assert.AreEqual(0, Object.FindObjectsByType<FloatingTextItem>(FindObjectsSortMode.None).Length);
        }

        // ------------------------------------------------------------------ feedback player
        [UnityTest]
        public IEnumerator FeedbackPlayer_PlaysEnabledItems_WithDelays()
        {
            var go = Make(type: PrimitiveType.Cube);
            var player = go.AddComponent<FeedbackPlayer>();
            int immediate = 0, delayed = 0, disabled = 0;
            var e1 = player.Add(new EventFeedback());
            e1.onPlay.AddListener(() => immediate++);
            var e2 = player.Add(new EventFeedback { delay = 0.2f });
            e2.onPlay.AddListener(() => delayed++);
            var e3 = player.Add(new EventFeedback { enabled = false });
            e3.onPlay.AddListener(() => disabled++);
            player.Add(new PunchScaleFeedback());

            player.Play();
            Assert.AreEqual(1, immediate);
            Assert.AreEqual(0, delayed);
            yield return WaitReal(0.1f);
            Assert.AreEqual(0, delayed, "gecikme bitmeden çalmamalı");
            Assert.AreNotEqual(Vector3.one, go.transform.localScale, "punch oynamalı");
            yield return WaitReal(0.2f);
            Assert.AreEqual(1, delayed);
            Assert.AreEqual(0, disabled, "kapalı efekt çalmamalı");

            player.SetEnabled<EventFeedback>(false);
            player.Play();
            Assert.AreEqual(1, immediate, "SetEnabled ile kapatılmalı");
        }
    }
}
