using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FPSPlotter : MonoBehaviour
{

    public bool done = false;
    private List<int> Framerates;

    private void Awake()
    {
        Framerates = new List<int>();
    }

    void Update()
    {
        if (done)
        {
            Done();
            done = false;
        }
        else
        {
            float frameDuration = Time.unscaledDeltaTime;

            int thisFramerate = Mathf.RoundToInt(1 / frameDuration);

            Framerates.Add(thisFramerate);
        }      
    }


    private void Done()
    {
        string filePath = Application.persistentDataPath + "/framerate.csv";
        StreamWriter writer = new StreamWriter(filePath);

        for (int i = 0; i < Framerates.Count; i++)
        {
            writer.WriteLine(Framerates[i]);
        }

        writer.Flush();
        writer.Close();
    }
}
