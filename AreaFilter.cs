namespace Mapbox.Unity.MeshGeneration.Filters
{
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Data;

    [CreateAssetMenu(menuName = "Mapbox/Filters/Area Filter")]
    public class AreaFilter : FilterBase
    {
        public override string Key { get { return "area_id"; } }
        [SerializeField]
        private string _id;
        [SerializeField]
        private TypeFilterType _behaviour;

        public override bool Try(VectorFeatureUnity feature)
        {
            var check = _id.ToLowerInvariant().Contains(feature.Properties["area_id"].ToString().ToLowerInvariant());
            return _behaviour == TypeFilterType.Include ? check : !check;
        }

        public enum TypeFilterType
        {
            Include,
            Exclude
        }
    }
}