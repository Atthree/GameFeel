using UnityEngine;

namespace Atthree.GameFeel.Demo
{
    /// <summary>
    /// "Juice it or lose it" demosu: hedeflere tıkla, efektleri tek tek aç/kapat ve farkı gör.
    /// Tools > Game Feel > Create Demo Scene ile kurulur.
    /// </summary>
    [AddComponentMenu("Game Feel/Demo/Juice Demo Panel")]
    public class JuiceDemo : MonoBehaviour
    {
        public Transform jumper;

        bool shake = true, hitStop = true, flash = true, punch = true, particles = true, sound = true, numbers = true;
        bool jumping;
        GUIStyle label, title;
        Rect panel;

        void OnGUI()
        {
            label ??= new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
            title ??= new GUIStyle(label) { fontSize = 20, fontStyle = FontStyle.Bold };

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && !panel.Contains(e.mousePosition)) ClickWorld(e.mousePosition);

            panel = new Rect(12, 12, 330, Screen.height - 24);
            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Label("Game Feel Demo", title);
            GUILayout.Label("Hedeflere tıkla. Efektleri kapatıp farkı hisset.", label);
            GUILayout.Space(6);

            bool all = shake && hitStop && flash && punch && particles && sound && numbers;
            bool newAll = GUILayout.Toggle(all, all ? " JUICE: AÇIK" : " JUICE: KAPALI (hepsini aç)");
            if (newAll != all) shake = hitStop = flash = punch = particles = sound = numbers = newAll;

            shake = GUILayout.Toggle(shake, " Kamera sarsıntısı");
            hitStop = GUILayout.Toggle(hitStop, " Hit stop (kısa donma)");
            flash = GUILayout.Toggle(flash, " Flash (beyaz yanıp sönme)");
            punch = GUILayout.Toggle(punch, " Punch (basıl-uza)");
            particles = GUILayout.Toggle(particles, " Parçacıklar");
            sound = GUILayout.Toggle(sound, " Ses");
            numbers = GUILayout.Toggle(numbers, " Hasar sayıları");
            Apply();

            GUILayout.Space(10);
            if (GUILayout.Button("Zıpla (squash & stretch)", GUILayout.Height(30))) Jump();
            if (GUILayout.Button("Büyük patlama (sarsıntı + ağır çekim)", GUILayout.Height(30))) BigExplosion();

            GUILayout.Space(10);
            GUILayout.Label("Erişilebilirlik ayarı örneği:", label);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sarsıntı şiddeti", label, GUILayout.Width(120));
            Feel.ShakeIntensity = GUILayout.HorizontalSlider(Feel.ShakeIntensity, 0f, 1f, GUILayout.Width(120));
            GUILayout.Label($"{Feel.ShakeIntensity * 100:0}%", label);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label($"timeScale: {Time.timeScale:0.00}   tween: {Tween.ActiveCount}", label);
            GUILayout.EndArea();
        }

        void Apply()
        {
            foreach (var player in FindObjectsByType<FeedbackPlayer>(FindObjectsSortMode.None))
            {
                player.SetEnabled<ShakeFeedback>(shake);
                player.SetEnabled<HitStopFeedback>(hitStop);
                player.SetEnabled<FlashFeedback>(flash);
                player.SetEnabled<PunchScaleFeedback>(punch);
                player.SetEnabled<ParticlesFeedback>(particles);
                player.SetEnabled<SoundFeedback>(sound);
            }
            DemoTarget.ShowDamageNumbers = numbers;
        }

        void ClickWorld(Vector2 guiPosition)
        {
            var cam = Camera.main;
            if (cam == null) return;
            var ray = cam.ScreenPointToRay(new Vector3(guiPosition.x, Screen.height - guiPosition.y, 0f));
            if (Physics.Raycast(ray, out var hit, 200f) && hit.collider.TryGetComponent(out DemoTarget target))
                target.Hit(hit.point);
        }

        /// <summary>Tween zinciri örneği: uza → yüksel → düş → basıl.</summary>
        void Jump()
        {
            if (jumper == null || jumping) return;
            jumping = true;
            Vector3 ground = jumper.position;
            if (punch) Feel.Stretch(jumper, 0.3f, 0.25f);
            Tween.Move(jumper, ground + Vector3.up * 2f, 0.35f).SetEase(Ease.OutQuad).OnComplete(() =>
                Tween.Move(jumper, ground, 0.3f).SetEase(Ease.InQuad).OnComplete(() =>
                {
                    if (punch) Feel.Squash(jumper, 0.35f, 0.35f);
                    if (shake) Feel.Shake(0.15f);
                    jumping = false;
                }));
        }

        void BigExplosion()
        {
            if (shake) Feel.Shake(1f);
            if (hitStop) Feel.SlowMotion(0.2f, 0.6f, 0.5f);
            foreach (var target in FindObjectsByType<DemoTarget>(FindObjectsSortMode.None))
            {
                if (flash) Feel.Flash(target.gameObject, new Color(1f, 0.6f, 0.2f), 0.3f);
                if (punch) Tween.PunchScale(target.transform, Vector3.one * 0.4f, 0.6f, 5);
                if (numbers) Feel.Text("BOOM", target.transform.position + Vector3.up * 1.5f, new Color(1f, 0.5f, 0.1f), 1.4f);
            }
        }
    }
}
