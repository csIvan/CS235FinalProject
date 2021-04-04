using UnityEngine;
using UnityEngine.UI;

public class DataUploadTextManager : MonoBehaviour
{
    [SerializeField] private string successMessage;
    [SerializeField] private string failureMessage;
    private Text textComponent;

    void Awake()
    {
        textComponent = gameObject.GetComponent<Text>();
    }

    public void showSuccess()
    {
        textComponent.text = successMessage;
    }

    public void showFailure()
    {
        textComponent.text = failureMessage;
    }
}
