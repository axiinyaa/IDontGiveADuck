using Unity.VisualScripting;
using UnityEngine;

namespace Axiinyaa.Tweening
{
    public enum Easing
    {
        Linear,
        EaseIn,
        EaseOut
    }


    public static class Tweening
    {
        public static float EaseIn(float t)
        {
            return t * t;
        }

        public static float EaseOut(float t)
        {
            return Flip(Square(Flip(t)));
        }

        public static float Square(float x)
        {
            return x * x;
        }

        public static float Flip(float x)
        {
            return 1 - x;
        }

        public static Vector2 Tween(this Vector2 v, Vector2 position, float durationSeconds, Easing easing)
        {
            if (easing == Easing.EaseIn)
            {
                return Vector2.Lerp(v, position, EaseIn(Time.deltaTime / durationSeconds));
            }

            if (easing == Easing.EaseOut)
            {
                return Vector2.Lerp(v, position, EaseOut(Time.deltaTime / durationSeconds));
            }

            // Linear
            return Vector2.Lerp(v, position, Time.deltaTime / durationSeconds);
        }

        public static void LerpRotation(Transform transform, Quaternion rotation, float durationSeconds, Easing easing)
        {
            if (easing == Easing.EaseIn)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, EaseIn(Time.deltaTime / durationSeconds));
                return;
            }

            if (easing == Easing.EaseOut)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, EaseOut(Time.deltaTime / durationSeconds));
                return;
            }

            // Linear
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime / durationSeconds);
        }

        public static void LerpScale(Transform transform, Vector2 scale, float durationSeconds, Easing easing)
        {
            if (easing == Easing.EaseIn)
            {
                transform.localScale = Vector2.Lerp(transform.localScale, scale, EaseIn(Time.deltaTime / durationSeconds));
                return;
            }

            if (easing == Easing.EaseOut)
            {
                transform.localScale = Vector2.Lerp(transform.localScale, scale, EaseOut(Time.deltaTime / durationSeconds));
                return;
            }

            // Linear
            transform.localScale = Vector2.Lerp(transform.localScale, scale, Time.deltaTime / durationSeconds);
        }
    }
}