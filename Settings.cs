namespace CMPE131Proj;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Blazored.LocalStorage.StorageOptions;

//CONST variables will be static across the whole project. Access with Settings.x
//CONST variables will also be IGNORED For saving a settings file.
//public static variables are the only ones saved to the settings file.
//If we change this file, it SHOULD automatically detect it and replace it w/ new defaults.
public static class Settings
{

    //only public vars save.
    public static string name = "Player";
   // --- Your Settings ---
    public const int CanvasWidth = 1024;
    public const int CanvasHeight = 576;
    public const bool hasAlpha = true;
    public const bool isDesyncronized = true; //better performance w animations
    public const bool willReadFrequently = false;
    
// Styling constants
    public const string CanvasBackground = "#1d1d1d";
    public const string KeyBackground = "#0dff00";
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
    public static string Save()
    {
        
        Console.WriteLine("Beginning save...");
        //write to stream to store is JS local data instead.
        MemoryStream fs = new MemoryStream() ;//File.Open(path, FileMode.OpenOrCreate);
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
        byte[] array = fs.ToArray();
        
        fs.Close();

        return Encoding.ASCII.GetString(array);

        
    }
    public static bool Load(string s)
    {
        if (s == null)
        {
            Console.WriteLine("s returned is null. First save?");
            return false;
        }
        /*
        if (!File.Exists(path))
        {
            //save defaults if the file isn't found.
            Console.WriteLine("Creating new default settings save...");
            Save();
            return;
        }*/
        Console.WriteLine("Beginning load...");

        Console.WriteLine("Converting local string to bytes.");
        byte[] bytes = Encoding.ASCII.GetBytes(s);
        Console.WriteLine("Loaded " + bytes.Length + " bytes.");
        MemoryStream fs = new MemoryStream(bytes);//File.Open(path, FileMode.Open);
        Console.WriteLine("converted to memory stream...");

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
                Console.WriteLine("Returning false for save call...");
                //Save();
                return false;

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
                Console.WriteLine("Returning false for save call...");
                //Save();
                return false;
            }
        }
        br.Close();
        fs.Close();

        return true;
    }



}