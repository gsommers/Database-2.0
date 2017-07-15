namespace Mapbox.Unity.Map
{
	using UnityEngine;
	using Mapbox.Map;
	using Mapbox.Unity.Utilities;
	using Mapbox.Utils;
    using UnityEngine.UI;
    using System.Collections;

    public class CameraBoundsTileProvider : AbstractTileProvider
	{
		[SerializeField]
		Camera _camera;

		// TODO: change to Vector4 to optimize for different aspect ratios.
		[SerializeField]
		int _visibleBuffer;

        [SerializeField]
        ZoomAndPan zoompan;

		[SerializeField]
		int _disposeBuffer;

		[SerializeField]
		float _updateInterval;

		Plane _groundPlane;
		Ray _ray;
		float _hitDistance;
		Vector3 _viewportTarget;
		float _elapsedTime;
		bool _shouldUpdate;

		Vector2d _currentLatitudeLongitude;
		UnwrappedTileId _cachedTile;
		UnwrappedTileId _currentTile;

		internal override void OnInitialized()
		{
			_groundPlane = new Plane(Vector3.up, Mapbox.Unity.Constants.Math.Vector3Zero);
			_viewportTarget = new Vector3(0.5f, 0.5f, 0);
			_shouldUpdate = true;
            StartCoroutine(UpdateCo());

		}

		IEnumerator UpdateCo()
		{
            while (true)
            {
                if (!_shouldUpdate)
                {
                    yield return null;
                }
                _ray = _camera.ViewportPointToRay(_viewportTarget);
               
                if (_groundPlane.Raycast(_ray, out _hitDistance))
                {
                    _currentLatitudeLongitude = _ray.GetPoint(_hitDistance).GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale); // is this correct??
                                                                                                                                          // _currentLatitudeLongitude = _camera.ScreenToWorldPoint(new Vector3(512, 384, 0)).GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
                    
                    zoompan.SetCenter(_currentLatitudeLongitude);
                    _currentTile = TileCover.CoordinateToTileId(_currentLatitudeLongitude, _map.Zoom);

                    if (!_currentTile.Equals(_cachedTile))
                    {
                        
                        // FIXME: this results in bugs at world boundaries! Does not cleanly wrap. Negative tileIds are bad.
                        for (int x = Mathd.Max(_currentTile.X - _visibleBuffer, 0); x <= (_currentTile.X + _visibleBuffer); x++)
                        {
                            for (int y = Mathd.Max(_currentTile.Y - _visibleBuffer, 0); y <= (_currentTile.Y + _visibleBuffer); y++)
                            {
                                AddTile(new UnwrappedTileId(_map.Zoom, x, y));
                            }
                        }
                        _cachedTile = _currentTile;
                        Cleanup(_currentTile);
                    }
                    
                }
                yield return new WaitForSeconds(_updateInterval);
            }
		}

        public Vector2d UpdateZoom(float zoom)
        {
            _map.Zoom = (int)zoom;
            var count = _activeTiles.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var tile = _activeTiles[i];
                RemoveTile(tile);
            }
            return _currentLatitudeLongitude;
        }

        void Cleanup(UnwrappedTileId currentTile)
		{
			var count = _activeTiles.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				var tile = _activeTiles[i];
				bool dispose = false;
				dispose = tile.X > currentTile.X + _disposeBuffer || tile.X < _currentTile.X - _disposeBuffer;
				dispose = dispose || tile.Y > _currentTile.Y + _disposeBuffer || tile.Y < _currentTile.Y - _disposeBuffer;

				if (dispose)
				{
					RemoveTile(tile);
				}
			}
		}
	}
}
