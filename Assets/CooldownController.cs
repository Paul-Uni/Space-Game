using UnityEngine;
using UnityEngine.UI;

public class CooldownController : MonoBehaviour
{
    private PlayerController script;

    public GameObject playerController; // assign in inspector

    //Coldowns
    public Image alpha1CooldownImage; // assign in inspector
    public Image alpha2CooldownImage; // assign in inspector
    public Image alpha3CooldownImage; // assign in inspector
    public Image HP;

    void Start()
    {
        script = playerController.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        alpha1CooldownImage.fillAmount = script.alpha1Cooldown / script.cd;
        alpha2CooldownImage.fillAmount = script.alpha2Cooldown / script.dashCd;
        alpha3CooldownImage.fillAmount = script.alpha3Cooldown / script.ballCooldown;
        HP.fillAmount = script.HP / script.MaxHP;
    }
}
