using piconavx.ui.graphics.ui;
using System.Numerics;

namespace piconavx.ui.graphics
{
    public class Transform
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);

        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);

        public static Vector3 GlobalScale { get; set; } = new Vector3(1, 1, 1);

        public bool UseGlobalScale { get; set; } = false;

        private Vector3 origin = new Vector3(0, 0, 0);
        public Vector3 Origin
        {
            get => origin; set
            {
                _hasCache = false;
                origin = value;
            }
        }

        public bool HasCache { get => _hasCache; }

        private UIController? _cacheController;
        private Vector2 _cacheOrigin;
        private bool _hasCache = false;

        public void UpdateCache()
        {
            if (_hasCache && _cacheController != null)
            {
                origin = new Vector3(_cacheController.Bounds.X + _cacheController.Bounds.Width * _cacheOrigin.X, _cacheController.Bounds.Y + _cacheController.Bounds.Height * _cacheOrigin.Y, 0);
            }
        }

        public void SetOrigin(UIController controller, Vector2 origin)
        {
            _hasCache = true;
            _cacheController = controller;
            _cacheOrigin = origin;
            UpdateCache();
        }

        public Quaternion Rotation { get; set; } = Quaternion.Identity;

        public Matrix4x4 Matrix => UseGlobalScale ? (Matrix4x4.CreateTranslation(-Origin) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Origin) * Matrix4x4.CreateScale(GlobalScale) * Matrix4x4.CreateTranslation(Position)) : LocalMatrix;

        public Matrix4x4 LocalMatrix => Matrix4x4.CreateTranslation(-Origin) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Origin) * Matrix4x4.CreateTranslation(Position);
    }
}
