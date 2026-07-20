using TMPro;
using UnityEngine;

public class PasswordToggle : MonoBehaviour
{
    [Header("Password Input")]
    public TMP_InputField passwordInput;

    [Header("Show / Hide Text")]
    public GameObject showObject;
    public GameObject hideObject;

    private bool isPasswordHidden = true;

    private void Start()
    {
        if (passwordInput != null)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            passwordInput.ForceLabelUpdate();
        }

        if (showObject != null)
            showObject.SetActive(true);

        if (hideObject != null)
            hideObject.SetActive(false);
    }

    public void TogglePassword()
    {
        isPasswordHidden = !isPasswordHidden;

        if (isPasswordHidden)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;

            if (showObject != null)
                showObject.SetActive(true);

            if (hideObject != null)
                hideObject.SetActive(false);
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;

            if (showObject != null)
                showObject.SetActive(false);

            if (hideObject != null)
                hideObject.SetActive(true);
        }

        passwordInput.ForceLabelUpdate();
    }
}