using System.Numerics;

namespace piconavx.ui.graphics
{
    public class Transform
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);

        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);

        public static Vector3 GlobalScale { get; set; } = new Vector3(1, 1, 1);

        public bool UseGlobalScale { get; set; } = false;

        public Vector3 Origin { get; set; } = new Vector3(0, 0, 0);

        public Quaternion Rotation { get; set; } = Quaternion.Identity;

        public Matrix4x4 Matrix => UseGlobalScale ? (Matrix4x4.CreateTranslation(-Origin) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Origin) * Matrix4x4.CreateScale(GlobalScale) * Matrix4x4.CreateTranslation(Position)) : LocalMatrix;
        
        public Matrix4x4 LocalMatrix => Matrix4x4.CreateTranslation(-Origin) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Origin) * Matrix4x4.CreateTranslation(Position);
    }
}
