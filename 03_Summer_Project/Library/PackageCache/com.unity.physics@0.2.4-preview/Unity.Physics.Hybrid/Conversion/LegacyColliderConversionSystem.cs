using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using LegacyPhysics = UnityEngine.Physics;
using LegacyCollider = UnityEngine.Collider;
using LegacyBox = UnityEngine.BoxCollider;
using LegacyCapsule = UnityEngine.CapsuleCollider;
using LegacyMesh = UnityEngine.MeshCollider;
using LegacySphere = UnityEngine.SphereCollider;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Physics.Authoring
{
    public abstract class BaseLegacyColliderConversionSystem<T> : BaseShapeConversionSystem<T> where T : LegacyCollider
    {
        static readonly IReadOnlyDictionary<PhysicMaterialCombine, Material.CombinePolicy> k_MaterialCombineLookup =
            new Dictionary<PhysicMaterialCombine, Material.CombinePolicy>
            {
                { PhysicMaterialCombine.Average, Material.CombinePolicy.ArithmeticMean },
                { PhysicMaterialCombine.Maximum, Material.CombinePolicy.Maximum },
                { PhysicMaterialCombine.Minimum, Material.CombinePolicy.Minimum }
            };

        static PhysicMaterial DefaultMaterial
        {
            get
            {
                if (s_DefaultMaterial == null)
                    s_DefaultMaterial = new PhysicMaterial { hideFlags = HideFlags.DontSave };
                return s_DefaultMaterial;
            }
        }
        static PhysicMaterial s_DefaultMaterial;

        protected static Material ProduceMaterial(LegacyCollider collider)
        {
            var material = new Material
            {
                // n.b. need to manually opt in to collision events with legacy colliders if desired
                Flags = collider.isTrigger
                    ? Material.MaterialFlags.IsTrigger
                    : new Material.MaterialFlags()
            };

            var legacyMaterial = collider.sharedMaterial;
            if (legacyMaterial == null)
                legacyMaterial = DefaultMaterial;

            material.Friction = legacyMaterial.dynamicFriction;
            if (k_MaterialCombineLookup.TryGetValue(legacyMaterial.frictionCombine, out var combine))
                material.FrictionCombinePolicy = combine;
            else
                Debug.LogWarning(
                    $"{collider.name} uses {legacyMaterial.name}, which specifies non-convertible mode {legacyMaterial.frictionCombine} for {nameof(PhysicMaterial.frictionCombine)}.",
                    collider
                );

            material.Restitution = legacyMaterial.bounciness;
            if (k_MaterialCombineLookup.TryGetValue(legacyMaterial.bounceCombine, out combine))
                material.RestitutionCombinePolicy = combine;
            else
                Debug.LogWarning(
                    $"{collider.name} uses {legacyMaterial.name}, which specifies non-convertible mode {legacyMaterial.bounceCombine} for {nameof(PhysicMaterial.bounceCombine)}.",
                    collider
                );

            return material;
        }

        protected static CollisionFilter ProduceCollisionFilter(LegacyCollider collider)
        {
            var layer = collider.gameObject.layer;
            var filter = new CollisionFilter { BelongsTo = (uint)(1 << collider.gameObject.layer) };
            for (var i = 0; i < 32; ++i)
                filter.CollidesWith |= (uint)(LegacyPhysics.GetIgnoreLayerCollision(layer, i) ? 0 : 1 << i);
            return filter;
        }

        protected override bool ShouldConvertShape(T shape) => shape.enabled;
        protected override GameObject GetPrimaryBody(T shape) => shape.GetPrimaryBody();
    }

    public sealed class LegacyBoxColliderConversionSystem : BaseLegacyColliderConversionSystem<LegacyBox>
    {
        protected override BlobAssetReference<Collider> ProduceColliderBlob(LegacyBox shape)
        {
            var worldCenter = math.mul(shape.transform.localToWorldMatrix, new float4(shape.center, 1f));
            var shapeFromWorld = math.inverse(
                new float4x4(new RigidTransform(shape.transform.rotation, shape.transform.position))
            );

            var geometry = new BoxGeometry
            {
                Center = math.mul(shapeFromWorld, worldCenter).xyz,
                Orientation = quaternion.identity
            };

            var linearScale = (float3)shape.transform.lossyScale;
            geometry.Size = math.abs(shape.size * linearScale);

            geometry.BevelRadius = math.min(ConvexHullGenerationParameters.Default.BevelRadius, math.cmin(geometry.Size) * 0.5f);

            return BoxCollider.Create(
                geometry,
                ProduceCollisionFilter(shape),
                ProduceMaterial(shape)
            );
        }
    }

    public sealed class LegacyCapsuleColliderConversionSystem : BaseLegacyColliderConversionSystem<LegacyCapsule>
    {
        protected override BlobAssetReference<Collider> ProduceColliderBlob(LegacyCapsule shape)
        {
            var linearScale = (float3)shape.transform.lossyScale;

            // radius is max of the two non-height axes
            var radius = shape.radius * math.cmax(new float3(math.abs(linearScale)) { [shape.direction] = 0f });

            var ax = new float3 { [shape.direction] = 1f };
            var vertex = ax * (0.5f * shape.height);
            var rt = new RigidTransform(shape.transform.rotation, shape.transform.position);
            var worldCenter = math.mul(shape.transform.localToWorldMatrix, new float4(shape.center, 0f));
            var offset = math.mul(math.inverse(new float4x4(rt)), worldCenter).xyz - shape.center * math.abs(linearScale);

            var v0 = offset + ((float3)shape.center + vertex) * math.abs(linearScale) - ax * radius;
            var v1 = offset + ((float3)shape.center - vertex) * math.abs(linearScale) + ax * radius;

            return CapsuleCollider.Create(
                new CapsuleGeometry { Vertex0 = v0, Vertex1 = v1, Radius = radius },
                ProduceCollisionFilter(shape),
                ProduceMaterial(shape)
            );
        }
    }

    public sealed class LegacySphereColliderConversionSystem : BaseLegacyColliderConversionSystem<LegacySphere>
    {
        protected override BlobAssetReference<Collider> ProduceColliderBlob(LegacySphere shape)
        {
            var worldCenter = math.mul(shape.transform.localToWorldMatrix, new float4(shape.center, 1f));
            var shapeFromWorld = math.inverse(
                new float4x4(new RigidTransform(shape.transform.rotation, shape.transform.position))
            );
            var center = math.mul(shapeFromWorld, worldCenter).xyz;

            var linearScale = (float3)shape.transform.lossyScale;
            var radius = shape.radius * math.cmax(math.abs(linearScale));

            return SphereCollider.Create(
                new SphereGeometry { Center = center, Radius = radius },
                ProduceCollisionFilter(shape),
                ProduceMaterial(shape)
            );
        }
    }

    public sealed class LegacyMeshColliderConversionSystem : BaseLegacyColliderConversionSystem<LegacyMesh>
    {
        List<Vector3> m_Vertices = new List<Vector3>(65535 / 2);

        protected override BlobAssetReference<Collider> ProduceColliderBlob(LegacyMesh shape)
        {
            if (shape.sharedMesh == null)
            {
                throw new InvalidOperationException(
                    $"No {nameof(LegacyMesh.sharedMesh)} assigned to {typeof(MeshCollider)} on {shape.name}."
                );
            }

            if (!shape.sharedMesh.IsValidForConversion(shape.gameObject))
            {
                throw new InvalidOperationException(
                    $"Mesh '{shape.sharedMesh}' assigned to {typeof(MeshCollider)} on {shape.name} is not readable. Ensure that you have enabled Read/Write on its import settings."
                );
            }

            var filter = ProduceCollisionFilter(shape);
            var material = ProduceMaterial(shape);

            // transform points into world space and back into shape space
            shape.sharedMesh.GetVertices(m_Vertices);
            var shapeFromWorld = math.inverse(
                new float4x4(new RigidTransform(shape.transform.rotation, shape.transform.position))
            );
            var pointCloud = new NativeList<float3>(shape.sharedMesh.vertexCount, Allocator.Temp);
            for (int i = 0, count = m_Vertices.Count; i < count; ++i)
            {
                var worldPt = math.mul(shape.transform.localToWorldMatrix, new float4(m_Vertices[i], 1f));
                pointCloud.Add(math.mul(shapeFromWorld, worldPt).xyz);
            }

            if (shape.convex)
                RegisterConvexColliderDeferred(shape, pointCloud, ConvexHullGenerationParameters.Default, filter, material);
            else
            {
                var triangles = new NativeArray<int>(shape.sharedMesh.triangles, Allocator.Temp);
                RegisterMeshColliderDeferred(shape, pointCloud, triangles, filter, material);
                triangles.Dispose();
            }

            pointCloud.Dispose();

            return default;
        }
    }
}
