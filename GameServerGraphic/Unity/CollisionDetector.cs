using GameServer;
using System.Collections;

public class CollisionDetector/* : MonoBehaviour*/
{
	//Collider collider;
	//Vector3 StartPoint;
	//Vector3 Origin;
	//public int NoOfRays = 10;
	//RaycastHit HitInfo;
	//float LengthOfRay, DistanceBetweenRays;
	//float margin = 0.015f;
	//Ray ray;
	//public Vector3 moveDirection;
	//bool hasHit = false;
	//float lastTime;
	//float checkTime = 0.3f;
	//Vector3 startPointFromTransform;
	//float startXPoint;
	//int i;

	//void Start()
	//{
	//	//Length of the Ray is distance from center to edge
	//	collider = GetComponent<Collider>();
	//	//LengthOfRay = GetComponent<Collider>().bounds.extents.y;
	//	LengthOfRay = 3f;
	//	lastTime = Time.time;
	//}

	//public void IsColliding()
	//{
	//	StartPoint = new Vector3(collider.bounds.min.x + margin, transform.position.y, transform.position.z);
	//	Origin = StartPoint;
	//	DistanceBetweenRays = (collider.bounds.size.x - 2 * margin) / (NoOfRays - 1);
	//	for (i = 0; i < NoOfRays; i++)
	//	{
	//		// Ray to be casted.
	//		ray = new Ray(Origin, moveDirection);
	//		//Draw ray in scene view to see visually. Remember visual length is not actual length
	//		Debug.DrawRay(Origin, moveDirection * LengthOfRay, Color.yellow);
	//		if (Physics.Raycast(ray, out HitInfo, LengthOfRay))
	//		{
	//			if (HitInfo.transform.tag ==  "Player" && HitInfo.transform != transform)
	//			{
	//				//Debug.Log("Collided With " + transform.GetComponent<PlayerController>().isWalking);
	//				hasHit = true;
	//				startPointFromTransform = transform.position;
	//				startXPoint = collider.bounds.min.x + margin;
	//				if(HitInfo.transform.parent == transform.parent && HitInfo.transform.GetComponent<PlayerController>().currentState != STATE.Moving)
	//				{
	//					Debug.Log("Hier");
	//					//HitInfo.transform.GetComponent<PlayerController>().IsCollidedWithTroop();
	//				}
	//				// Negate the Directionfactor to reverse the moving direction of colliding cube(here cube2)
	//				break;
	//			}
	//		}
	//		Origin += new Vector3(DistanceBetweenRays, 0, 0);
	//	}
	//}

	//public void RayToEndPoint(Vector3 destination, float maxDistance)
	//{
	//	StartPoint = new Vector3(collider.bounds.min.x + margin, transform.position.y, transform.position.z);
	//	Origin = StartPoint;
	//	DistanceBetweenRays = (collider.bounds.size.x - 2 * margin) / (NoOfRays - 1);
	//	for (i = 0; i < NoOfRays; i++)
	//	{
	//		// Ray to be casted.
	//		ray = new Ray(Origin, destination-transform.position);
	//		//Draw ray in scene view to see visually. Remember visual length is not actual length
	//		Debug.DrawRay(Origin, (destination-transform.position).normalized * maxDistance, Color.yellow);
	//		if (Physics.Raycast(ray, out HitInfo, maxDistance))
	//		{
	//			/*if (HitInfo.transform.tag == "Player" && HitInfo.transform != transform)
	//			{
	//				Debug.Log("Collided With " + transform.GetComponent<PlayerController>().isWalking);
	//				hasHit = true;
	//				startPointFromTransform = transform.position;
	//				startXPoint = collider.bounds.min.x + margin;
	//				if (HitInfo.transform.parent == transform.parent && !HitInfo.transform.GetComponent<PlayerController>().isWalking)
	//				{
	//					Debug.Log("Hier");
	//					HitInfo.transform.GetComponent<PlayerController>().IsCollidedWithTroop();
	//				}
	//				// Negate the Directionfactor to reverse the moving direction of colliding cube(here cube2)
	//				break;
	//			}*/
	//		}
	//		Origin += new Vector3(DistanceBetweenRays, 0, 0);
	//	}
	//}
}

