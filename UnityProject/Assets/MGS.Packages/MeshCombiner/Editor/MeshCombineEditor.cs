/*************************************************************************
 *  Copyright © 2021 Mogoson. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  MeshCombineEditor.cs
 *  Description  :  Draw the extend editor window and combine meshes.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  0.1.0
 *  Date         :  3/9/2018
 *  Description  :  Initial development version.
 *************************************************************************/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MGS.MeshCombiner
{
    public class MeshCombineEditor : ScriptableWizard
    {
        #region Field and Property 
        [Tooltip("Root gameobject of meshes.")]
        public GameObject meshesRoot;

        [Tooltip("Gameobject to save new combine mesh.")]
        public GameObject meshSave;
        #endregion

        #region Private Method
        [MenuItem("Tool/Mesh Combiner &M")]
        private static void ShowEditor()
        {
            DisplayWizard("Mesh Combiner", typeof(MeshCombineEditor), "Combine");
        }

        private void OnWizardUpdate()
        {
            if (meshesRoot && meshSave)
            {
                isValid = true;
            }
            else
            {
                isValid = false;
            }
        }

        private void OnWizardCreate()
        {
            var meshSavePath = EditorUtility.SaveFilePanelInProject(
                "Save New Combine Mesh",
                "NewCombineMesh",
                "asset",
                "Enter a file name to save the new combine mesh.");

            if (string.IsNullOrEmpty(meshSavePath))
            {
                return;
            }

            Selection.activeObject = CombineMeshTo(meshesRoot, meshSave, meshSavePath);
        }

        private Mesh CombineMeshTo(GameObject meshesRoot, GameObject meshSave, string meshSavePath)
        {
            var meshFilters = meshesRoot.GetComponentsInChildren<MeshFilter>();
            var combines = new CombineInstance[meshFilters.Length];
            var materialList = new List<Material>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combines[i].mesh = meshFilters[i].sharedMesh;
                combines[i].transform = Matrix4x4.TRS(meshFilters[i].transform.position - meshesRoot.transform.position,
                    meshFilters[i].transform.rotation, meshFilters[i].transform.lossyScale);
                var materials = meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials;
                foreach (var material in materials)
                {
                    materialList.Add(material);
                }
            }
            var newMesh = new Mesh();
            newMesh.CombineMeshes(combines, false);

#if !UNITY_5_5_OR_NEWER
            //Mesh.Optimize was removed in version 5.5.2p4.
            newMesh.Optimize();
#endif
            var filter = meshSave.GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = meshSave.AddComponent<MeshFilter>();
            }

            var renderer = meshSave.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = meshSave.AddComponent<MeshRenderer>();
            }

            var collider = meshSave.GetComponent<MeshCollider>();
            if (collider == null)
            {
                collider = meshSave.AddComponent<MeshCollider>();
            }

            filter.sharedMesh = newMesh;
            collider.sharedMesh = newMesh;
            renderer.sharedMaterials = materialList.ToArray();

            AssetDatabase.CreateAsset(newMesh, meshSavePath);
            AssetDatabase.Refresh();
            return newMesh;
        }
        #endregion
    }
}