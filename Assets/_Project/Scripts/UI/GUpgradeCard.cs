using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class GUpgradeCard : MonoBehaviour
{
    [SerializeField]
    Image _iconImg;

    [SerializeField]
    LocalizeStringEvent _nameTxt;

    [SerializeField]
    LocalizeStringEvent _descriptionTxt;

    [SerializeField]
    Button _upgradeBtn;

    public void Initialize(int upgradeIndex)
    {
        _upgradeBtn.onClick.AddListener(() => GUpgradeManager.Instance.OnUpgradeSelected(upgradeIndex));
    }
    
    public void Build(GSOUpgrade upgrade)
    {
        _nameTxt.StringReference.SetReference(upgrade.Name.TableReference, upgrade.Name.TableEntryReference);
        _descriptionTxt.StringReference.SetReference(upgrade.Description.TableReference, upgrade.Description.TableEntryReference);
        _iconImg.sprite = upgrade.Icon;
        // TODO : Localized Name and Description
    }
}
