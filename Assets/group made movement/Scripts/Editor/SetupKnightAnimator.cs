using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class SetupKnightAnimator : EditorWindow
{
    [MenuItem("Tools/Setup Knight Animator Controller")]
    public static void ShowWindow()
    {
        GetWindow<SetupKnightAnimator>("Setup Knight Animator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Setup Knight Animator Controller", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will create an Animator Controller for the knight with Idle and Charge animations.", MessageType.Info);
        
        if (GUILayout.Button("Create Knight Animator Controller"))
        {
            CreateKnightAnimatorController();
        }
    }

    private void CreateKnightAnimatorController()
    {
        // Create Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(
            "Assets/Toon_RTS_demo/animations/Knight_Animator.controller");

        // Get animation clips from FBX files
        // Unity stores animation clips inside FBX files, so we need to load them differently
        Object[] idleObjects = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Toon_RTS_demo/animations/WK_heavy_infantry_05_combat_idle.FBX");
        Object[] chargeObjects = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Toon_RTS_demo/animations/WK_heavy_infantry_04_charge.FBX");

        AnimationClip idleClip = null;
        AnimationClip chargeClip = null;

        // Find the AnimationClip in the loaded objects
        foreach (Object obj in idleObjects)
        {
            if (obj is AnimationClip clip && clip.name.Contains("idle"))
            {
                idleClip = clip;
                break;
            }
        }

        foreach (Object obj in chargeObjects)
        {
            if (obj is AnimationClip clip && clip.name.Contains("charge"))
            {
                chargeClip = clip;
                break;
            }
        }

        // If not found by name, try to get the first AnimationClip
        if (idleClip == null)
        {
            foreach (Object obj in idleObjects)
            {
                if (obj is AnimationClip)
                {
                    idleClip = obj as AnimationClip;
                    break;
                }
            }
        }

        if (chargeClip == null)
        {
            foreach (Object obj in chargeObjects)
            {
                if (obj is AnimationClip)
                {
                    chargeClip = obj as AnimationClip;
                    break;
                }
            }
        }

        if (idleClip == null || chargeClip == null)
        {
            EditorUtility.DisplayDialog("Error", 
                $"Could not find animation clips.\nIdle: {(idleClip == null ? "Not found" : "Found")}\nCharge: {(chargeClip == null ? "Not found" : "Found")}\n\nMake sure the FBX files are imported correctly.", 
                "OK");
            return;
        }

        // Get the root state machine
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // Create IsMoving parameter
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

        // Create Idle state
        AnimatorState idleState = rootStateMachine.AddState("Idle");
        idleState.motion = idleClip;
        rootStateMachine.defaultState = idleState;

        // Create Charge state
        AnimatorState chargeState = rootStateMachine.AddState("Charge");
        chargeState.motion = chargeClip;

        // Create transitions
        AnimatorStateTransition idleToCharge = idleState.AddTransition(chargeState);
        idleToCharge.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");
        idleToCharge.hasExitTime = false;
        idleToCharge.duration = 0.25f;

        AnimatorStateTransition chargeToIdle = chargeState.AddTransition(idleState);
        chargeToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");
        chargeToIdle.hasExitTime = false;
        chargeToIdle.duration = 0.25f;

        EditorUtility.DisplayDialog("Success", 
            "Knight Animator Controller created successfully at:\nAssets/Toon_RTS_demo/animations/Knight_Animator.controller", 
            "OK");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

