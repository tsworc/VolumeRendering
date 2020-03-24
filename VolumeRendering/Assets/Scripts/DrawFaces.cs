using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawFaces : MonoBehaviour
{
    public Material materialFront;
    public Material materialBack;
    public Material materialVolume;
    Material mrMaterial;
    RenderTexture rtFront;
    RenderTexture rtback;
    public Vector3 offset;
    public Vector3 boundsMax = new Vector3(1,1,1);
    public Vector3 boundsMin  = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        var buffer = Graphics.activeColorBuffer;
        //rtFront = new RenderTexture(Screen.width, Screen.height, 32);
        //rtback = new RenderTexture(Screen.width, Screen.height, 32);
    }

    // Update is called once per frame
    /*void Update()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        //Graphics.SetRenderTarget(rt);
        //GL.Clear(true, true, Color.white);
        m.SetPass(0);
        Graphics.DrawMeshNow(mf.sharedMesh, transform.localToWorldMatrix);
    }*/
    Vector4 tov4(Vector3 v,float w) { return new Vector4(v.x, v.y, v.z, w); }
    private void OnRenderObject()
    {
        if (rtFront == null) rtFront = new RenderTexture(RenderTexture.active);
        if (rtback == null) rtback = new RenderTexture(RenderTexture.active);
        if (mrMaterial == null) mrMaterial = new Material(materialVolume);
        MeshFilter mf = GetComponent<MeshFilter>();
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = rtFront;
        GL.Clear(true, true, Color.black);
        materialFront.SetVector("_Scale", tov4(transform.localScale,0));
        materialFront.SetPass(0);
        Graphics.DrawMeshNow(mf.sharedMesh, transform.localToWorldMatrix);
        RenderTexture.active = rtback;
        GL.Clear(true, true, Color.black);
        materialBack.SetVector("_Scale", tov4(transform.localScale, 0));
        materialBack.SetPass(0);
        Graphics.DrawMeshNow(mf.sharedMesh, transform.localToWorldMatrix);
        RenderTexture.active = active;
        mrMaterial.SetTexture("FrontS", rtFront);
        mrMaterial.SetTexture("BackS", rtback);
        mrMaterial.SetVector("_offset", new Vector4(offset.x, offset.y, offset.z, 0));
        mrMaterial.SetVector("boundsMax", tov4(boundsMax, 0));
        mrMaterial.SetVector("boundsMin", tov4(boundsMin, 0));
        LoadVolumeScript lvs = GetComponent<LoadVolumeScript>();
        mrMaterial.SetTexture("VolumeS", lvs.GetVolume());
        //MeshRenderer mr = GetComponent<MeshRenderer>();
        //mr.material = mrMaterial;
        mrMaterial.SetPass(0);
        Graphics.DrawMeshNow(mf.sharedMesh, transform.localToWorldMatrix);
        /*Graphics.SetRenderTarget(null);
        GL.Clear(true, true, Color.black);
        materialVolume.SetPass(0);
        Graphics.DrawMeshNow(mf.sharedMesh, transform.localToWorldMatrix);*/
    }
    /*public void OnPostRender()
    {
        GL.PushMatrix();
        GL.LoadOrtho();
        MeshFilter mf = GetComponent<MeshFilter>();
        //Graphics.SetRenderTarget(rt);
        //GL.Clear(true, true, Color.white);
        m.SetPass(0);
        Graphics.DrawMeshNow(mf.sharedMesh, transform.localToWorldMatrix);
        GL.Begin(GL.QUADS);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(1, 0, 0);
        GL.Vertex3(1, 1, 0);
        GL.Vertex3(0, 1, 0);
        GL.End();

        GL.PopMatrix();
    }*/
}
