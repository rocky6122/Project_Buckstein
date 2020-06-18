////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  CameraScript : class | Written by Anthony Pascone and John Imgrund                                            //
//  Watched this video: https://youtu.be/xcn7hz7J7sI. Camera will rotate around an empty gameobject when you click//
//  middle mouse button and move when using WASD                                                                  //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class CameraScript : MonoBehaviour {

	//this is the video I watched to do the camera rotation
	//

	//Point of Rotation for the Camera
    GameObject gridMiddle;
	public float cameraMoveSpeedDivider;	//Divides the transform.Translate

    Camera cam;
    float scrollRate = 4f;
    float zoomMin = 15f;
    float zoomMax = 45f;

	//Camera Rotation
    bool rotate = false;
	public float rotateSpeed;
	public float smoothFactor;

	Vector3 camOffset;

    bool followUnit = false;

	void Start () {

        cam = Camera.main;

        //get the gameobject we want to rotate around and the camera offset from it
        gridMiddle = GameObject.Find("GridMid");

        //This needs to be set in start or else the original WASD controls will be inverted unless you rotate
        gridMiddle.transform.rotation = new Quaternion(transform.rotation.x, 0, transform.rotation.z, 0);
    }
	
	void FixedUpdate () {


		camOffset = transform.position - gridMiddle.transform.position;

        if (!GameManager.instance.IsChatting())
        {
            CheckInput();
            RotateCamera();
        }
        CameraFollowUnit();
	}

    /// <summary>
    /// Check if middle mouse button or WASD are being pressed and move the empty gameobject "GridMid "accordingly.
    /// </summary>
	void CheckInput()
	{
		if (Input.GetMouseButton (2)) {
			rotate = true;
		} else {
			rotate = false;
		}

        //This will move the area that the camera focuses on
        if (!followUnit)
        {
            if (Input.GetKey(KeyCode.W))
            {
                gridMiddle.transform.Translate(-Vector3.forward / cameraMoveSpeedDivider);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                gridMiddle.transform.Translate(Vector3.forward / cameraMoveSpeedDivider);
            }

            if (Input.GetKey(KeyCode.A))
            {
                gridMiddle.transform.Translate(-Vector3.right / cameraMoveSpeedDivider);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                gridMiddle.transform.Translate(Vector3.right / cameraMoveSpeedDivider);
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0f) //Zomming In
            {
                cam.fieldOfView -= scrollRate;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) //Zooming Out
            {
                cam.fieldOfView += scrollRate;
            }
        }

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, zoomMin, zoomMax);

    }

    /// <summary>
    /// Rotate the camera around the "GridMid" object based on the locationg of the mouse in the x-axis
    /// </summary>
	void RotateCamera()
	{
		//Rotate around the gameobject (gridmid)
		if (rotate) 
		{
			Quaternion camTurnAngle = Quaternion.AngleAxis (Input.GetAxis ("Mouse X") * rotateSpeed, Vector3.up);

			camOffset = camTurnAngle * camOffset;

			//Make camera rotate spot's rotation equal to the cameras so it is facing in the correct direction
			gridMiddle.transform.rotation = new Quaternion(transform.rotation.x,0,transform.rotation.z,0);
		}
		//Do this stuff outside of the if statement so the camera always follows the gridMiddle object
		Vector3 newPos = gridMiddle.transform.position + camOffset;
		transform.position = Vector3.Slerp (transform.position, newPos, smoothFactor);

		transform.LookAt (gridMiddle.transform);
	}

    /// <summary>
    /// Look at a specified Unit
    /// </summary>
    public void CameraFollowUnit()
    {
        if (followUnit)
        {
            int index = EnemyUnitManager.instance.GetUnitIndex();

            //Zoom in
            cam.fieldOfView = 20;

            //Make sure index isnt out of range
            if (index < EnemyUnitManager.instance.GetUnitList().Count)
            {
                //Lerp to enemy unit
                gridMiddle.transform.position = Vector3.Lerp(gridMiddle.transform.position, EnemyUnitManager.instance.GetUnitList()[index].transform.position, Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Set the camera follow bool
    /// </summary>
    /// <param name="follow"></param>
    public void SetCameraFollow(bool follow)
    {
        followUnit = follow;
    }
}
