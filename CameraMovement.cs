namespace Mapbox.Examples
{
    using Mapbox.Unity.Map;
    using Mapbox.Unity.Utilities;
    using Mapbox.Utils;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class CameraMovement : MonoBehaviour
    {
        [SerializeField]
        float Speed; // how fast is the panning?

        public CameraBounds camBounds; // tile provider

        Vector3 _dragOrigin; // where the user presses down mouse
        Vector3 _cameraPosition; // location of main camera
        Vector3 _panOrigin; // where the user releases mouse

        private bool startedDrag = false; // is the user currently panning?

        void Update()
        {
            /*if (Input.GetKey(KeyCode.A))
			{
				transform.Translate(-1 * Speed * Time.deltaTime, 0, 0, Space.World);
			}

			if (Input.GetKey(KeyCode.W))
			{
				transform.Translate(0, 0, 1 * Speed * Time.deltaTime, Space.World);
			}

			if (Input.GetKey(KeyCode.S))
			{
				transform.Translate(0, 0, -1 * Speed * Time.deltaTime, Space.World);
			}

			if (Input.GetKey(KeyCode.D))
			{
				transform.Translate(1 * Speed * Time.deltaTime, 0, 0, Space.World);
			}
            */
            // Debug.Log(EventSystem.current.IsPointerOverGameObject());

            // can't pan if user is interacting with UI
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetMouseButtonDown(0)) // user pressed down to start panning
                {
                    startedDrag = true;
                    _cameraPosition = transform.position;
                    _panOrigin = Input.mousePosition;
                    camBounds.StartMove(); // tell tile provider that user is panning
                }

                if (startedDrag && Input.GetMouseButton(0))
                {
                    LeftMouseDrag(); // move the camera as user pans
                }
            }

            if (startedDrag && Input.GetMouseButtonUp(0)) // user is done panning
            {
                camBounds.EndMove();
                startedDrag = false;
            }
        }
        // TODO: add acceleration!
        void LeftMouseDrag()
        {
            Vector3 pos = Input.mousePosition - _panOrigin;

            // camera moves in opposite direction of mouse drag
            pos.z = pos.y;
            pos.y = 0;
            transform.position = _cameraPosition + -pos * Speed;
        }
    }
}