using UnityEngine;

public class Effect : MonoBehaviour
{
    [SerializeField]
    protected Tower tower;

    // Be called by animation event
    public void OnAttack()
    {
        for (int i = 0; i < tower.targets.Count;)
        {
            // Update enemy
            if (tower.targets[i].IsDied())
            {
                tower.targets.Remove(tower.targets[i]);
                continue;
            }

            // Play vfx effect
            tower.attackEffects[i].SetVector3("EndPos", tower.targets[i].transform.position);
            tower.attackEffects[i].Play();

            i++;
        }

        // Play attack sound
        tower.audioSource.PlayOneShot(tower.attackSound);

        // Impact to all enemies (damage, slow,...)
        tower.Impact();
    }
}
