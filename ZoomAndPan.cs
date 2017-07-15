namespace Mapbox.Unity.Map
{
    using System;
    using Mapbox.Unity.MeshGeneration;
    using Mapbox.Unity.Utilities;
    using Utils;
    using UnityEngine;
    using Mapbox.Map;
    using UnityEngine.UI;
    using System.Collections;

    // TODO: make abstract! For example: MapFromFile, MapFromLocationProvider, etc.
    public class ZoomAndPan : MonoBehaviour, IMap
    {
        [Geocode]
        [SerializeField]
        string _latitudeLongitudeString;

        int _zoom;
        public int Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                _zoom = value;
            }
        }

        public Slider zoomSlide;
        [SerializeField]
        Transform _root;
        public Transform Root
        {
            get
            {
                return _root;
            }
        }

        [SerializeField]
        CameraBoundsTileProvider _tileProvider;

        [SerializeField]
        MapVisualizer _mapVisualizer;

        [SerializeField]
        float _unityTileSize = 100;

        MapboxAccess _fileSouce;

        Vector2d _mapCenterLatitudeLongitude;
        public Vector2d CenterLatitudeLongitude
        {
            get
            {
                return _mapCenterLatitudeLongitude;
            }
            set
            {
                _latitudeLongitudeString = string.Format("{0}, {1}", value.x, value.y);
                _mapCenterLatitudeLongitude = value;
            }
        }

        Vector2d _mapCenterMercator;
        public Vector2d CenterMercator
        {
            get
            {
                return _mapCenterMercator;
            }
        }

        float _worldRelativeScale;
        public float WorldRelativeScale
        {
            get
            {
                return _worldRelativeScale;
            }
        }

        public event Action OnInitialized = delegate { };

        [SerializeField]
        Examples.ForwardGeocodeUserInput _searchLocation;

        [Geocode]
        [SerializeField]
        string _latLon;

        void OnDestroy()
        {
            if (_searchLocation != null)
            {
                _searchLocation.OnGeocoderResponse -= SearchLocation_OnGeocoderResponse;
            }
        }

        public float zoomWait;
        private float elapsedTime = 0;

        /// <summary>
        /// New search location has become available, begin a new _map query.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void SearchLocation_OnGeocoderResponse(object sender, EventArgs e)
        {
            if (_mapCenterLatitudeLongitude.Equals(_searchLocation.Coordinate))
            {
                Debug.Log("Same");
                return;
            }
            _mapCenterLatitudeLongitude = _searchLocation.Coordinate;
            Debug.Log(_searchLocation.Coordinate + " search");
            SlideZoom();
        }

    protected virtual void Awake()
        {
          if (_searchLocation != null)
        _searchLocation.OnGeocoderResponse += SearchLocation_OnGeocoderResponse;
        _fileSouce = MapboxAccess.Instance;
            _tileProvider.OnTileAdded += TileProvider_OnTileAdded;
            _tileProvider.OnTileRemoved += TileProvider_OnTileRemoved;
            if (!_root)
            {
                _root = transform;
            }
        }

        /*protected virtual void OnDestroy()
        {
            if (_tileProvider != null)
            {
                _tileProvider.OnTileAdded -= TileProvider_OnTileAdded;
                _tileProvider.OnTileRemoved -= TileProvider_OnTileRemoved;
            }

            _mapVisualizer.Destroy();
        }*/

        // This is the part that is abstract?
        protected virtual void Start()
        {
            var latLonSplit = _latitudeLongitudeString.Split(',');
            _mapCenterLatitudeLongitude = new Vector2d(double.Parse(latLonSplit[0]), double.Parse(latLonSplit[1]));
            Zoom = 5;
            Setup();
        }

        void Setup()
        {
            var referenceTileRect = Conversions.TileBounds(TileCover.CoordinateToTileId(_mapCenterLatitudeLongitude, _zoom));
            _mapCenterMercator = referenceTileRect.Center;

            _worldRelativeScale = (float)(_unityTileSize / referenceTileRect.Size.x);
            Root.localScale = Vector3.one * _worldRelativeScale;

            _mapVisualizer.Initialize(this, _fileSouce);
            _tileProvider.Initialize(this);

            OnInitialized();
        }

       void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > zoomWait)
            {
                elapsedTime = 0;
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    zoomSlide.value++;
                    SlideZoom();
                }

                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    zoomSlide.value--;
                    SlideZoom();
                }
            }
        }

        public void SlideZoom()
        {
            zoomSlide.value = Mathf.Clamp(zoomSlide.value, 5, 18);
            _tileProvider.UpdateZoom(zoomSlide.value);
            // Debug.Log(_mapCenterLatitudeLongitude + " zoom");
            Zoom = (int)zoomSlide.value;
            Setup();
            // Debug.Log(_mapCenterLatitudeLongitude + " setup");
            
        }

        public void SetCenter(Vector2d coords)
        {
            _mapCenterLatitudeLongitude = coords;
            // Debug.Log(_mapCenterMercator);
            Debug.Log(_mapCenterLatitudeLongitude + " center");
        }

        void TileProvider_OnTileAdded(UnwrappedTileId tileId)
        {
            _mapVisualizer.LoadTile(tileId);
        }

        void TileProvider_OnTileRemoved(UnwrappedTileId tileId)
        {
            _mapVisualizer.DisposeTile(tileId);
        }
    }
}