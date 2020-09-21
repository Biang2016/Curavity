using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class InputDialogWindow : EditorWindow
{
    public string editorWindowText = "Input Dialog Window";
    private UnityAction<string> ConfirmAction;
    private UnityAction CancelAction;
    private string ConfirmButtonText;
    private string CancelButtonText;

    private string inputString;

    public InputDialogWindow(string windowText, string confirmButtonText, string cancelButtonText, UnityAction<string> confirm, UnityAction cancel)
    {
        editorWindowText = windowText;
        ConfirmAction = confirm;
        CancelAction = cancel;
        ConfirmButtonText = confirmButtonText;
        CancelButtonText = cancelButtonText;
    }

    void OnGUI()
    {
        inputString = EditorGUILayout.TextField(editorWindowText, inputString);

        if (GUILayout.Button(ConfirmButtonText))
        {
            ConfirmAction?.Invoke(inputString);
            Close();
        }

        if (GUILayout.Button(CancelButtonText))
        {
            CancelAction?.Invoke();
            Close();
        }
    }
}