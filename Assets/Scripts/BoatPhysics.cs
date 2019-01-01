using System.Collections.Generic;
using UnityEngine;

namespace LegendSailer
{
    public class BoatPhysics : MonoBehaviour
    {
        //水下部分只是为了染色，看起来方便
        //对漂浮本身不起作用
        public GameObject underWaterObj;
        public GameObject boatMeshObj;
        public GameObject aboveWaterObj;

        public float boatDrag = 0.5f;

        //Script that's doing everything needed with the boat mesh, such as finding out which part is above the water
        private ModifyBoatMesh modifyBoatMesh;

        public Vector3 centerOfMass;
        //Mesh for debugging
        private Mesh underWaterMesh;
        private Mesh aboveWaterMesh;
        //The boats rigidbody
        private Rigidbody boatRB;

        //The density of the water the boat is traveling in
        private float rhoWater = BoatPhysicsMath.RHO_OCEAN_WATER;
        private float rhoAir = BoatPhysicsMath.RHO_AIR;

        void Awake()
        {
            boatRB = this.GetComponent<Rigidbody>();
        }

        private void Start()
        {
            //Init the script that will modify the boat mesh
            modifyBoatMesh = new ModifyBoatMesh(boatMeshObj, underWaterObj, aboveWaterObj, boatRB);

            //Meshes that are below and above the water
            underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
            aboveWaterMesh = aboveWaterObj.GetComponent<MeshFilter>().mesh;
        }

        private void Update()
        {
            //Generate the under water mesh
            modifyBoatMesh.GenerateUnderwaterMesh();

            //Display the under water mesh
            modifyBoatMesh.DisplayMesh(underWaterMesh, "UnderWater Mesh", modifyBoatMesh.underWaterTriangleData);
        }

        private void FixedUpdate()
        {
            //Change the center of mass - experimental - move to Start() later
            boatRB.centerOfMass = centerOfMass;

            //Add forces to the part of the boat that's below the water
            if (modifyBoatMesh.underWaterTriangleData.Count > 0)
            {
                AddUnderWaterForces();
            }
            //Add forces to the part of the boat that's above the water
            if (modifyBoatMesh.aboveWaterTriangleData.Count > 0)
            {
                AddAboveWaterForces();
            }
        }

        //Add all forces that act on the squares below the water
        void AddUnderWaterForces()
        {
            //The resistance coefficient - same for all triangles
            float Cf = BoatPhysicsMath.ResistanceCoefficient(
                rhoWater,
                boatRB.velocity.magnitude,
                modifyBoatMesh.CalculateUnderWaterLength());

            //To calculate the slamming force we need the velocity at each of the original triangles
            List<SlammingForceData> slammingForceData = modifyBoatMesh.slammingForceData;

            CalculateSlammingVelocities(slammingForceData);

            //Need this data for slamming forces
            float boatArea = modifyBoatMesh.boatArea;
            float boatMass = boatRB.mass; //Replace this line with your boat's total mass

            //To connect the submerged triangles with the original triangles
            List<int> indexOfOriginalTriangle = modifyBoatMesh.indexOfOriginalTriangle;

            //Get all triangles
            List<TriangleData> underWaterTriangleData = modifyBoatMesh.underWaterTriangleData;

            for (int i = 0; i < underWaterTriangleData.Count; i++)
            {
                TriangleData triangleData = underWaterTriangleData[i];


                //Calculate the forces
                Vector3 forceToAdd = Vector3.zero;

                //Force 1 - The hydrostatic force (buoyancy)
                forceToAdd += BoatPhysicsMath.BuoyancyForce(rhoWater, triangleData);

                //Force 2 - Viscous Water Resistance
                forceToAdd += BoatPhysicsMath.ViscousWaterResistanceForce(rhoWater, triangleData, Cf);

                //Force 3 - Pressure drag
                forceToAdd += BoatPhysicsMath.PressureDragForce(triangleData);

                //Force 4 - Slamming force
                //Which of the original triangles is this triangle a part of
                int originalTriangleIndex = indexOfOriginalTriangle[i];

                SlammingForceData slammingData = slammingForceData[originalTriangleIndex];

                forceToAdd += BoatPhysicsMath.SlammingForce(slammingData, triangleData, boatArea, boatMass);


                //Add the forces to the boat
                boatRB.AddForceAtPosition(forceToAdd, triangleData.center);


                //Debug

                //Normal
                //Debug.DrawRay(triangleData.center, triangleData.normal * 3f, Color.white);

                //Buoyancy
                //Debug.DrawRay(triangleData.center, BoatPhysicsMath.BuoyancyForce(rhoWater, triangleData).normalized * -3f, Color.blue);

                //Velocity
                //Debug.DrawRay(triangleCenter, triangleVelocityDir * 3f, Color.black);

                //Viscous Water Resistance
                //Debug.DrawRay(triangleCenter, viscousWaterResistanceForce.normalized * 3f, Color.black);
            }
        }

