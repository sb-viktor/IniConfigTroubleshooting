using System.IO;
using System.Reflection;

namespace IniConfigTroubleshooting.Resources;

internal class ResourceLoader
{
    const string SampleFilesPrefix = "IniConfigTroubleshooting.Resources.SampleFiles.";

    internal static string LoadSampleFile(string fileName)
    {
        Stream stream = typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(SampleFilesPrefix + fileName);

        if (stream == null)
            return string.Empty;

        using (stream)
        using (StreamReader reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}