using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Atthree.GameFeel.Editor
{
    /// <summary>
    /// FeedbackPlayer Inspector'ı: "Efekt Ekle" menüsü, her efekt için açılır kutu, sıralama, silme,
    /// Play modunda "Oynat" düğmesi.
    /// </summary>
    [CustomEditor(typeof(FeedbackPlayer))]
    public class FeedbackPlayerEditor : UnityEditor.Editor
    {
        SerializedProperty items, playOnEnable, muted;

        void OnEnable()
        {
            items = serializedObject.FindProperty(nameof(FeedbackPlayer.items));
            playOnEnable = serializedObject.FindProperty(nameof(FeedbackPlayer.playOnEnable));
            muted = serializedObject.FindProperty(nameof(FeedbackPlayer.muted));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var player = (FeedbackPlayer)target;

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button(Application.isPlaying ? "▶ Oynat" : "▶ Oynat (Play modunda)", GUILayout.Height(26)))
                    player.Play();
            }

            EditorGUILayout.PropertyField(playOnEnable);
            EditorGUILayout.PropertyField(muted);
            EditorGUILayout.Space();

            for (int i = 0; i < items.arraySize; i++)
            {
                var element = items.GetArrayElementAtIndex(i);
                var item = player.items[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var enabledProp = element.FindPropertyRelative(nameof(FeedbackItem.enabled));
                        if (enabledProp != null) enabledProp.boolValue = EditorGUILayout.Toggle(enabledProp.boolValue, GUILayout.Width(16));
                        string label = item != null ? item.Label : "(eksik tip)";
                        element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, label, true);
                        GUILayout.FlexibleSpace();
                        using (new EditorGUI.DisabledScope(i == 0))
                            if (GUILayout.Button("▲", EditorStyles.miniButtonLeft, GUILayout.Width(22))) { items.MoveArrayElement(i, i - 1); break; }
                        using (new EditorGUI.DisabledScope(i == items.arraySize - 1))
                            if (GUILayout.Button("▼", EditorStyles.miniButtonMid, GUILayout.Width(22))) { items.MoveArrayElement(i, i + 1); break; }
                        if (GUILayout.Button("✕", EditorStyles.miniButtonRight, GUILayout.Width(22))) { items.DeleteArrayElementAtIndex(i); break; }
                    }

                    if (element.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        var child = element.Copy();
                        var end = element.GetEndProperty();
                        bool enterChildren = true;
                        while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
                        {
                            enterChildren = false;
                            if (child.name == nameof(FeedbackItem.enabled)) continue;
                            EditorGUILayout.PropertyField(child, true);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }

            if (items.arraySize == 0)
                EditorGUILayout.HelpBox("Henüz efekt yok. Aşağıdan ekle: sarsıntı, hit stop, flash, punch, parçacık, ses...", MessageType.Info);

            if (GUILayout.Button("+ Efekt Ekle", GUILayout.Height(24))) ShowAddMenu();

            serializedObject.ApplyModifiedProperties();
        }

        void ShowAddMenu()
        {
            var menu = new GenericMenu();
            var types = TypeCache.GetTypesDerivedFrom<FeedbackItem>()
                .Where(t => !t.IsAbstract && !t.IsGenericType && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.Name);
            foreach (var type in types)
            {
                var instance = (FeedbackItem)Activator.CreateInstance(type);
                menu.AddItem(new GUIContent(instance.Label.Split('(')[0].Trim()), false, () =>
                {
                    serializedObject.Update();
                    items.arraySize++;
                    var element = items.GetArrayElementAtIndex(items.arraySize - 1);
                    element.managedReferenceValue = Activator.CreateInstance(type);
                    element.isExpanded = true;
                    serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }
    }
}
