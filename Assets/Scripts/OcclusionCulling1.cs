using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OcclusionCulling : MonoBehaviour
{
    private List<GameObject> visibleObjects;
    //Define a structure for BVH nodes
    public class BVHNode
    {
        public Bounds bounds;
        public List<GameObject> objects;
        public BVHNode leftChild;
        public BVHNode rightChild;

        public BVHNode(Bounds b)
        {
            bounds = b;
            objects = new List<GameObject>();
            leftChild = null;
            rightChild = null;
        }
    }

    public BVHNode root;
    void Start()
    {
        //Build the initial BVH from objects in scene
        List<GameObject> sceneObjects = GetAllSceneObjectsWithTag("StaticOcclude");
        Bounds combinedBounds = CalculateCombinedBounds(sceneObjects);
        root = BuildBVH(sceneObjects, combinedBounds);

    }

    // Update is called once per frame
    void Update()
    {
        //Get visible objects from BVH
        visibleObjects = new List<GameObject>();
        GetVisibleObjects(root);
        // Render only the visible objects
        foreach (GameObject obj in root.objects)
        {
            bool isVisible = visibleObjects.Contains(obj);

            // Toggle Renderer component based on visibility
            obj.GetComponent<Renderer>().enabled = isVisible;

        }
    }

    //Return a list of visible objects utilizing the BVH tree
    void GetVisibleObjects(BVHNode node)
    {
        if (node == null)
        {
            return;
        }

        if (node.leftChild == null && node.rightChild == null)
        {
            //If leaf node, check visibility for the object in the node
            if (IsObjectVisible(node.objects.First()))
            {
                visibleObjects.Add(node.objects.First());
            }
        }
        else
        {
            //If not a leaf node, check the visibility for the entire node
            if (IsNodeVisible(node))
            {
                GetVisibleObjects(node.leftChild);
                GetVisibleObjects(node.rightChild);

            }
        }

    }

    //Check if a node is within the camera frustum
    bool IsNodeVisible(BVHNode node)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, node.bounds);
        }
        return false;
    }

    //Check if the object is visible
    bool IsObjectVisible(GameObject obj)
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            Bounds bounds = CalculateObjectBounds(obj);
            Plane[] frustumPlane = GeometryUtility.CalculateFrustumPlanes(mainCamera);

            if (!GeometryUtility.TestPlanesAABB(frustumPlane, bounds))
            {
                return false; // Object is outside the camera frustum
            }

            //Offset so that rays don't hit a point shared by two objects
            float offset = 0.001f; 

            Vector3[] testPoints = {
                bounds.center,
                
                //Extending the center
                bounds.center + new Vector3(bounds.extents.x - offset, 0, 0), // Offset along the x-axis
                bounds.center + new Vector3(-bounds.extents.x + offset, 0, 0), // Offset along the negative x-axis
                bounds.center + new Vector3(0, bounds.extents.y - offset, 0), // Offset along the y-axis
                bounds.center + new Vector3(0, -bounds.extents.y + offset, 0), // Offset along the negative y-axis
                bounds.center + new Vector3(0, 0, bounds.extents.z - offset), // Offset along the z-axis
                bounds.center + new Vector3(0, 0, -bounds.extents.z + offset), // Offset along the negative z-axis
                
                //Front
                new Vector3(bounds.min.x + offset, bounds.max.y - offset, bounds.min.z + offset), // Top-front-left corner, top left
                new Vector3(bounds.max.x - offset, bounds.max.y - offset, bounds.min.z + offset), // Top-front-right corner, top right
                new Vector3(bounds.min.x + offset, bounds.min.y + offset, bounds.min.z + offset), // Bottom-front-left corner, bottom left
                new Vector3(bounds.max.x - offset, bounds.min.y + offset, bounds.min.z + offset), // Bottom-front-right corner, bottom right
                
                //Back
                new Vector3(bounds.min.x + offset, bounds.max.y - offset, bounds.max.z - offset), // Top-back-left corner, top left
                new Vector3(bounds.max.x - offset, bounds.max.y - offset, bounds.max.z - offset), // Top-back-right corner, top right
                new Vector3(bounds.min.x + offset, bounds.min.y + offset, bounds.max.z - offset), // Bottom-back-left corner, bottom left
                new Vector3(bounds.max.x - offset, bounds.min.y + offset, bounds.max.z - offset) // Bottom-back-right corner, bottom right
                
             };

            bool rayHitTarget = false;

            foreach (Vector3 testPoint in testPoints)
            {

                Vector3 direction = testPoint - mainCamera.transform.position;
                RaycastHit hit;

                if (Physics.Raycast(mainCamera.transform.position, direction, out hit))
                {
                    if (hit.collider.gameObject == obj)
                    {
                        rayHitTarget = true;
                        break;
                    }
                }
            }

            return rayHitTarget;
        }

        return false; // Camera not found or other error
    }

    //Build the BVH
    BVHNode BuildBVH(List<GameObject> objects, Bounds bounds)
    {
        BVHNode node = new BVHNode(bounds);
        node.objects.AddRange(objects);

        if (objects.Count <= 1)
        {
            //Add objects to leaf node if count is 1 or less
            node.objects.AddRange(objects);
            return node;
        }

        List<GameObject> leftObjects, rightObjects;
        PartitionObjects(objects, out leftObjects, out rightObjects);

        node.leftChild = BuildBVH(leftObjects, CalculateCombinedBounds(leftObjects));
        node.rightChild = BuildBVH(rightObjects, CalculateCombinedBounds(rightObjects));

        return node;
    }

    //Get all objects in a scene with specific tag
    List<GameObject> GetAllSceneObjectsWithTag(string tag)
    {
        List<GameObject> sceneObjects = new List<GameObject>();

        // Get all active scenes in the build
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            // Find all GameObjects in the scene with the specified tag and add them to the list
            GameObject[] objectsWithTag = scene.GetRootGameObjects().Where(obj => obj.CompareTag(tag)).ToArray();
            sceneObjects.AddRange(objectsWithTag);

            foreach (GameObject obj in objectsWithTag)
            {
                GetAllChildObjects(obj.transform, tag, ref sceneObjects);
            }
        }

        return sceneObjects;
    }

    //Recursively get child objects of a parent object with a specific tag
    void GetAllChildObjects(Transform parent, string tag, ref List<GameObject> objectsList)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                objectsList.Add(child.gameObject);
            }
            GetAllChildObjects(child, tag, ref objectsList);
        }
    }

    //Calculate combined bounds of multiple objects
    Bounds CalculateCombinedBounds(List<GameObject> objects)
    {
        if (objects == null || objects.Count == 0)
        {
            // If no objects or null list is provided, return default bounds
            return new Bounds(Vector3.zero, Vector3.one);
        }

        // Initialize bounds using the first object's position
        Bounds combinedBounds = objects.First().GetComponent<Renderer>().bounds;

        // Expand bounds to encapsulate all objects
        foreach (GameObject obj in objects)
        {
            // Extend bounds by encapsulating the object's bounds
            Bounds objBounds = obj.GetComponent<Renderer>().bounds;
            combinedBounds.Encapsulate(objBounds.min);
            combinedBounds.Encapsulate(objBounds.max);
        }

        return combinedBounds;
    }

    //Partition objects based on their positions along an axis
    void PartitionObjects(List<GameObject> objects, out List<GameObject> left, out List<GameObject> right)
    {
        left = new List<GameObject>();
        right = new List<GameObject>();

        // Find the axis with the largest spread
        float xMin = float.MaxValue, xMax = float.MinValue;
        float yMin = float.MaxValue, yMax = float.MinValue;
        float zMin = float.MaxValue, zMax = float.MinValue;

        //Calculate the minimum and maximum values along each axis
        foreach (GameObject obj in objects)
        {
            Vector3 position = obj.transform.position;
            xMin = Mathf.Min(xMin, position.x);
            xMax = Mathf.Max(xMax, position.x);
            yMin = Mathf.Min(yMin, position.y);
            yMax = Mathf.Max(yMax, position.y);
            zMin = Mathf.Min(zMin, position.z);
            zMax = Mathf.Max(zMax, position.z);
        }

        //Calculate the spread along each axis
        float xSpread = xMax - xMin;
        float ySpread = yMax - yMin;
        float zSpread = zMax - zMin;

        //Determine the axis with the maximum spread and split objects along that axis
        if (xSpread >= ySpread && xSpread >= zSpread)
            SplitAlongAxis(objects, obj => obj.transform.position.x, out left, out right);
        else if (ySpread >= xSpread && ySpread >= zSpread)
            SplitAlongAxis(objects, obj => obj.transform.position.y, out left, out right);
        else
            SplitAlongAxis(objects, obj => obj.transform.position.z, out left, out right);
    }

    //Split objects along a specified axis
    void SplitAlongAxis(List<GameObject> objects, Func<GameObject, float> axisSelector, out List<GameObject> left, out List<GameObject> right)
    {
        left = new List<GameObject>();
        right = new List<GameObject>();

        //Sort objects based on the specified axis
        objects.Sort((a, b) => axisSelector(a).CompareTo(axisSelector(b)));
        int midpoint = objects.Count / 2;

        //Divide objects into left and right based on sorted order
        for (int i = 0; i < objects.Count; i++)
        {
            if (i < midpoint)
                left.Add(objects[i]);
            else
                right.Add(objects[i]);
        }
    }

    //Calculate bounds of an object
    Bounds CalculateObjectBounds(GameObject obj)
    {
        Collider renderer = obj.GetComponent<Collider>();
        if (renderer != null)
        {
            return renderer.bounds;
        }
        // No renderer, so return a default bounds object
        return new Bounds(obj.transform.position, Vector3.one);
    }
}
