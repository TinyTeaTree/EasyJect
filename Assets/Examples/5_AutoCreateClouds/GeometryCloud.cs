using EasyJect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Example_AutoCreateClouds
{
    [Cloud, AutoCreate]
    public class GeometryCloud
    {
        [Inject] CubeBehaviour Cube { get; set; }
        [Inject] SphereBehaviour Sphere { get; set; }

        public void CubePressed()
        {
            Sphere.Move();
        }

        public void SpherePressed()
        {
            Cube.Move();
        }
    }
}