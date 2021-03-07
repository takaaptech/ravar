using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Character character;
    private Vector2 input;

    public event Action OnEncounter;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    //
    // MOVEMENT
    //
    public void HandleUpdate()
    {
        if(!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // prevent diagonal movement
            if (input.x != 0) input.y = 0;

            if(input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, CheckForEncounters));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
            Interact();
    }

    //
    // INTERACTION
    //
    void Interact()
    {
        var lookingAt = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var nextTile = transform.position + lookingAt;

        var collider = Physics2D.OverlapCircle(nextTile, 0.3f, MapLayers.Instance.InteractLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    //
    // HELPER FUNCTIONS
    //
    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, MapLayers.Instance.EncountersLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 101) <= 7) //TODO - decide the percentage of event triggers
            {
                character.Animator.IsMoving = false;
                OnEncounter();
            }
        }
    }
}