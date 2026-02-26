namespace CMPE131Proj;
using System;
using Blazorex;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;

public class Settings
{

    //only public vars save.

    //canvas settings
    public int CanvasWidth = 1024;
    public int CanvasHeight = 768;
    public bool hasAlpha = false;
    public bool isDesyncronized = true; //better performance w animations
    public bool willReadFrequently = false;
    
// Styling constants
    public string CanvasBackground = "#1d1d1d";
    public string KeyBackground = "#2c3e50";
    public string KeyBorder = "#34495e";
    public string KeyText = "#ecf0f1";
    public string KeyFont = "bold 18px 'Segoe UI', Arial, sans-serif";

    //use private for class objects that we don't want to save (serialize)
    private static string path = "./settings.xml";
    
    public void Save()
    {
        Console.WriteLine("Saving to path: " + Path.GetFullPath(path) );
        XmlSerializer serializer = new XmlSerializer(typeof(Settings));
        using (StringWriter writer = new StringWriter())
        {
            serializer.Serialize(writer, this);
            string xmlOutput = writer.ToString();
             File.WriteAllText(path, xmlOutput);
        }

    }
    public static Settings Load()
    {
        try{
            string readfromfile = File.ReadAllText(path);
            Console.WriteLine("Loading save from: " + Path.GetFullPath(path) );
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            using (StringReader reader = new StringReader(readfromfile))
            {
            // The Deserialize method returns an object, which needs to be cast to the correct type
                Settings deseralized = (Settings)serializer.Deserialize(reader);
                return deseralized;
            }

        }
        catch (DirectoryNotFoundException)
        {
            //save default settings.
            Settings s = new Settings();
            s.Save();
            return s;

        }
        catch (IOException)
        {
            Settings s = new Settings();
            s.Save();
            return s;
        }
        

        
    }
}