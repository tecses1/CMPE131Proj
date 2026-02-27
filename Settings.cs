namespace CMPE131Proj;
using System;
using Blazorex;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Reflection;

//CONST variables will be static across the whole project. Access with Settings.x
//CONST variables will also be IGNORED For saving a settings file.
//public static variables are the only ones saved to the settings file.
//If we change this file, it SHOULD automatically detect it and replace it w/ new defaults.
public static class Settings
{

    //only public vars save.

   // --- Your Settings ---
    public const int CanvasWidth = 1024;
    public const int CanvasHeight = 768;
    public const bool hasAlpha = false;
    public const bool isDesyncronized = true; //better performance w animations
    public const bool willReadFrequently = false;
    
// Styling constants
    public const string CanvasBackground = "#1d1d1d";
    public const string KeyBackground = "#2c3e50";
    public const string KeyBorder = "#34495e";
    public const string KeyText = "#ecf0f1";
    public const string KeyFont = "bold 18px 'Segoe UI', Arial, sans-serif";

    //use private for class objects that we don't want to save (serialize)
    private static string path = "./settings";
    /// <summary>
    /// Grabs all static properties via Reflection and saves them to a JSON file.
    /// </summary>
    /// 
    public static bool isConst(FieldInfo fi)
    {
        return fi.IsLiteral && !fi.IsInitOnly;
    }
    public static void Save()
    {
        Console.WriteLine("Beginning save...");
        Stream fs = File.Open(path, FileMode.OpenOrCreate);
        BinaryWriter bf = new BinaryWriter(fs);        
        FieldInfo[] staticFields = typeof(Settings).GetFields(BindingFlags.Static | BindingFlags.Public);
        Console.WriteLine("Found field info, it is " + staticFields.Length + " many");
        foreach (FieldInfo fi in staticFields)
        {  
            string fieldName = fi.Name;
            bf.Write(fieldName);

            if (isConst(fi))
            {
                bf.Write("skip_const");
                Console.WriteLine("Skipping const value: " + fieldName);
                continue;
            }

            object generic = fi.GetValue(null);

            Console.WriteLine("Saving: " +fieldName + " = " + generic.ToString());
            
            switch (Type.GetTypeCode(generic.GetType())){
                case TypeCode.Boolean: bf.Write((bool)generic); break;
                case TypeCode.Int32: bf.Write((int)generic); break;
                case TypeCode.Double: bf.Write((double)generic); break;
                case TypeCode.String: bf.Write((string)generic); break;
            }
            
        }
        bf.Close();
        fs.Close();
    }
    public static void Load()
    {
        if (!File.Exists(path))
        {
            //save defaults if the file isn't found.
            Console.WriteLine("Creating new default settings save...");
            Save();
            return;
        }
        Console.WriteLine("Beginning load...");
        Stream fs = File.Open(path, FileMode.Open);
        BinaryReader br = new BinaryReader(fs); 
        FieldInfo[] staticFields = typeof(Settings).GetFields(BindingFlags.Static | BindingFlags.Public);
        Console.WriteLine("Found field info, it is " + staticFields.Length + " many");
        foreach (FieldInfo fi in staticFields)
        {
            string fieldName = fi.Name;
            string readName = br.ReadString();
            if (! fieldName.Equals(readName))
            {
                Console.WriteLine("[ERROR] Settings file mismatch (" + fieldName + "," + readName + "). Saving new.");
                br.Close();
                fs.Close();
                Console.WriteLine("Creating new default settings save...");
                Save();
                return;

            }
            if (isConst(fi))
            {
                Console.WriteLine("Skipping const value: " + fieldName);
                br.ReadString();
                continue;
            }
            object generic = fi.GetValue(null);
            
            try{
                switch (Type.GetTypeCode(generic.GetType())){
                    case TypeCode.Boolean: fi.SetValue(null,br.ReadBoolean()); break;
                    case TypeCode.Int32: fi.SetValue(null,br.ReadInt32()); break;
                    case TypeCode.Double: fi.SetValue(null,br.ReadDouble()); break;
                    case TypeCode.String: fi.SetValue(null,br.ReadString()); break;
                }
                

                Console.WriteLine("Loaded: " + fieldName + " = " + fi.GetValue(null).ToString());
            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("[ERROR] EOF prematurely. IS the file corrupted or missing?");
                br.Close();
                fs.Close();
                Save();
                return;
            }
        }
        br.Close();
        fs.Close();
    }



}