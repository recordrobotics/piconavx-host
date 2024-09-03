using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public static class MathHelper
    {
        public static float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }

        public static Quaternion CreateLookRotation(Vector3 forward, Vector3 up)
        {
            float dot = Vector3.Dot(-Vector3.UnitZ, forward);

            if (MathF.Abs(dot + 1.0f) < 0.000001f)
            {
                return Quaternion.CreateFromAxisAngle(up, MathF.PI);
            }
            if (MathF.Abs(dot - 1.0f) < 0.000001f)
            {
                return Quaternion.Identity;
            }

            return Quaternion.CreateFromAxisAngle(
                Vector3.Normalize(Vector3.Cross(-Vector3.UnitZ, forward)),
                (float)Math.Acos(dot)
                );
        }

        public static Matrix4x4 CreatePerspectiveFieldOfViewOffCenter(float fieldOfView, float aspectRatio, float offsetX, float offsetY, float nearPlaneDistance, float farPlaneDistance)
        {
            offsetX *= aspectRatio * nearPlaneDistance;
            offsetY *= nearPlaneDistance;

            float top = MathF.Tan(fieldOfView * 0.5f) * nearPlaneDistance;
            float bottom = -top;
            float left = aspectRatio * bottom;
            float right = aspectRatio * top;
            return Matrix4x4.CreatePerspectiveOffCenter(left + offsetX, right + offsetX, bottom + offsetY, top + offsetY, nearPlaneDistance, farPlaneDistance);
        }
    }
}
