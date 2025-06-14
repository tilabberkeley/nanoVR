using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Pointer
{
    private float x, y, z;
    private Vector3 position;

    public Pointer() {
        this.x = 0;
        this.y = 0;
        this.z = 0;
    }
    
    public Pointer(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
        position = new Vector3(x, y, z);
    }

    public float getX() {
        return this.x;
    }

    public float getY() {
        return this.y;
    }

    public float getZ() {
        return this.z;
    }

    public Vector3 getPosition() {
        return this.position;
    }

    public void setX(int x) {
        this.x = x;
    }

    public void setY(int y) {
        this.y = y;
    }

    public void setZ(int z) {
        this.z = z;
    }

    public void setCoordinate(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public string toString() {
        return "(" + getX() + ", " + getY() + ", " + getZ() + ")";
    }
}