        //Add all forces that act on the squares above the water
        void AddAboveWaterForces()
        {
            //Get all triangles
            List<TriangleData> aboveWaterTriangleData = modifyBoatMesh.aboveWaterTriangleData;

            //Loop through all triangles
            for (int i = 0; i < aboveWaterTriangleData.Count; i++)
            {
                TriangleData triangleData = aboveWaterTriangleData[i];


                //Calculate the forces
                Vector3 forceToAdd = Vector3.zero;

                //Force 1 - Air resistance 
                //Replace VisbyData.C_r with your boat's drag coefficient
                forceToAdd += BoatPhysicsMath.AirResistanceForce(rhoAir, triangleData, boatDrag);

                //Add the forces to the boat
                boatRB.AddForceAtPosition(forceToAdd, triangleData.center);


                //Debug

                //The normal
                //Debug.DrawRay(triangleCenter, triangleNormal * 3f, Color.white);

                //The velocity
                //Debug.DrawRay(triangleCenter, triangleVelocityDir * 3f, Color.black);

                if (triangleData.cosTheta > 0f)
                {
                    //Debug.DrawRay(triangleCenter, triangleVelocityDir * 3f, Color.black);
                }

                //Air resistance
                //-3 to show it in the opposite direction to see what's going on
                //Debug.DrawRay(triangleCenter, airResistanceForce.normalized * -3f, Color.blue);
            }
        }

        //The buoyancy force so the boat can float
        private Vector3 BuoyancyForce(float rho, TriangleData triangleData)
        {
            //Buoyancy is a hydrostatic force - it's there even if the water isn't flowing or if the boat stays still

            // F_buoyancy = rho * g * V
            // rho - density of the mediaum you are in
            // g - gravity
            // V - volume of fluid directly above the curved surface

            // V = z * S * n
            // z - distance to surface
            // S - surface area
            // n - normal to the surface
            Vector3 buoyancyForce = rho * Physics.gravity.y * triangleData.distanceToSurface * triangleData.area * triangleData.normal;

            //The vertical component of the hydrostatic forces don't cancel out but the horizontal do
            buoyancyForce.x = 0f;
            buoyancyForce.z = 0f;

            return buoyancyForce;
        }

        //Calculate the current velocity at the center of each triangle of the original boat mesh
        private void CalculateSlammingVelocities(List<SlammingForceData> slammingForceData)
        {
            for (int i = 0; i < slammingForceData.Count; i++)
            {
                //Set the new velocity to the old velocity
                slammingForceData[i].previousVelocity = slammingForceData[i].velocity;

                //Center of the triangle in world space
                Vector3 center = transform.TransformPoint(slammingForceData[i].triangleCenter);

                //Get the current velocity at the center of the triangle
                slammingForceData[i].velocity = BoatPhysicsMath.GetTriangleVelocity(boatRB, center);
            }
        }
    }
}