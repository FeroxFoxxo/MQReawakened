using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Data.Events;
using System.Xml;
using static A2m.Server.DescriptionHandler;

namespace Server.Reawakened.XMLs.Bundles.Base;
public class EventPrefabs : EventPrefabsXML, IBundledXml
{
    public string BundleName => "event_prefabs";
    public BundlePriority Priority => BundlePriority.Low;

    public ServerRwConfig RwConfig { get; set; }
    public ServerRConfig RConfig { get; set; }
    public ILogger<EventPrefabs> Logger { get; set; }

    public Dictionary<string, Dictionary<string, string>> PrefabMap { get; private set; }
    public Dictionary<string, Dictionary<string, string>> ReversePrefabMap { get; private set; }
    public Dictionary<int, string> EventIdToNameDict { get; private set; }
    public Dictionary<string, int> ReversePrefabNameToEvent { get; private set; }
    public Dictionary<int, string> EventIdToLevelId { get; set; }
    public Dictionary<KeyValuePair<int, string>, TimedSocialEventsVO> TimedSocialEvents { get; private set; }

    public EventInfo EventInfo { get; private set; }

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;
    }

    public void EditDescription(XmlDocument xml)
    {
        if (RConfig.GameVersion >= GameVersion.vEarly2014)
        {
            var node = xml.SelectNodes("/events/event");

            foreach (XmlNode events in node)
            {
                var parent = events.ParentNode;
                var eventName = new string([.. events.Attributes["name"].Value.Reverse()]);

                switch (eventName)
                {
                    case "noitarbeleC_enuJ_10R":
                    case "htnoM21_pihsrebmeM_TVE":
                    case "10_steP_CN_3102_TVE":
                    case "konhcaR_3102_TVE":
                    case "nonnaCittefnoC_3102_TVE":
                    case "adnaP_steP_3102_TVE":
                    case "20_steP_CN_3102_TVE":
                    case "wen20_steP_CN_3102_TVE":
                    case "snruteR_ekirtsrebaS_3102_TVE":
                    case "10reppaZnekcihC_3102_TVE":
                    case "10_elaS_steP_CN_3102_TVE":
                    case "neewollaH_3102_TVE":
                    case "10yadiloH_3102_TVE":
                    case "weiverPSTC_3102_TVE":
                    case "20weiverPSTC_3102_TVE":
                    case "10tePyraG_3102_TVE":
                    case "10_3102boBegnopS_3102_TVE":
                    case "20_3102boBegnopS_3102_TVE":
                    case "30_3102boBegnopS_3102_TVE":
                    case "40_3102boBegnopS_3102_TVE":
                    case "50_3102boBegnopS_3102_TVE":
                    case "hsaBananaB_4102_TVE":
                    case "onitnelaV_4102_TVE":
                    case "mooRerusaerT_4102_TVE":
                    case "yaDkoO_4102_TVE":
                    case "uneMgnippohS_4102_TVE":
                    case "slooFlirpA_4102_TVE":
                        parent.RemoveChild(events);
                        break;
                    default:
                        if (eventName.Contains("1102") || eventName.Contains("2102"))
                            parent.RemoveChild(events);
                        break;
                }
            }
        }
        else if (RConfig.GameVersion >= GameVersion.vEarly2013)
        {
            var node = xml.SelectNodes("/events/event");

            foreach (XmlNode events in node)
            {
                var parent = events.ParentNode;
                var eventName = new string([.. events.Attributes["name"].Value.Reverse()]);

                switch (eventName)
                {
                    case "noitarbeleC_enuJ_10R":
                    case "htnoM21_pihsrebmeM_TVE":
                    case "adnaPuFgnuK_2102_ORP":
                    case "yrasrevinnA_2102_TVE":
                    case "steP_2102_TVE":
                    case "semaGiniM_2102_TVE":
                    case "10_steP_CN_3102_TVE":
                    case "konhcaR_3102_TVE":
                    case "nonnaCittefnoC_3102_TVE":
                    case "adnaP_steP_3102_TVE":
                    case "20_steP_CN_3102_TVE":
                    case "wen20_steP_CN_3102_TVE":
                    case "snruteR_ekirtsrebaS_3102_TVE":
                    case "10reppaZnekcihC_3102_TVE":
                    case "10_elaS_steP_CN_3102_TVE":
                    case "neewollaH_3102_TVE":
                    case "10yadiloH_3102_TVE":
                    case "weiverPSTC_3102_TVE":
                    case "20weiverPSTC_3102_TVE":
                    case "10tePyraG_3102_TVE":
                    case "10_3102boBegnopS_3102_TVE":
                    case "20_3102boBegnopS_3102_TVE":
                    case "30_3102boBegnopS_3102_TVE":
                    case "40_3102boBegnopS_3102_TVE":
                    case "50_3102boBegnopS_3102_TVE":
                        parent.RemoveChild(events);
                        break;
                    default:
                        if (eventName.Contains("1102"))
                            parent.RemoveChild(events);
                        break;
                }
            }
        }
        else if (RConfig.GameVersion >= GameVersion.vEarly2012)
        {
            var node = xml.SelectNodes("/events/event");

            foreach (XmlNode events in node)
            {
                var parent = events.ParentNode;
                var eventName = new string([.. events.Attributes["name"].Value.Reverse()]);

                switch (eventName)
                {
                    case "noitarbeleC_enuJ_10R":
                    case "htnoM21_pihsrebmeM_TVE":
                    case "adnaPuFgnuK_2102_ORP":
                    case "yrasrevinnA_2102_TVE":
                    case "steP_2102_TVE":
                    case "semaGiniM_2102_TVE":
                        parent.RemoveChild(events);
                        break;
                    default:
                        if (eventName.Contains("1102"))
                            parent.RemoveChild(events);
                        break;
                }
            }
        }
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        PrefabMap = [];
        ReversePrefabMap = [];
        EventIdToNameDict = [];
        ReversePrefabNameToEvent = [];
        TimedSocialEvents = [];
        EventIdToLevelId = [];

        foreach (XmlNode eventPrefabXml in xmlDocument.ChildNodes)
        {
            if (eventPrefabXml.Name != "events")
                continue;

            foreach (XmlNode mEvent in eventPrefabXml.ChildNodes)
                switch (mEvent.Name)
                {
                    case "event":
                        var eventName = string.Empty;
                        var eventId = 0;
                        var levelId = string.Empty;

                        foreach (XmlAttribute mEventAttribute in mEvent.Attributes)
                            switch (mEventAttribute.Name)
                            {
                                case "name":
                                    eventName = mEventAttribute.Value.ToString();
                                    continue;
                                case "id":
                                    eventId = int.Parse(mEventAttribute.Value);
                                    continue;
                                case "LevelID":
                                    levelId = mEventAttribute.Value.ToString();
                                    continue;
                            }

                        if (string.IsNullOrEmpty(eventName))
                            continue;

                        PrefabMap[eventName] = [];
                        ReversePrefabMap[eventName] = [];
                        EventIdToNameDict[eventId] = eventName;
                        EventIdToLevelId[eventId] = levelId;

                        foreach (XmlNode eventPrefab in mEvent.ChildNodes)
                        {
                            if (eventPrefab.Name != "prefab")
                                continue;

                            var prefabName = string.Empty;
                            var replacementName = string.Empty;

                            foreach (XmlAttribute prefabAttribute in eventPrefab.Attributes)
                                switch (prefabAttribute.Name)
                                {
                                    case "name":
                                        prefabName = prefabAttribute.Value.ToString();
                                        continue;
                                    case "replacement":
                                        replacementName = prefabAttribute.Value.ToString();
                                        continue;
                                }

                            if (prefabName != string.Empty && replacementName != string.Empty)
                            {
                                PrefabMap[eventName][prefabName] = replacementName;
                                ReversePrefabMap[eventName][replacementName] = prefabName;

                                ReversePrefabNameToEvent.Remove(replacementName);
                                ReversePrefabNameToEvent.Add(replacementName, eventId);
                            }
                        }
                        break;
                    case "timedEvent":
                        var timedEventId = 0;
                        var timedEventName = string.Empty;

                        foreach (XmlAttribute mEventAttribute in mEvent.Attributes)
                            switch (mEventAttribute.Name)
                            {
                                case "timedEventId":
                                    timedEventId = int.Parse(mEventAttribute.Value);
                                    continue;
                                case "timedEventName":
                                    timedEventName = mEventAttribute.Value.ToString();
                                    continue;
                            }

                        if (timedEventId == 0 || string.IsNullOrEmpty(timedEventName))
                            continue;

                        var comb = new KeyValuePair<int, string>(timedEventId, timedEventName);

                        TimedSocialEvents[comb] = new TimedSocialEventsVO();

                        foreach (XmlNode eventPrefab in mEvent.ChildNodes)
                        {
                            if (eventPrefab.Name != "prefab")
                                continue;

                            foreach (XmlAttribute prefabAttribute in eventPrefab.Attributes)
                                switch (prefabAttribute.Name)
                                {
                                    case "activePopup":
                                        TimedSocialEvents[comb].ActivePopupName = prefabAttribute.Value.ToString();
                                        continue;
                                    case "partyPopup":
                                        TimedSocialEvents[comb].PartyPopupName = prefabAttribute.Value.ToString();
                                        continue;
                                    case "isMemberOnly":
                                        TimedSocialEvents[comb].IsMemberOnly = prefabAttribute.Value.ToString() != "0";
                                        continue;
                                }

                            foreach (XmlNode timePeriod in eventPrefab.ChildNodes)
                            {
                                if (timePeriod.Name != "timePeriod")
                                    continue;

                                var timeWindow = new TimeWindow();

                                foreach (XmlAttribute timePeriodAttribute in timePeriod.Attributes)
                                    switch (timePeriodAttribute.Name)
                                    {
                                        case "startTime":
                                            timeWindow.Start = timePeriodAttribute.Value.ToString();
                                            timeWindow.StartDateTime = DateTime.Parse(timeWindow.Start);
                                            continue;
                                        case "endTime":
                                            timeWindow.End = timePeriodAttribute.Value.ToString();
                                            timeWindow.EndDateTime = DateTime.Parse(timeWindow.End);
                                            continue;
                                    }

                                TimedSocialEvents[comb].TimeWindows.Add(timeWindow);
                            }
                        }

                        TimedSocialEvents[comb].TimeWindows =
                            [.. TimedSocialEvents[comb].TimeWindows.OrderBy(x => DateTime.Parse(x.Start).TimeOfDay.TotalSeconds)];
                        break;
                }
        }

        this.SetField<EventPrefabsXML>("_eventPrefabsMap", PrefabMap);
        this.SetField<EventPrefabsXML>("_reverseEventPrefabsMap", ReversePrefabMap);
        this.SetField<EventPrefabsXML>("_eventIDTONameDict", EventIdToNameDict);
        this.SetField<EventPrefabsXML>("_reversePrefabNameToEvent", ReversePrefabNameToEvent);
        this.SetField<EventPrefabsXML>("_timedSocialEventAssetNames", TimedSocialEvents.ToDictionary(x => x.Key.Key, x => x.Value));
    }

    private class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public bool HasEventAd { get; set; }
        public bool HasEventPopupIcon { get; set; }
        public bool HasEventPopupMenu { get; set; }
    }

    public void FinalizeBundle()
    {
        GameFlow.EventPrefabsXML = this;

        var defaultEventName = string.Empty;
        var defaultTimedEvent = string.Empty;

        if (!string.IsNullOrEmpty(RwConfig.CurrentEventOverride))
            defaultEventName = RwConfig.CurrentEventOverride;
        else if (RConfig.CurrentEvent.TryGetValue(RConfig.GameVersion, out var defaultEventString))
            defaultEventName = new string([.. defaultEventString.Reverse()]);

        if (!string.IsNullOrEmpty(RwConfig.CurrentTimedEventOverride))
            defaultTimedEvent = RwConfig.CurrentTimedEventOverride;
        else if (RConfig.CurrentTimedEvent.TryGetValue(RConfig.GameVersion, out var defaultTimedString))
            defaultTimedEvent = new string([.. defaultTimedString.Reverse()]);

        var reversedDict = EventIdToNameDict.ToDictionary(
            x => x.Value,
            x =>
            {
                var hasEventAd = PrefabMap[x.Value].ContainsKey("Event_Ads");
                var hasEventPopupIcon = PrefabMap[x.Value].ContainsKey("EventPopupIcon");
                var hasEventPopupMenu = PrefabMap[x.Value].ContainsKey("EventPopupMenu");

                return new Event()
                {
                    EventId = x.Key,
                    EventName = x.Value,
                    HasEventAd = hasEventAd,
                    HasEventPopupIcon = hasEventPopupIcon,
                    HasEventPopupMenu = hasEventPopupMenu
                };
            }
        );

        if (!reversedDict.ContainsKey(defaultEventName))
        {
            var newEventName = PrefabMap.MaxBy(x => x.Value.Count).Key;
            Logger.LogError("Could not find event for: '{oldName}'. Defaulting to: '{newName}'...", defaultEventName, newEventName);
            defaultEventName = newEventName;
        }

        if (!TimedSocialEvents.Any(x => x.Key.Value == defaultTimedEvent))
        {
            var newTimedEvent = TimedSocialEvents.Count > 0 ?
                TimedSocialEvents.MaxBy(x => x.Value.TimeWindows.Count).Key.Value :
                string.Empty;
            Logger.LogError("Could not find event for: '{oldName}'. Defaulting to: '{newName}'...", defaultTimedEvent, newTimedEvent);
            defaultTimedEvent = newTimedEvent;
        }

        var defaultEvent = reversedDict[defaultEventName];

        EventInfo = new EventInfo()
        {
            DisplayAd = defaultEvent.HasEventAd,
            DisplayAutoPopup = defaultEvent.HasEventPopupIcon || defaultEvent.HasEventPopupMenu,
            DisplayPopupIcon = defaultEvent.HasEventPopupIcon,
            DisplayPopupMenu = defaultEvent.HasEventPopupMenu,

            EventId = defaultEvent.EventId,

            SecondaryEvents = [.. reversedDict.Values
                .Where(x => x.EventId != defaultEvent.EventId)
                .Select(x => new SecondaryEventInfo()
                {
                    EventId = x.EventId,
                    DisplayAd = x.HasEventAd,
                    DisplayIcon = x.HasEventPopupIcon
                })],

            TimedEventName = defaultTimedEvent,
            GameVersion = RConfig.GameVersion
        };
    }
}
