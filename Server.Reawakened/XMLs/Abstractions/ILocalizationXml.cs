using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Reawakened.XMLs.Abstractions;
public interface ILocalizationXml : IBundledXml
{
    string LocalizationName { get; }

    void LoadLocalization(string xml);
}
