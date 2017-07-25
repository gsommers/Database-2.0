//-----------------------------------------------------------------------
// <copyright file="RasterTileExample.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mapbox.Examples.Playground {
	using System.Linq;
	using System;
	using Mapbox.Map;
	using Mapbox.Unity;
	using UnityEngine;
	using UnityEngine.UI;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;

	public class Search : MonoBehaviour {
		[SerializeField]
		ForwardGeocodeUserInput _searchLocation;

		Map<RasterTile> _map;

		[Geocode]
		[SerializeField]
		string _latLon;


		// start location - San Francisco
		Vector2d _startLoc = new Vector2d();

		void Awake() {
			_searchLocation.OnGeocoderResponse += SearchLocation_OnGeocoderResponse;
			var parsed = _latLon.Split(',');
			_startLoc.x = double.Parse(parsed[0]);
			_startLoc.y = double.Parse(parsed[1]);
		}

		void OnDestroy() {
			if (_searchLocation != null) {
				_searchLocation.OnGeocoderResponse -= SearchLocation_OnGeocoderResponse;
			}
		}

		void Start() {
			_map = new Map<RasterTile>(MapboxAccess.Instance);
			_map.Center = _startLoc;
			//_map.Subscribe(this);
			_map.Update();
		}

		/// <summary>
		/// New search location has become available, begin a new _map query.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void SearchLocation_OnGeocoderResponse(object sender, EventArgs e) {
			_map.Center = _searchLocation.Coordinate;
			_map.Update();
		}

	}
}