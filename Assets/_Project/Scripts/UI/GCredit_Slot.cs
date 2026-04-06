using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GCredit_Slot : MonoBehaviour
{
    [SerializeField] string _creditName;
    [SerializeField] string _role;

    [TextArea(3, 8)]
    [SerializeField] string _skills;
    [SerializeField] Sprite _portrait;

    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _roleText;
    [SerializeField] TMP_Text _skillsText;
    [SerializeField] Image _portraitImage;

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    void Apply()
    {
        if (_nameText != null)
            _nameText.text = _creditName;

        if (_roleText != null)
            _roleText.text = _role;

        if (_skillsText != null)
            _skillsText.text = _skills;

        if (_portraitImage != null)
            _portraitImage.sprite = _portrait;
    }
}
