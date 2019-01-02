using UnityEngine;
using System.Collections;

namespace LegendSailer
{
    //Controlls the water
    public class WaterController : MonoBehaviour
    {
        public static WaterController current;

        public bool isMoving;

        //Wave height and speed
        public float scale = 0.1f;
        public float speed = 1.0f;
        //The width between the waves
        public float waveDistance = 1f;
        //Noise parameters
        public float noiseStrength = 1f;
        public float noiseWalk = 1f;

        //need to use Awake instead of Start, this obj need to be reference by other class
        //If there all in start function, cannot ganrantee to get the reference ahead of time
        void Awake()
        {
            current = this;
        }

        //Get the y coordinate from whatever wavetype we are using
        public float GetWaveYPos(Vector3 position, float timeSinceStart)
        {
            if (isMoving)
            {
                return WaveTypes.SinXWave(position, speed, scale, waveDistance, noiseStrength, noiseWalk, timeSinceStart);
            }
            else
            {
                return 0f;
            }
        }

        //Find the distance from a vertice to water
        //Make sure the position is in global coordinates
        //Positive if above water
        //Negative if below water
        public float DistanceToWater(Vector3 position, float timeSinceStart)
        {
            float waterHeight = GetWaveYPos(position, timeSinceStart);

            float distanceToWater = position.y - waterHeight;

            return distanceToWater;
        }
    }

    //Different wavetypes
    public class WaveTypes
    {

        //Sinus waves
        public static float SinXWave(
            Vector3 position,
            float speed,
            float scale,
            float waveDistance,
            float noiseStrength,
            float noiseWalk,
            float timeSinceStart)
        {
            float x = position.x;
            float y = 0f;
            float z = position.z;

            //Using only x or z will produce straight waves
            //Using only y will produce an up/down movement
            //x + y + z rolling waves
            //x * z produces a moving sea without rolling waves

            float waveType = z;
            z += Mathf.PerlinNoise(x + noiseWalk, y + Mathf.Sin(timeSinceStart * 0.1f)) * noiseStrength;
            x += Mathf.Sin((timeSinceStart * speed + waveType) / waveDistance) * scale/2;
            y += Mathf.Sin((timeSinceStart * speed + waveType) / waveDistance) * scale;

            //Add noise to make it more realistic
            y += Mathf.PerlinNoise(x + noiseWalk, y + Mathf.Sin(timeSinceStart * 0.1f)) * noiseStrength;

            return y;
        }
    }

}
