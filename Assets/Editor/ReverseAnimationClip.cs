using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

public static class ReverseAnimationClip
{
    [MenuItem("Tools/Animation/Reverse Selected Animation Clip")]
    static void ReverseClip()
    {
        AnimationClip original = Selection.activeObject as AnimationClip;
        if (original == null) return;

        AnimationClip newClip = new AnimationClip();
        EditorUtility.CopySerialized(original, newClip);

        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(original);
        foreach (var binding in bindings)
        {
            var keyframes = AnimationUtility.GetObjectReferenceCurve(original, binding);
            List<ObjectReferenceKeyframe> reversed = new List<ObjectReferenceKeyframe>();

            for (int i = keyframes.Length - 1; i >= 0; i--)
            {
                var frame = keyframes[i];
                frame.time = original.length - frame.time;
                reversed.Add(frame);
            }
            AnimationUtility.SetObjectReferenceCurve(newClip, binding, reversed.ToArray());
        }

        string path = AssetDatabase.GetAssetPath(original);
        string newPath = System.IO.Path.GetDirectoryName(path) +
                          "/" + original.name + "_Reversed.anim";
        AssetDatabase.CreateAsset(newClip, newPath);
        AssetDatabase.SaveAssets();
    }
}
