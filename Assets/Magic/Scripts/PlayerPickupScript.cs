using UnityEngine;
using System.Collections;

// Matthew Cormack
// 30/05/2015
//
// Location:
// For attachment to the player camera controller
//
// Functionality:
// - Allows the player to hold objects with PickupDescriptionScript components
// in the left and right hands,
// - Allows for independant rotation of up to two held items
//
// Requirements:
// - Requires two children of the player for item placement;
//   - LeftHandItem
//   - RightHandItem
// These do not contain any components other than a transform
// - Requires the PlayerCraftScript for the combining item functionality
//
// Buttons:
// - Interact - default f - pickup/drop items in the current hand
// - LeftHand - default q - switch to left hand
// - RightHand - default e - switch to right hand

public class PlayerPickupScript : MonoBehaviour
{
	// The range at which the player can pickup and drop items
	public float InteractionRange = 2;

	// The time LeftHand or RightHand buttons must be held down before rotation starts
	public float RotationStartTime = 0.2f;

	// The rate at which to rotate items when the mouse is moving
	public float RotateRate = 1;

	// The amount of mouse axis movement before the item starts rotating (0->1)
	public float RotateStart = 0.1f;

	// Reference to the player controller for disabling camera turning
	public UnityStandardAssets.Characters.FirstPerson.FirstPersonController PlayerController;

	// The hand currently active for picking up/dropping/rotating an item (0 - left, 1 - right)
	private int CurrentHand = 0;

	// The items current held (left and right hands)
	// Has a public getter/setter
	private GameObject[] Item;

	// The name of the hand transform object which corresponds to each hand
	private string[] HandTransformName = { "LeftHandItem", "RightHandItem" };

	// Flag for when the player is rotating one of their held items by holding LeftHand or RightHand buttons
	private bool Rotating = false;

	// The current time a rotation button has been held, for delaying activation of item rotation
	private float RotateButtonHeldTime = 0;

	// Reference to the PlayerCraftScript created by this script upon the player
	private PlayerCraftScript PlayerCraft;

	// Use this for initialization
	void Start()
	{
		// Initialize array of held items to null
		Item = new GameObject[2];
		{
			for ( int item = 0; item < 2; item++ )
			{
				Item[item] = null;
			}
		}

		// Create the player crafting functionality script
		PlayerCraft = gameObject.AddComponent<PlayerCraftScript>();
		PlayerCraft.PlayerPickup = this;
	}

	// Update is called once per frame
	void Update()
	{
		// Switch active hands
		Update_Hand_Active();

		// Rotate an item if LeftHand or RightHand is held
		Update_Hand_Rotate();

		// Try to pickup/drop objects
		Update_Interact();
	}

	private void Update_Hand_Active()
	{
		// Activate left hand
		if ( Input.GetButtonDown( "LeftHand" ) )
		{
			CurrentHand = 0;

			// Update visual representation of selected hand
			Update_SelectedHandVisuals();
		}
		// Activate right hand
		if ( Input.GetButtonDown( "RightHand" ) )
		{
			CurrentHand = 1;

			// Update visual representation of selected hand
			Update_SelectedHandVisuals();
		}
	}

	private void Update_Hand_Rotate()
	{
		// Logic for activating rotation; Either button is pressed, not both
		if ( Input.GetButton( "LeftHand" ) ^ Input.GetButton( "RightHand" ) )
		{
			RotateButtonHeldTime += Time.deltaTime;
			if ( RotateButtonHeldTime > RotationStartTime )
			{
				// Only rotate if there is actually an item to influence
				if ( Item[CurrentHand] )
				{
					Rotating = true;
				}
			}
		}
		// Otherwise reset timer
		else
		{
			RotateButtonHeldTime = 0;
			Rotating = false;
		}

		// Rotation when activated takes over mouse/joystick input
		if ( Rotating )
		{
			// Disable camera rotation
			if ( PlayerController )
			{
				PlayerController.CanTurn = false;
			}

			// Turn hand using mouse axis
			float mousex = Input.GetAxis( "Mouse X" );
			float mousey = Input.GetAxis( "Mouse Y" );
			if ( Mathf.Abs( mousex ) > RotateStart )
			{
				Item[CurrentHand].transform.Rotate( -transform.up, ( mousex * RotateRate ), Space.World );
			}
			if ( Mathf.Abs( mousey ) > RotateStart )
			{
				Item[CurrentHand].transform.Rotate( transform.right, ( mousey * RotateRate ), Space.World );
			}
			print( Item[CurrentHand].transform.up );
		}
		else
		{
			// Re-enable camrea rotation
			if ( PlayerController )
			{
				PlayerController.CanTurn = true;
			}
		}
	}

