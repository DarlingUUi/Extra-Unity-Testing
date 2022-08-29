using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadTexture : MonoBehaviour
{
    public RawImage m_rimgPickedTexture;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize);
                if (texture == null)
                {
                    return;
                }
                m_rimgPickedTexture.texture = texture;

                StartCoroutine(UploadImage(texture, path));
            }
        });
    }
    Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
    public IEnumerator UploadImage(Texture2D _texture, string _path)
    {
        byte[] boundary = UnityWebRequest.GenerateBoundary();
        string contentType = string.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));
        string mimetypes = "image/" + _path.Substring(_path.Length - 3).ToLower();
        List<IMultipartFormSection> form = new List<IMultipartFormSection>();
        form.Add(new MultipartFormFileSection("image", File.ReadAllBytes(_path), "image." + _path.Substring(_path.Length - 3).ToLower(), mimetypes));

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:7000/api/chat/image", form, boundary))
        {
            www.SetRequestHeader("Content-Type", contentType);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }
}
