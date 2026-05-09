using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	public LayerMask collisionMask;
	
	public const float skinWidth = 0.01f; //0.015f
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;

	[HideInInspector]
	public BoxCollider2D boxcollider;
	public RaycastOrigins raycastOrigins;

	public virtual void Awake() {
		boxcollider = GetComponent<BoxCollider2D> ();
	}

	public virtual void Start() {
		CalculateRaySpacing ();
	}

	public void UpdateRaycastOrigins() {
		Bounds bounds = boxcollider.bounds;
		bounds.Expand (skinWidth * -2);
		
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}
	
	public void CalculateRaySpacing() {
		Bounds bounds = boxcollider.bounds;
		bounds.Expand (skinWidth * -2);
		
		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);
		
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}
	
	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

    // Aggiungi questo alla fine di RaycastController.cs
    void OnDrawGizmos()
    {
        if (boxcollider == null)
        {
            boxcollider = GetComponent<BoxCollider2D>();
        }

        // Disegna i bordi del calcolo collisioni (Skin Width)
        Bounds bounds = boxcollider.bounds;
        bounds.Expand(skinWidth * -2);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        // Disegna dei piccoli cubi nei punti di origine dei raggi
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(raycastOrigins.bottomLeft, 0.05f);
            Gizmos.DrawSphere(raycastOrigins.bottomRight, 0.05f);
            Gizmos.DrawSphere(raycastOrigins.topLeft, 0.05f);
            Gizmos.DrawSphere(raycastOrigins.topRight, 0.05f);
        }
    }
}
