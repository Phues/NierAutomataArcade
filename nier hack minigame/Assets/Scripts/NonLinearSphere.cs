using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class NonLinearSphere : Enemy
{
    public PathCreator pathCreator;
    private float distanceTraveled;

    public override void Move()
    {
        distanceTraveled += speed * Time.deltaTime;
        transform.position = pathCreator.path.GetPointAtDistance(distanceTraveled);
    }
}