	private void Update_Interact()
	{
		if ( Input.GetButtonDown( "Interact" ) )
		{
			// Trace forward looking for a pickup or space to drop an item
			int layer_notplayer = ~( 1 << 8 );
			Vector3 forward = GetComponent<Camera>().transform.forward;
			RaycastHit hitinfo;
			bool hit = Physics.Raycast( transform.position, forward, out hitinfo, InteractionRange, layer_notplayer );
			// Has hit something; either pickup, drop, or swap
			if ( hit )
			{
				GameObject hitobject = hitinfo.transform.gameObject;
				Transform handtransform = transform.FindChild( HandTransformName[CurrentHand] );

				// Has hit a pickup; either pickup, or swap
				if ( hitobject.GetComponent<PickupDescriptionScript>() )
				{
					// Already has a pickup; swap
					if ( Item[CurrentHand] )
					{
						// Drop the old item at the other item's position
						DropItem( hitobject.transform.position );

						// Pickup the new item
						PickupItem( hitobject, handtransform );
					}
					// Doesn't have a pickup; pickup
					else
					{
						PickupItem( hitobject, handtransform );
					}
				}
				// Hasn't hit a pickup; drop if item is held
				else if ( Item[CurrentHand] )
				{
					DropItem( hitinfo.point );
				}
			}
			// Hasn't hit anything; if an item is held, drop it at InteractionRange forward
			else if ( Item[CurrentHand] )
			{
				DropItem( transform.position + ( forward * InteractionRange ) );
			}
		}
	}

	private void Update_SelectedHandVisuals()
	{
		// Get the index of the other (not currently selected) hand
		int other = 1;
		{
			if ( CurrentHand == 1 )
			{
				other = 0;
			}
		}
		Transform currenthand = transform.FindChild( HandTransformName[CurrentHand] );
		Transform otherhand = transform.FindChild( HandTransformName[other] );

		// Move active forward slightly
		currenthand.localPosition = new Vector3( currenthand.localPosition.x, currenthand.localPosition.y, 1.2f );
		otherhand.localPosition = new Vector3( otherhand.localPosition.x, otherhand.localPosition.y, 1 );

		// Turn active item slightly green
		currenthand.gameObject.GetComponent<MeshRenderer>().material.color = new Color( 0, 1, 0 );
		otherhand.gameObject.GetComponent<MeshRenderer>().material.color = new Color( 1, 1, 1 );
	}

	private void PickupItem( GameObject item, Transform handtransform )
	{
		// Parent the newly held item and ensure it is in the right position
		Item[CurrentHand] = item;
		Vector3 rotation = Item[CurrentHand].transform.localEulerAngles;
		Item[CurrentHand].transform.position = handtransform.position;
		Item[CurrentHand].transform.parent = handtransform;
		Item[CurrentHand].transform.localEulerAngles = rotation;

		// Disable physics and collisions
		Item[CurrentHand].GetComponent<Rigidbody>().isKinematic = true;
		Item[CurrentHand].GetComponent<Collider>().enabled = false;
	}

	private void DropItem( Vector3 position )
	{
		// Enable physics and collisions
		Item[CurrentHand].GetComponent<Collider>().enabled = true;
		Item[CurrentHand].GetComponent<Rigidbody>().isKinematic = false;

		// Unparent from hand
		Vector3 rotation = Item[CurrentHand].transform.eulerAngles;
		Item[CurrentHand].transform.parent = null;
		Item[CurrentHand].transform.position = position;
		Item[CurrentHand].transform.eulerAngles = rotation;
		Item[CurrentHand] = null;

		// Cancel rotating if it was happening
		Rotating = false;
	}

	public GameObject[] GetHeldItems()
	{
		return Item;
	}

	public void SetHeldItem( int index, GameObject item )
	{
		Transform handtransform = transform.FindChild( HandTransformName[index] );
		PickupItem( item, handtransform );
	}
}