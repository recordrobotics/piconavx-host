using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class Transform
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);

        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);

        public Vector3 Origin { get; set; } = new Vector3(0, 0, 0);

        public Quaternion Rotation { get; set; } = Quaternion.Identity;

        public Matrix4x4 Matrix => Matrix4x4.CreateTranslation(-Origin) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Origin) * Matrix4x4.CreateTranslation(Position);
    }
}
