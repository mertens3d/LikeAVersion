using mertens3d.LikeAVersion.FileTools;
using mertensd.LikeAVersion.Build;
using mertense3d.LikeAVersion.Models;
using System;
using System.IO;

namespace mertensd.LikeAVersion.FileTools
{
    public class SettingsFile : CommonBase_a

    {
        public SettingsFile(HeartBeatHub hub) : base(hub)
        {
        }

        public XmlSettings ReadFromXml(FileInfo targetXml)
        {
            XmlSettings toReturn = null;

            var targetExists = targetXml.Exists;

            Hub.Reporter.ToHuman("File : " + targetXml);
            Hub.Reporter.ToHuman("Exists? : " + targetExists);

            if (targetExists)
            {
                string xmlInputData = File.ReadAllText(targetXml.FullName);
                try
                {
                    Serializer ser = new Serializer();
                    toReturn = ser.Deserialize<XmlSettings>(xmlInputData);
                }
                catch (Exception ex)
                {
                    Hub.Reporter.Error(ex.Message);
                }
            }
            else
            {
                Hub.Logger.Error("Did not find settings file: " + targetXml.FullName);
            }

            return toReturn;
        }
    }
}