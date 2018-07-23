using UnityEngine;
using System.Collections;

public class LocalAIInput : IPlayerInput
{
    public void UpdateInput(ref GameInput input, LocalPlayerController controller)
    {
        input.offHand = true;


    }    
}
