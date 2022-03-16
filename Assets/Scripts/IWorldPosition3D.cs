using UnityEngine;

public interface IWorldPosition3D
{
    Vector3 WorldPosition3D { get; }
    void SetSiblingIndexFor2D(int index);
}