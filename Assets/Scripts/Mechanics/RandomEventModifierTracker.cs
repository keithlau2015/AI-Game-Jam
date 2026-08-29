using System.Collections;
using UnityEngine;

namespace Platformer.Mechanics
{
    public class RandomEventModifierTracker : MonoBehaviour
    {
        public void TrackInvertControls(PlayerController player, float duration)
        {
            if (player == null)
                return;
            StartCoroutine(InvertControlsRoutine(player, duration));
        }

        public void TrackDisableJump(PlayerController player, float duration)
        {
            if (player == null)
                return;
            StartCoroutine(DisableJumpRoutine(player, duration));
        }

        IEnumerator InvertControlsRoutine(PlayerController player, float duration)
        {
            player.invertHorizontalInput = true;
            yield return new WaitForSecondsRealtime(duration);
            if (player != null)
                player.invertHorizontalInput = false;
        }

        IEnumerator DisableJumpRoutine(PlayerController player, float duration)
        {
            player.jumpDisabled = true;
            yield return new WaitForSecondsRealtime(duration);
            if (player != null)
                player.jumpDisabled = false;
        }
    }
}
