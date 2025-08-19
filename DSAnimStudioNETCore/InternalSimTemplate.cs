using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DSAnimStudio
{
    public class InternalSimTemplate
    {
        private static object _lock_Instance = new object();
        private static InternalSimTemplate _instance = null;
        public static InternalSimTemplate Instance
        {
            get
            {
                InternalSimTemplate _instanceCopy = null;
                lock (_lock_Instance)
                {
                    if (_instance == null)
                    {
                        var xml = new XmlDocument();
                        //xml.Load($"{Main.Directory}\\Res\\InternalSimulationTemplate_DoNotEdit.xml");
                        xml.LoadXml(Main.GetEmbeddedResourceText("/EmbRes/InternalSimulationTemplate.xml"));
                        _instance = new InternalSimTemplate(xml);
                    }
                    _instanceCopy = _instance;
                }
                
                return _instanceCopy;
            }
        }

        public class FieldOverride
        {
            public readonly SoulsAssetPipeline.SoulsGames Game;
            public TAE.Template.ParamTypes? OverrideValType = null;
            public int? OverrideValOffset = null;
            public FieldOverride(XmlNode node)
            {
                Game = (SoulsAssetPipeline.SoulsGames)Enum.Parse(typeof(SoulsAssetPipeline.SoulsGames), node.Attributes["game"].InnerText);

                OverrideValType = null;
                if (node.Attributes["type"] != null)
                    OverrideValType = (TAE.Template.ParamTypes)Enum.Parse(typeof(TAE.Template.ParamTypes), node.Attributes["type"].InnerText);

                OverrideValOffset = null;
                if (node.Attributes["offset"] != null)
                {
                    var offsetText = node.Attributes["offset"]?.InnerText?.ToLower();
                    if (offsetText != null)
                    {
                        if (offsetText.StartsWith("0x"))
                            OverrideValOffset = int.Parse(offsetText.Substring(2), System.Globalization.NumberStyles.HexNumber);
                        else
                            OverrideValOffset = int.Parse(offsetText, System.Globalization.NumberStyles.HexNumber);
                    }
                }
            }
        }

        public class Field
        {
            public readonly string Name;
            public TAE.Template.ParamTypes DefaultValType;
            public int DefaultValOffset;
            public Dictionary<SoulsAssetPipeline.SoulsGames, FieldOverride> Overrides = new Dictionary<SoulsAssetPipeline.SoulsGames, FieldOverride>();

            public List<SoulsAssetPipeline.SoulsGames> NotInGames = new List<SoulsAssetPipeline.SoulsGames>();
            public List<SoulsAssetPipeline.SoulsGames> OnlyInGames = new List<SoulsAssetPipeline.SoulsGames>();

            public Field(XmlNode node, int eventIDForDebug)
            {
                Name = node.Attributes["name"].InnerText;
                DefaultValType = (TAE.Template.ParamTypes)Enum.Parse(typeof(TAE.Template.ParamTypes), node.Attributes["type"].InnerText);

                var offsetText = node.Attributes["offset"]?.InnerText?.ToLower();
                if (offsetText != null)
                {
                    if (offsetText.StartsWith("0x"))
                        DefaultValOffset = int.Parse(offsetText.Substring(2), System.Globalization.NumberStyles.HexNumber);
                    else
                        DefaultValOffset = int.Parse(offsetText, System.Globalization.NumberStyles.HexNumber);
                }

                var notInText = node.Attributes["not_in"]?.InnerText;
                if (notInText != null)
                {
                    NotInGames = notInText.Split(",")
                        .Select(x => (SoulsAssetPipeline.SoulsGames)Enum.Parse(typeof(SoulsAssetPipeline.SoulsGames), x))
                        .ToList();
                }

                var onlyInText = node.Attributes["only_in"]?.InnerText;
                if (onlyInText != null)
                {
                    OnlyInGames = onlyInText.Split(",")
                        .Select(x => (SoulsAssetPipeline.SoulsGames)Enum.Parse(typeof(SoulsAssetPipeline.SoulsGames), x))
                        .ToList();
                }

                Overrides.Clear();
                foreach (XmlNode overrideNode in node.SelectNodes("override"))
                {
                    var newFieldOverride = new FieldOverride(overrideNode);
                    if (Overrides.ContainsKey(newFieldOverride.Game))
                        throw new Exception($"{nameof(InternalSimTemplate)} -> Event Type '{eventIDForDebug}' field '{Name}' has more than one override for game '{newFieldOverride.Game}'.");
                    Overrides.Add(newFieldOverride.Game, newFieldOverride);
                }
            }
        }
        public class Event
        {
            public readonly int ID;
            public string Name;
            public Dictionary<string, Field> Fields = new Dictionary<string, Field>();

            public List<SoulsAssetPipeline.SoulsGames> NotInGames = new List<SoulsAssetPipeline.SoulsGames>();
            public List<SoulsAssetPipeline.SoulsGames> OnlyInGames = new List<SoulsAssetPipeline.SoulsGames>();

            public Event(XmlNode node)
            {
                ID = int.Parse(node.Attributes["id"].InnerText);
                Name = node.Attributes["name"].InnerText;

                var notInText = node.Attributes["not_in"]?.InnerText;
                if (notInText != null)
                {
                    NotInGames = notInText.Split(",")
                        .Select(x => (SoulsAssetPipeline.SoulsGames)Enum.Parse(typeof(SoulsAssetPipeline.SoulsGames), x))
                        .ToList();
                }

                var onlyInText = node.Attributes["only_in"]?.InnerText;
                if (onlyInText != null)
                {
                    OnlyInGames = onlyInText.Split(",")
                        .Select(x => (SoulsAssetPipeline.SoulsGames)Enum.Parse(typeof(SoulsAssetPipeline.SoulsGames), x))
                        .ToList();
                }

                Fields.Clear();
                foreach (XmlNode fieldNode in node.SelectNodes("field"))
                {
                    var newField = new Field(fieldNode, ID);
                    if (Fields.ContainsKey(newField.Name))
                        throw new Exception($"{nameof(InternalSimTemplate)} -> Event Type '{ID}' has more than one field entry with name '{newField.Name}'.");
                    Fields.Add(newField.Name, newField);
                }
            }
        }

        public Dictionary<int, Event> Events = new Dictionary<int, Event>();

        private InternalSimTemplate(XmlDocument xml)
        {
            XmlNode templateNode = xml.SelectSingleNode("internal_simulation_template");

            Events.Clear();
            var allEventNodes = templateNode.SelectNodes("event");

            foreach (XmlNode eventNode in allEventNodes)
            {
                var newEvent = new Event(eventNode);
                if (Events.ContainsKey(newEvent.ID))
                    throw new Exception($"{nameof(InternalSimTemplate)} has more than one event entry with a type ID of '{newEvent.ID}'.");
                Events.Add(newEvent.ID, newEvent);
            }
        }
    }
}
