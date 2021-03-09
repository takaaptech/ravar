using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogController : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;

    Dialog dialog;
    int currentString = 0;
    bool isTyping;

    Action onDialogFinished;
    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    public bool IsShowing { get; private set; }
    public static DialogController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowDialog(Dialog dialog, Action onFinished = null)
    {
        // Wait so the Z key isn't immediately counted as pressed
        // Otherwise we might skip the first string
        yield return new WaitForEndOfFrame();
        OnShowDialog?.Invoke();
        IsShowing = true;
        this.dialog = dialog;
        onDialogFinished = onFinished;
        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Strings[0]));
    }

    public IEnumerator TypeDialog(string dialog)
    {
        isTyping = true;
        dialogText.text = "";

        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        isTyping = false;
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isTyping)
        {
            ++currentString;
            if (currentString < dialog.Strings.Count)
                StartCoroutine(TypeDialog(dialog.Strings[currentString]));
            else
            {
                currentString = 0;
                IsShowing = false;
                dialogBox.SetActive(false);
                onDialogFinished?.Invoke();
                OnCloseDialog?.Invoke();
            }
        }
    }
}
