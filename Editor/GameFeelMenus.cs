using Atthree.GameFeel.Demo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Atthree.GameFeel.Editor
{
    public static class GameFeelMenus
    {
        const string HitSoundPath = "Packages/com.atthree.game-feel/Sounds/Hit.wav";

        [MenuItem("Tools/Game Feel/Add Camera Shaker To Main Camera", priority = 0)]
        static void AddShaker()
        {
            var cam = Camera.main;
            if (cam == null) { EditorUtility.DisplayDialog("Game Feel", "Sahnede 'MainCamera' etiketli kamera yok.", "Tamam"); return; }
            if (!cam.TryGetComponent(out CameraShaker shaker)) shaker = Undo.AddComponent<CameraShaker>(cam.gameObject);
            Selection.activeObject = shaker;
        }

        [MenuItem("Tools/Game Feel/Create Demo Scene", priority = 20)]
        static void CreateDemoScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var cam = Camera.main;
            cam.transform.SetPositionAndRotation(new Vector3(0f, 4.5f, -9f), Quaternion.Euler(20f, 0f, 0f));
            cam.gameObject.AddComponent<CameraShaker>();

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(3f, 1f, 3f);
            Tint(ground, new Color(0.35f, 0.38f, 0.42f));

            var sparks = CreateSparks();
            var hit = AssetDatabase.LoadAssetAtPath<AudioClip>(HitSoundPath);

            var shapes = new[] { PrimitiveType.Cube, PrimitiveType.Capsule, PrimitiveType.Cylinder };
            var colors = new[] { new Color(0.9f, 0.25f, 0.2f), new Color(0.25f, 0.55f, 0.95f), new Color(0.3f, 0.8f, 0.4f) };
            for (int i = 0; i < 3; i++)
            {
                var target = GameObject.CreatePrimitive(shapes[i]);
                target.name = "Target " + (i + 1);
                target.transform.position = new Vector3(-3f + i * 3f, 1f, 1f);
                Tint(target, colors[i]);

                var player = target.AddComponent<FeedbackPlayer>();
                player.Add(new ShakeFeedback { trauma = 0.25f });
                player.Add(new HitStopFeedback { seconds = 0.05f });
                player.Add(new FlashFeedback { color = Color.white, seconds = 0.12f });
                player.Add(new PunchScaleFeedback { punch = new Vector3(0.2f, -0.2f, 0.2f), seconds = 0.3f });
                player.Add(new ParticlesFeedback { particles = sparks, spawnCopyAtPosition = true });
                player.Add(new SoundFeedback { clip = hit, volume = 0.8f });

                var demoTarget = target.AddComponent<DemoTarget>();
                demoTarget.hitFeedback = player;
            }

            var jumper = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            jumper.name = "Jumper";
            jumper.transform.position = new Vector3(5.5f, 1f, 1f);
            Tint(jumper, new Color(0.95f, 0.8f, 0.25f));

            var panel = new GameObject("Juice Demo");
            panel.AddComponent<JuiceDemo>().jumper = jumper.transform;

            Selection.activeGameObject = panel;
            Debug.Log("[GameFeel] Demo sahnesi kuruldu. Play'e bas ve hedeflere tıkla.");
        }

        /// <summary>Kıvılcım parçacığı (pasif şablon; FeedbackPlayer her vuruşta kopyasını yaratır).</summary>
        static ParticleSystem CreateSparks()
        {
            var go = new GameObject("Sparks (template)");
            go.SetActive(false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.2f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.5f), new Color(1f, 0.6f, 0.2f));
            main.gravityModifier = 1.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var pipeline = GraphicsSettings.currentRenderPipeline;
            renderer.sharedMaterial = pipeline != null && pipeline.defaultParticleMaterial != null
                ? pipeline.defaultParticleMaterial
                : AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
            return ps;
        }

        static void Tint(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            var material = new Material(renderer.sharedMaterial) { color = color };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            renderer.sharedMaterial = material;
        }
    }
}
