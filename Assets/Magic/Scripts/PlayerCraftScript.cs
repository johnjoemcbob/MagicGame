using UnityEngine;
using System.Collections;

// Matthew Cormack
// 31/05/2015
//
// Location:
// - Created by the PlayerPickupScript on the player camera controller
//
// Functionality:
// - Allows crafting of items by combining two held items from PlayerPickupScript
//
// Requirements:
// - Requires the PlayerPickupScript for the picking up item functionality
//
// Buttons:
// - Combine - default r - attempt to combine the two items held

public struct CraftDescriptionStruct
{
	public string OutputPrefabName; // name of the resulting combination to spawn
	public string[] InputItemName; // 2 elements, left and right item names
	public Vector3[] InputItemDirection; // 2 elements, left and right item orientations for the combination
	public bool Reversed; // Flag for use when comparing held items, true if they are in the reverse order of that described
}

public class PlayerCraftScript : MonoBehaviour
{
	// Reference to the associated PlayerPickupScript; which handles the items held
	public PlayerPickupScript PlayerPickup;

	// Array of possible item combinations; contains members of type CraftDescriptionStruct
	private ArrayList CraftDescription = new ArrayList();

	// Use this for initialization
	void Start()
	{
		// Recipe listings
		AddCraftingDescrition( "Sphere", "Cube", "Coin", new Vector3( 0, 1, 0 ), new Vector3( 1, 0, 0 ) );
	}

	// Update is called once per frame
	void Update()
	{
		// Attemp to combine
		if ( Input.GetButtonDown( "Combine" ) )
		{
			// Check that two items are held
			GameObject itemleft = PlayerPickup.GetHeldItems()[0];
			GameObject itemright = PlayerPickup.GetHeldItems()[1];
			if ( itemleft && itemright )
			{
				PickupDescriptionScript descriptionleft = itemleft.GetComponent<PickupDescriptionScript>();
				PickupDescriptionScript descriptionright = itemright.GetComponent<PickupDescriptionScript>();

				// Check both items to find crafts they appear in together
				ArrayList matchingcraft = GetMatchingCraftDescriptions( descriptionleft.Name, descriptionright.Name );
				foreach ( object CraftObject in matchingcraft )
				{
					CraftDescriptionStruct description = (CraftDescriptionStruct) CraftObject;
					if (
						( Vector3.Distance( itemleft.transform.up, description.InputItemDirection[0] ) < 0.3f ) &&
						( Vector3.Distance( itemright.transform.up, description.InputItemDirection[1] ) < 0.3f )
					)
					{
						Destroy( itemleft );
						Destroy( itemright );
						PlayerPickup.SetHeldItem( 0, (GameObject) Instantiate( Resources.Load( description.OutputPrefabName ) ) );
					}
				}
			}
		}
	}

	private void AddCraftingDescrition( string outputname, string inputnameleft, string inputnameright, Vector3 inputdirectionleft, Vector3 inputdirectionright )
	{
		CraftDescription.Add( new CraftDescriptionStruct() );
		{
			int element = CraftDescription.Count - 1;
			CraftDescriptionStruct newcraftingdescription = (CraftDescriptionStruct) CraftDescription[element];
			{
				newcraftingdescription.OutputPrefabName = outputname;
				newcraftingdescription.InputItemName = new string[2] { inputnameleft, inputnameright };
				newcraftingdescription.InputItemDirection = new Vector3[2] { inputdirectionleft, inputdirectionright };
			}
			CraftDescription[element] = newcraftingdescription;
		}
	}

	private ArrayList GetMatchingCraftDescriptions( string left, string right )
	{
		ArrayList result = new ArrayList();
		{
			// Every crafting description
			foreach ( object CraftObject in CraftDescription )
			{
				CraftDescriptionStruct description = (CraftDescriptionStruct) CraftObject;
				// Find those containing both held items
				if ( ( description.InputItemName[0] == left ) && ( description.InputItemName[1] == right ) )
				{
					description.Reversed = false;
					result.Add( description );
				}
				else if ( ( description.InputItemName[0] == right ) && ( description.InputItemName[1] == left ) )
				{
					description.Reversed = true;
					result.Add( description );
				}
			}
		}
		return result;
	}
}